using System.Collections.Generic;
using System.Text;

namespace Libgpgme
{
    public sealed class KeyPreferences
    {
        public List<CipherAlgorithm> Ciphers = new List<CipherAlgorithm>();
        public List<CompressAlgorithm> Compress = new List<CompressAlgorithm>();
        public List<HashAlgorithm> Hashes = new List<HashAlgorithm>();
        public PgpFeatureFlags PGPFeatures = PgpFeatureFlags.MDC;

        public string GetPrefString() {
            var sb = new StringBuilder();

            // prefered symmetric ciphers
            foreach (CipherAlgorithm algo in Ciphers) {
                UpdateSb(sb, "S" + (int) algo);
            }

            // prefered hashes
            foreach (HashAlgorithm hash in Hashes) {
                UpdateSb(sb, "H" + (int) hash);
            }

            // prefered compression algorithms
            foreach (CompressAlgorithm compress in Compress) {
                UpdateSb(sb, "Z" + (int) compress);
            }

            //if (

            return sb.ToString();
        }

        private void UpdateSb(StringBuilder sb, string addtxt) {
            if (sb.Length > 0) {
                sb.Append(" ");
            }
            sb.Append(addtxt);
        }
    }
}