using System;
using System.Collections.Generic;

namespace PowerPagesAlmDriftInspector.Models
{
    public class ComparisonResultModel
    {
        public string SettingName { get; set; }

        public string SourceValue { get; set; }

        public string TargetValue { get; set; }

        public string SourceDisplayValue { get; set; }

        public string TargetDisplayValue { get; set; }

        public string Category { get; set; }

        public string Status { get; set; }

        public int SourceRecordCount { get; set; }

        public int TargetRecordCount { get; set; }

        public int SourceDistinctValueCount { get; set; }

        public int TargetDistinctValueCount { get; set; }

        public string DuplicateClassification { get; set; }

        public string ReviewFocus
        {
            get { return GetReviewFocus(); }
        }

        public bool IsEnvironmentSpecificCandidate
        {
            get { return GetEnvironmentSpecificReasonList().Count > 0; }
        }

        public string EnvironmentSpecificReason
        {
            get
            {
                List<string> reasons = GetEnvironmentSpecificReasonList();

                return reasons.Count == 0
                    ? "No obvious environment-specific pattern detected."
                    : string.Join(" ", reasons.ToArray());
            }
        }

        public string RecommendedAction
        {
            get { return GetRecommendedAction(); }
        }

        public bool IsMatch
        {
            get { return string.Equals(Status, "Match", StringComparison.OrdinalIgnoreCase); }
        }

        public bool IsFinding
        {
            get { return !IsMatch; }
        }

