using System;

namespace PowerPagesAlmDriftInspector.Models
{
    /// <summary>
    /// Power Pages storage model used by the selected website.
    /// Enhanced = mspp_* Power Pages tables.
    /// Standard = older / standard adx_* Portal Management tables.
    /// </summary>
    public enum PowerPagesDataModel
    {
        Unknown = 0,
        Enhanced = 1,
        Standard = 2
    }

    public class WebsiteModel
    {
        private string _displayName;

        public WebsiteModel()
        {
        }

        public WebsiteModel(Guid websiteId, string name, string partialUrl)
        {
            WebsiteId = websiteId;
            Name = name;
            PartialUrl = partialUrl;
        }

        public WebsiteModel(Guid websiteId, string name, string partialUrl, string primaryDomainName)
            : this(websiteId, name, partialUrl)
        {
            PrimaryDomainName = primaryDomainName;
        }

        public Guid WebsiteId { get; set; }

        // Compatibility aliases retained for earlier/current control revisions.
        public Guid Id
        {
            get { return WebsiteId; }
            set { WebsiteId = value; }
        }

        public Guid RecordId
        {
            get { return WebsiteId; }
            set { WebsiteId = value; }
        }

        public Guid WebsiteGuid
        {
            get { return WebsiteId; }
            set { WebsiteId = value; }
        }

        public string Name { get; set; }

        public string PartialUrl { get; set; }

        public string PartialURL
        {
            get { return PartialUrl; }
            set { PartialUrl = value; }
        }

        public string PrimaryDomainName { get; set; }

        public string Url
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(PrimaryDomainName))
                {
                    return PrimaryDomainName;
                }

                return PartialUrl ?? string.Empty;
            }
            set
            {
                PrimaryDomainName = value;
            }
        }

        public PowerPagesDataModel DataModel { get; set; }

        /// <summary>
        /// Basic model name retained for compatibility and programmatic use.
        /// </summary>
        public string DataModelName
        {
            get
            {
                switch (DataModel)
                {
                    case PowerPagesDataModel.Enhanced:
                        return "Enhanced";
                    case PowerPagesDataModel.Standard:
                        return "Standard";
                    default:
                        return "Unknown";
                }
            }
        }

        /// <summary>
        /// Compact UI label used by the TC2 diagnostic layer.
        /// </summary>
        public string DataModelShortName
        {
            get
            {
                switch (DataModel)
                {
                    case PowerPagesDataModel.Enhanced:
                        return "Enhanced (mspp)";
                    case PowerPagesDataModel.Standard:
                        return "Standard (adx)";
                    default:
                        return "Unknown";
                }
            }
        }

        public string WebsiteEntityLogicalName
        {
            get
            {
                switch (DataModel)
                {
                    case PowerPagesDataModel.Enhanced:
                        return "mspp_website";
                    case PowerPagesDataModel.Standard:
                        return "adx_website";
                    default:
                        return string.Empty;
                }
            }
        }

        public string SiteSettingEntityLogicalName
        {
            get
            {
                switch (DataModel)
                {
                    case PowerPagesDataModel.Enhanced:
                        return "mspp_sitesetting";
                    case PowerPagesDataModel.Standard:
                        return "adx_sitesetting";
                    default:
                        return string.Empty;
                }
            }
        }

        /// <summary>
        /// Explicit runtime evidence of the table family used for this website.
        /// </summary>
        public string DataModelDiagnosticName
        {
            get
            {
                switch (DataModel)
                {
                    case PowerPagesDataModel.Enhanced:
                        return "Enhanced (mspp_website / mspp_sitesetting)";
                    case PowerPagesDataModel.Standard:
                        return "Standard (adx_website / adx_sitesetting)";
                    default:
                        return "Unknown";
                }
            }
        }

        /// <summary>
        /// Existing display behavior remains unchanged so matching logic and
        /// non-diagnostic UI continue to use the same website text.
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_displayName))
                {
                    return _displayName;
                }

                string name = Name ?? string.Empty;
                string partialUrl = PartialUrl ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(partialUrl))
                {
                    return name + " - " + partialUrl;
                }

                if (!string.IsNullOrWhiteSpace(name))
                {
                    return name;
                }

                if (!string.IsNullOrWhiteSpace(partialUrl))
                {
                    return partialUrl;
                }

                if (!string.IsNullOrWhiteSpace(PrimaryDomainName))
                {
                    return PrimaryDomainName;
                }

                return WebsiteId == Guid.Empty ? "(Unnamed website)" : WebsiteId.ToString();
            }
            set
            {
                _displayName = value;
            }
        }

        /// <summary>
        /// TC2 selector display text. This is deliberately separate from DisplayName.
        /// </summary>
        public string DisplayNameWithDataModel
        {
            get
            {
                return DisplayName + " [" + DataModelShortName + "]";
            }
        }

        public override string ToString()
        {
            // Preserve pre-TC2 behavior. The diagnostic adapter explicitly opts
            // website selectors into DisplayNameWithDataModel.
            return DisplayName;
        }
    }
}
