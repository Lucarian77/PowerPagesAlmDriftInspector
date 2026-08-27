using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using PowerPagesAlmDriftInspector.Models;

namespace PowerPagesAlmDriftInspector.Services
{
    /// <summary>
    /// Reads Power Pages websites and site settings from both supported storage models.
    ///
    /// Enhanced:
    ///   mspp_website
    ///   mspp_sitesetting
    ///
    /// Standard / legacy:
    ///   adx_website
    ///   adx_sitesetting
    ///
    /// The model is tracked per website, not per environment, because standard and
    /// enhanced Power Pages sites can coexist in the same Dataverse environment.
    /// </summary>
    public class DataverseRetrievalService
    {
        private const int DataversePageSize = 5000;

        private static readonly PowerPagesSchema EnhancedSchema = new PowerPagesSchema(
            PowerPagesDataModel.Enhanced,
            "Enhanced",
            "mspp_website",
            "mspp_websiteid",
            "mspp_name",
            "mspp_partialurl",
            "mspp_primarydomainname",
            "mspp_sitesetting",
            "mspp_sitesettingid",
            "mspp_name",
            "mspp_value",
            "mspp_websiteid");

        private static readonly PowerPagesSchema StandardSchema = new PowerPagesSchema(
            PowerPagesDataModel.Standard,
            "Standard",
            "adx_website",
            "adx_websiteid",
            "adx_name",
            "adx_partialurl",
            "adx_primarydomainname",
            "adx_sitesetting",
            "adx_sitesettingid",
            "adx_name",
            "adx_value",
            "adx_websiteid");

        private readonly ConditionalWeakTable<IOrganizationService, ConcurrentDictionary<Guid, PowerPagesDataModel>>
            _websiteModels =
                new ConditionalWeakTable<IOrganizationService, ConcurrentDictionary<Guid, PowerPagesDataModel>>();

        // TC3: keep a small, process-local history of website retrieval batches.
        // The UI layer can use this authoritative data instead of trying to rediscover
        // WebsiteModel instances from private fields on the existing validated control.
        // A start-sequence is assigned before retrieval begins so Source/Target ordering
        // remains deterministic even if their Dataverse calls complete at different times.
        private static readonly object WebsiteBatchSync = new object();
        private static readonly List<WebsiteRetrievalBatch> RecentWebsiteBatches =
            new List<WebsiteRetrievalBatch>();
        private static long _websiteRetrievalSequence;
        private const int MaxRecentWebsiteBatches = 8;

        /// <summary>
        /// Non-fatal retrieval warning from the most recent website load.
        /// The current 1.2026.1.3 UI does not need to consume this property.
        /// It is available for a later UI enhancement if desired.
        /// </summary>
        public string LastCompatibilityWarning { get; private set; }

        public List<WebsiteModel> GetWebsites(IOrganizationService service)
        {
            return RetrieveWebsites(service);
        }

        public List<WebsiteModel> LoadWebsites(IOrganizationService service)
        {
            return RetrieveWebsites(service);
        }

        public List<WebsiteModel> GetPowerPagesWebsites(IOrganizationService service)
        {
            return RetrieveWebsites(service);
        }

        public List<WebsiteModel> RetrievePowerPagesWebsites(IOrganizationService service)
        {
            return RetrieveWebsites(service);
        }

        /// <summary>
        /// Returns active websites from both enhanced and standard Power Pages models.
        /// If the same active record id appears in both models, enhanced wins.
        /// </summary>
        public List<WebsiteModel> RetrieveWebsites(IOrganizationService service)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            long retrievalSequence = Interlocked.Increment(ref _websiteRetrievalSequence);
            LastCompatibilityWarning = null;

            var allWebsites = new List<WebsiteModel>();
            var diagnostics = new List<string>();

            bool enhancedReadable = TryRetrieveWebsites(service, EnhancedSchema, allWebsites, diagnostics);
            bool standardReadable = TryRetrieveWebsites(service, StandardSchema, allWebsites, diagnostics);

            if (!enhancedReadable && !standardReadable)
            {
                throw new InvalidOperationException(
                    "Power Pages website data could not be read from either supported data model. " +
                    "The tool tried enhanced mspp_website and standard adx_website. " +
                    BuildDiagnosticSuffix(diagnostics));
            }