        private string GetReviewFocus()
        {
            if (IsMatch)
            {
                return "No action";
            }

            if (IsDuplicate)
            {
                return string.IsNullOrWhiteSpace(DuplicateClassification)
                    ? "Duplicate site-setting records"
                    : DuplicateClassification;
            }

            if (IsAuthenticationSetting())
            {
                return "Authentication configuration";
            }

            if (IsHttpSecuritySetting())
            {
                return "HTTP/security configuration";
            }

            if (IsCertificateOrSecretSetting())
            {
                return "Certificate, key, or secret reference";
            }

            if (IsAnalyticsSetting())
            {
                return "Analytics or tracking configuration";
            }

            if (IsUrlOrDomainSetting())
            {
                return "URL or domain configuration";
            }

            if (string.Equals(Status, "Missing in Source", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(Status, "Missing in Target", StringComparison.OrdinalIgnoreCase))
            {
                return "Missing setting";
            }

            return "Configuration value drift";
        }

        private string GetRecommendedAction()
        {
            if (string.Equals(Status, "Match", StringComparison.OrdinalIgnoreCase))
            {
                return "No action required.";
            }

            if (IsDuplicate)
            {
                if (!string.IsNullOrWhiteSpace(DuplicateClassification) &&
                    DuplicateClassification.IndexOf("Conflicting", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return "Resolve conflicting duplicate records before deployment. Confirm the intended value, keep only the intended site-setting record in each affected environment, then run the comparison again.";
                }

                return "Remove redundant duplicate records before deployment. Keep one intended site-setting record in each affected environment, then run the comparison again.";
            }

            if (string.Equals(Status, "Different Value", StringComparison.OrdinalIgnoreCase))
            {
                if (IsAuthenticationSetting())
                {
                    return "Review the authentication setting with the portal or identity owner. Confirm whether the difference is expected for this environment before deployment.";
                }

                if (IsHttpSecuritySetting())
                {
                    return "Review the HTTP or security policy difference carefully. Confirm the target policy still allows required services while preserving the intended protection level.";
                }

                if (IsCertificateOrSecretSetting())
                {
                    return "Confirm whether the certificate, key, token, or secret reference is intentionally environment-specific. Do not copy sensitive values without validating ownership and rotation requirements.";
                }

                if (IsAnalyticsSetting())
                {
                    return "Confirm whether the analytics or tracking value should differ by environment. Update deployment notes if the difference is intentional.";
                }

                if (IsUrlOrDomainSetting())
                {
                    return "Confirm whether the URL or domain value should point to the source or target environment. Update the target value or document the environment-specific difference.";
                }

                return "Review the value difference and confirm which environment value should be retained before deployment.";
            }

            if (string.Equals(Status, "Missing in Source", StringComparison.OrdinalIgnoreCase))
            {
                if (IsEnvironmentSpecificCandidate)
                {
                    return "Confirm whether the target-only setting is intentional. Add it to source only if it should be solution-managed, otherwise document it as an accepted environment-specific setting.";
                }

                return "Confirm whether the target-only setting is intentional. Add it to source or document it as an accepted environment-specific setting.";
            }

            if (string.Equals(Status, "Missing in Target", StringComparison.OrdinalIgnoreCase))
            {
                if (IsEnvironmentSpecificCandidate)
                {
                    return "Create the equivalent target setting if required, but validate the target-specific value instead of copying the source value directly.";
                }

                return "Create or deploy this setting to target if required, or document why the target environment should omit it.";
            }

            return "Review this setting and confirm the expected source and target configuration.";
        }

        private List<string> GetEnvironmentSpecificReasonList()
        {
            List<string> reasons = new List<string>();

            string settingName = SettingName ?? string.Empty;
            string category = Category ?? string.Empty;
            string combinedValue = ((SourceValue ?? string.Empty) + " " + (TargetValue ?? string.Empty)).Trim();
            string combinedText = (settingName + " " + category + " " + combinedValue).Trim();

            if (IsMatch)
            {
                return reasons;
            }

            if (ContainsAny(combinedValue, "http://", "https://", ".powerappsportals.com", ".powerapps.com", ".dynamics.com", ".gov.on.ca", ".on.ca", "localhost"))
            {
                reasons.Add("The compared value contains a URL or domain that may be environment-specific.");
            }

            if (ContainsAny(combinedText, "redirecturi", "redirect uri", "assertionconsumerserviceurl", "assertion consumer service", "serviceproviderrealm", "metadataaddress", "validissuers", "authority", "openidconnect", "saml", "azuread", "b2c"))
            {
                reasons.Add("The setting appears related to authentication or identity provider configuration.");
            }

            if (ContainsAny(combinedText, "clientid", "client id", "secret", "token", "key", "certificate", "thumbprint", "connectionstring", "connection string"))
            {
                reasons.Add("The setting name or value suggests an identifier, secret, certificate, or protected reference.");
            }

            if (ContainsAny(combinedText, "trackingid", "tracking id", "googleanalytics", "google analytics", "tagmanager", "gtm"))
            {
                reasons.Add("The setting appears related to analytics or tracking and may intentionally differ by environment.");
            }

            if (ContainsAny(combinedText, "content-security-policy", "access-control-allow-origin", "cors", "frame-ancestors", "script-src", "connect-src"))
            {
                reasons.Add("The setting appears related to HTTP security headers or allowed origins.");
            }

            return reasons;
        }

        private bool IsDuplicate
        {
            get
            {
                return string.Equals(Status, "Duplicate", StringComparison.OrdinalIgnoreCase) ||
                       SourceRecordCount > 1 ||
                       TargetRecordCount > 1;
            }
        }

        private bool IsAuthenticationSetting()
        {
            return ContainsAny(SettingName, "authentication/", "openidconnect", "saml", "azuread", "b2c", "oauth", "login", "registration") ||
                   ContainsAny(Category, "authentication", "implicitgrantflow");
        }

        private bool IsHttpSecuritySetting()
        {
            return ContainsAny(SettingName, "content-security-policy", "access-control-allow-origin", "cors", "frame-ancestors", "script-src", "connect-src") ||
                   ContainsAny(Category, "http");
        }

        private bool IsCertificateOrSecretSetting()
        {
            return ContainsAny(SettingName, "certificate", "thumbprint", "secret", "clientsecret", "client secret", "token", "key", "connectionstring", "connection string") ||
                   ContainsAny(Category, "customcertificates", "certificate");
        }

        private bool IsAnalyticsSetting()
        {
            return ContainsAny(SettingName, "googleanalytics", "google analytics", "trackingid", "tracking id", "tagmanager", "gtm") ||
                   ContainsAny(Category, "googleanalytics", "analytics");
        }

        private bool IsUrlOrDomainSetting()
        {
            return ContainsAny(SettingName, "url", "uri", "realm", "issuer", "authority", "origin", "domain") ||
                   ContainsAny(SourceValue, "http://", "https://") ||
                   ContainsAny(TargetValue, "http://", "https://");
        }

        private static bool ContainsAny(string value, params string[] fragments)
        {
            if (string.IsNullOrWhiteSpace(value) || fragments == null)
            {
                return false;
            }

            foreach (string fragment in fragments)
            {
                if (!string.IsNullOrWhiteSpace(fragment) &&
                    value.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
