using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace Duracellko.WindowsAzureVmManager.Identity
{
    public class CacheIssuerNameRegistry : ValidatingIssuerNameRegistry
    {
        private const string IssuingAuthorityCacheKey = "Duracellko.WindowsAzureVmManager.IssuingAuthority";

        private static readonly char[] issuerPathDelimiter = new char[] { '/' };
        private static readonly ObjectCache issuingAuthorityCache = MemoryCache.Default;

        public static string MetadataLocation { get; set; }

        protected override bool IsThumbprintValid(string thumbprint, string issuer)
        {
            var issuingAuthority = GetIssuingAuthority();
            string issuerId = GetIssuerId(issuer);
            return ContainsThumbprint(issuingAuthority, thumbprint) && ContainsIssuerId(issuingAuthority, issuerId);
        }

        private static bool ContainsThumbprint(IssuingAuthority issuingAuthority, string thumbprint)
        {
            return issuingAuthority.Thumbprints.Contains(thumbprint);
        }

        private static bool ContainsIssuerId(IssuingAuthority issuingAuthority, string issuerId)
        {
            return issuingAuthority.Issuers.Any(i => string.Equals(GetIssuerId(i), issuerId, StringComparison.OrdinalIgnoreCase));
        }

        private static string GetIssuerId(string issuer)
        {
            string issuerId = issuer.Trim(issuerPathDelimiter);
            int index = issuerId.LastIndexOfAny(issuerPathDelimiter);
            return issuerId.Substring(index + 1);
        }

        private static IssuingAuthority GetIssuingAuthority()
        {
            IssuingAuthority issuingAuthority = issuingAuthorityCache[IssuingAuthorityCacheKey] as IssuingAuthority;
            if (issuingAuthority == null)
            {
                issuingAuthority = ValidatingIssuerNameRegistry.GetIssuingAuthority(MetadataLocation);
                issuingAuthorityCache.Add(IssuingAuthorityCacheKey, issuingAuthority, DateTimeOffset.UtcNow.AddHours(1.0));
            }

            return issuingAuthority;
        }
    }
}