            // During or after a data-model migration, equivalent records can exist in both
            // stores. Only active websites are queried, and an enhanced record wins if the
            // same GUID is returned from both stores.
            var deduplicated = allWebsites
                .GroupBy(w => w.WebsiteId)
                .Select(group =>
                    group.OrderBy(w => w.DataModel == PowerPagesDataModel.Enhanced ? 0 : 1)
                         .First())
                .OrderBy(w => w.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ThenBy(w => w.PartialUrl ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var map = _websiteModels.GetOrCreateValue(service);
            map.Clear();

            foreach (WebsiteModel website in deduplicated)
            {
                if (website.WebsiteId != Guid.Empty)
                {
                    map[website.WebsiteId] = website.DataModel;
                }
            }

            var nonFatal = new List<string>();

            if (!enhancedReadable)
            {
                nonFatal.Add("Enhanced Power Pages website data was not readable.");
            }

            if (!standardReadable)
            {
                nonFatal.Add("Standard Power Pages website data was not readable.");
            }

            if (nonFatal.Count > 0)
            {
                LastCompatibilityWarning = string.Join(" ", nonFatal) + " " + BuildDiagnosticSuffix(diagnostics);
                Trace.WriteLine("PowerPagesAlmDriftInspector compatibility warning: " + LastCompatibilityWarning);
            }

            RecordWebsiteRetrievalBatch(retrievalSequence, deduplicated);
            return deduplicated;
        }

        /// <summary>
        /// Returns defensive snapshots of the most recent website retrieval batches,
        /// ordered by retrieval start sequence. This is intentionally internal and
        /// process-local; it exists only to let the TC3 UI diagnostics bind to the
        /// same WebsiteModel objects that the retrieval service actually returned.
        /// </summary>
        internal static List<List<WebsiteModel>> GetRecentWebsiteBatchesSnapshot()
        {
            lock (WebsiteBatchSync)
            {
                return RecentWebsiteBatches
                    .OrderBy(batch => batch.Sequence)
                    .Select(batch => batch.Websites.ToList())
                    .ToList();
            }
        }

        private static void RecordWebsiteRetrievalBatch(
            long sequence,
            IEnumerable<WebsiteModel> websites)
        {
            var snapshot = (websites ?? Enumerable.Empty<WebsiteModel>())
                .Where(website => website != null)
                .ToList();

            lock (WebsiteBatchSync)
            {
                RecentWebsiteBatches.RemoveAll(batch => batch.Sequence == sequence);
                RecentWebsiteBatches.Add(new WebsiteRetrievalBatch(sequence, snapshot));
                RecentWebsiteBatches.Sort((left, right) => left.Sequence.CompareTo(right.Sequence));

                while (RecentWebsiteBatches.Count > MaxRecentWebsiteBatches)
                {
                    RecentWebsiteBatches.RemoveAt(0);
                }
            }
        }

        public List<SiteSettingModel> GetSiteSettings(IOrganizationService service, Guid websiteId)
        {
            return RetrieveSiteSettings(service, websiteId);
        }

        public List<SiteSettingModel> LoadSiteSettings(IOrganizationService service, Guid websiteId)
        {
            return RetrieveSiteSettings(service, websiteId);
        }

        public List<SiteSettingModel> GetPowerPagesSiteSettings(IOrganizationService service, Guid websiteId)
        {
            return RetrieveSiteSettings(service, websiteId);
        }

        public List<SiteSettingModel> RetrievePowerPagesSiteSettings(IOrganizationService service, Guid websiteId)
        {
            return RetrieveSiteSettings(service, websiteId);
        }

        /// <summary>
        /// Loads site settings from the storage family that owns the selected website.
        /// It never mixes mspp_* and adx_* settings for one selected website.
        /// </summary>
        public List<SiteSettingModel> RetrieveSiteSettings(IOrganizationService service, Guid websiteId)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (websiteId == Guid.Empty)
            {
                throw new ArgumentException("A valid website id is required.", nameof(websiteId));
            }

            PowerPagesDataModel dataModel = ResolveWebsiteDataModel(service, websiteId);
            PowerPagesSchema schema = dataModel == PowerPagesDataModel.Standard
                ? StandardSchema
                : EnhancedSchema;

            try
            {
                QueryExpression query = new QueryExpression(schema.SiteSettingEntity)
                {
                    ColumnSet = new ColumnSet(
                        schema.SiteSettingIdAttribute,
                        schema.SiteSettingNameAttribute,
                        schema.SiteSettingValueAttribute,
                        schema.SiteSettingWebsiteAttribute),
                    NoLock = true
                };

                query.Criteria.AddCondition(
                    schema.SiteSettingWebsiteAttribute,
                    ConditionOperator.Equal,
                    websiteId);

                List<Entity> entities = RetrieveAllPages(service, query);

                return entities
                    .Select(entity => new SiteSettingModel
                    {
                        SiteSettingId = GetEntityId(entity, schema.SiteSettingIdAttribute),
                        WebsiteId = websiteId,
                        Name = entity.GetAttributeValue<string>(schema.SiteSettingNameAttribute) ?? string.Empty,
                        Value = entity.GetAttributeValue<string>(schema.SiteSettingValueAttribute) ?? string.Empty,
                        DataModel = schema.DataModel
                    })
                    .OrderBy(setting => setting.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(setting => setting.SiteSettingId)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "The selected website uses the " + schema.DisplayName +
                    " Power Pages data model, but its site settings could not be read from " +
                    schema.SiteSettingEntity + ". Confirm the table is available and that the " +
                    "connected Dataverse user can read it. Details: " + ex.Message,
                    ex);
            }
        }

