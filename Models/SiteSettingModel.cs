using System;

namespace PowerPagesAlmDriftInspector.Models
{
    public class SiteSettingModel
    {
        private string _category;

        public SiteSettingModel()
        {
        }

        public SiteSettingModel(Guid siteSettingId, string name, string value)
        {
            SiteSettingId = siteSettingId;
            Name = name;
            Value = value;
        }

        public SiteSettingModel(Guid siteSettingId, Guid websiteId, string name, string value)
            : this(siteSettingId, name, value)
        {
            WebsiteId = websiteId;
        }

        public Guid SiteSettingId { get; set; }

        // Compatibility aliases used by different project revisions.
        public Guid Id
        {
            get { return SiteSettingId; }
            set { SiteSettingId = value; }
        }

        public Guid RecordId
        {
            get { return SiteSettingId; }
            set { SiteSettingId = value; }
        }

        public Guid WebsiteId { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        /// <summary>
        /// Retained for compatibility with the existing 1.2026.1.3 UI,
        /// category filter, detail form and export code.
        ///
        /// Existing code can explicitly assign Category. If it does not,
        /// the category is derived from the first segment of the site-setting
        /// name (for example "Authentication/..." -> "Authentication").
        /// </summary>
        public string Category
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_category))
                {
                    return _category;
                }

                return DeriveCategory(Name);
            }
            set
            {
                _category = value;
            }
        }

        public PowerPagesDataModel DataModel { get; set; }

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

        private static string DeriveCategory(string settingName)
        {
            if (string.IsNullOrWhiteSpace(settingName))
            {
                return string.Empty;
            }

            string trimmed = settingName.Trim();
            int separatorIndex = trimmed.IndexOf('/');

            if (separatorIndex <= 0)
            {
                return trimmed;
            }

            return trimmed.Substring(0, separatorIndex).Trim();
        }
    }
}