        /// <summary>
        /// Returns the detected data model for a website already loaded by RetrieveWebsites.
        /// If the website was not cached, the service performs a targeted lookup.
        /// </summary>
        public PowerPagesDataModel GetWebsiteDataModel(IOrganizationService service, Guid websiteId)
        {
            return ResolveWebsiteDataModel(service, websiteId);
        }

        private bool TryRetrieveWebsites(
            IOrganizationService service,
            PowerPagesSchema schema,
            List<WebsiteModel> destination,
            List<string> diagnostics)
        {
            try
            {
                QueryExpression query = new QueryExpression(schema.WebsiteEntity)
                {
                    ColumnSet = new ColumnSet(
                        schema.WebsiteIdAttribute,
                        schema.WebsiteNameAttribute,
                        schema.WebsitePartialUrlAttribute,
                        schema.WebsitePrimaryDomainAttribute),
                    NoLock = true
                };

                // Avoid exposing deactivated migration remnants as selectable websites.
                query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);

                List<Entity> entities = RetrieveAllPages(service, query);

                foreach (Entity entity in entities)
                {
                    Guid websiteId = GetEntityId(entity, schema.WebsiteIdAttribute);

                    if (websiteId == Guid.Empty)
                    {
                        continue;
                    }

                    destination.Add(new WebsiteModel
                    {
                        WebsiteId = websiteId,
                        Name = entity.GetAttributeValue<string>(schema.WebsiteNameAttribute) ?? string.Empty,
                        PartialUrl = entity.GetAttributeValue<string>(schema.WebsitePartialUrlAttribute) ?? string.Empty,
                        PrimaryDomainName = entity.GetAttributeValue<string>(schema.WebsitePrimaryDomainAttribute) ?? string.Empty,
                        DataModel = schema.DataModel
                    });
                }

                return true;
            }
            catch (Exception ex)
            {
                diagnostics.Add(schema.DisplayName + " (" + schema.WebsiteEntity + "): " + ex.Message);

                if (!IsSchemaUnavailable(ex, schema.WebsiteEntity))
                {
                    Trace.WriteLine(
                        "PowerPagesAlmDriftInspector could not read " +
                        schema.DisplayName + " website data: " + ex);
                }

                return false;
            }
        }

        private PowerPagesDataModel ResolveWebsiteDataModel(IOrganizationService service, Guid websiteId)
        {
            ConcurrentDictionary<Guid, PowerPagesDataModel> map = _websiteModels.GetOrCreateValue(service);

            PowerPagesDataModel cached;
            if (map.TryGetValue(websiteId, out cached) && cached != PowerPagesDataModel.Unknown)
            {
                return cached;
            }

            var diagnostics = new List<string>();

            bool enhancedExists = TryWebsiteExists(service, EnhancedSchema, websiteId, diagnostics);
            if (enhancedExists)
            {
                map[websiteId] = PowerPagesDataModel.Enhanced;
                return PowerPagesDataModel.Enhanced;
            }

            bool standardExists = TryWebsiteExists(service, StandardSchema, websiteId, diagnostics);
            if (standardExists)
            {
                map[websiteId] = PowerPagesDataModel.Standard;
                return PowerPagesDataModel.Standard;
            }

            throw new InvalidOperationException(
                "The selected Power Pages website could not be resolved to either the enhanced " +
                "mspp_website model or the standard adx_website model. Reload the website list " +
                "and select the site again. " + BuildDiagnosticSuffix(diagnostics));
        }

        private bool TryWebsiteExists(
            IOrganizationService service,
            PowerPagesSchema schema,
            Guid websiteId,
            List<string> diagnostics)
        {
            try
            {
                QueryExpression query = new QueryExpression(schema.WebsiteEntity)
                {
                    ColumnSet = new ColumnSet(schema.WebsiteIdAttribute),
                    NoLock = true,
                    PageInfo = new PagingInfo
                    {
                        PageNumber = 1,
                        Count = 1
                    }
                };

                query.Criteria.AddCondition(
                    schema.WebsiteIdAttribute,
                    ConditionOperator.Equal,
                    websiteId);

                query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);

                EntityCollection result = service.RetrieveMultiple(query);
                return result != null && result.Entities.Count > 0;
            }
            catch (Exception ex)
            {
                diagnostics.Add(schema.DisplayName + " (" + schema.WebsiteEntity + "): " + ex.Message);
                return false;
            }
        }

        private static List<Entity> RetrieveAllPages(
            IOrganizationService service,
            QueryExpression query)
        {
            var all = new List<Entity>();
            int pageNumber = 1;
            string pagingCookie = null;

            while (true)
            {
                query.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = DataversePageSize,
                    PagingCookie = pagingCookie
                };

                EntityCollection page = service.RetrieveMultiple(query);

                if (page == null)
                {
                    break;
                }

                all.AddRange(page.Entities);

                if (!page.MoreRecords)
                {
                    break;
                }

                pageNumber++;
                pagingCookie = page.PagingCookie;
            }

            return all;
        }

        private static Guid GetEntityId(Entity entity, string idAttribute)
        {
            if (entity == null)
            {
                return Guid.Empty;
            }

            if (entity.Id != Guid.Empty)
            {
                return entity.Id;
            }

            return entity.GetAttributeValue<Guid>(idAttribute);
        }

        private static bool IsSchemaUnavailable(Exception exception, string logicalName)
        {
            if (exception == null)
            {
                return false;
            }

            string message = exception.ToString();
            if (string.IsNullOrWhiteSpace(message))
            {
                return false;
            }

            string lower = message.ToLowerInvariant();
            string entity = (logicalName ?? string.Empty).ToLowerInvariant();

            if (!string.IsNullOrWhiteSpace(entity) && !lower.Contains(entity))
            {
                return false;
            }

            return lower.Contains("not found in the metadatacache") ||
                   lower.Contains("not found in the metadata cache") ||
                   lower.Contains("does not exist") ||
                   lower.Contains("unknown entity") ||
                   lower.Contains("invalid entity") ||
                   lower.Contains("entity type") && lower.Contains("not found");
        }

        private static string BuildDiagnosticSuffix(IEnumerable<string> diagnostics)
        {
            string text = string.Join(
                " | ",
                (diagnostics ?? Enumerable.Empty<string>())
                    .Where(item => !string.IsNullOrWhiteSpace(item)));

            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            return "Diagnostics: " + text;
        }

        private sealed class WebsiteRetrievalBatch
        {
            public WebsiteRetrievalBatch(long sequence, List<WebsiteModel> websites)
            {
                Sequence = sequence;
                Websites = websites ?? new List<WebsiteModel>();
            }

            public long Sequence { get; private set; }
            public List<WebsiteModel> Websites { get; private set; }
        }

        private sealed class PowerPagesSchema
        {
            public PowerPagesSchema(
                PowerPagesDataModel dataModel,
                string displayName,
                string websiteEntity,
                string websiteIdAttribute,
                string websiteNameAttribute,
                string websitePartialUrlAttribute,
                string websitePrimaryDomainAttribute,
                string siteSettingEntity,
                string siteSettingIdAttribute,
                string siteSettingNameAttribute,
                string siteSettingValueAttribute,
                string siteSettingWebsiteAttribute)
            {
                DataModel = dataModel;
                DisplayName = displayName;
                WebsiteEntity = websiteEntity;
                WebsiteIdAttribute = websiteIdAttribute;
                WebsiteNameAttribute = websiteNameAttribute;
                WebsitePartialUrlAttribute = websitePartialUrlAttribute;
                WebsitePrimaryDomainAttribute = websitePrimaryDomainAttribute;
                SiteSettingEntity = siteSettingEntity;
                SiteSettingIdAttribute = siteSettingIdAttribute;
                SiteSettingNameAttribute = siteSettingNameAttribute;
                SiteSettingValueAttribute = siteSettingValueAttribute;
                SiteSettingWebsiteAttribute = siteSettingWebsiteAttribute;
            }

            public PowerPagesDataModel DataModel { get; private set; }

            public string DisplayName { get; private set; }

            public string WebsiteEntity { get; private set; }

            public string WebsiteIdAttribute { get; private set; }

            public string WebsiteNameAttribute { get; private set; }

            public string WebsitePartialUrlAttribute { get; private set; }

            public string WebsitePrimaryDomainAttribute { get; private set; }

            public string SiteSettingEntity { get; private set; }

            public string SiteSettingIdAttribute { get; private set; }

            public string SiteSettingNameAttribute { get; private set; }

            public string SiteSettingValueAttribute { get; private set; }

            public string SiteSettingWebsiteAttribute { get; private set; }
        }
    }
}
