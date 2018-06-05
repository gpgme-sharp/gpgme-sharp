using System;

namespace Libgpgme
{
    public class PgpSubkeyOptions
    {
        public const int KEY_LENGTH_1024 = 1024;
        public const int KEY_LENGTH_2048 = 2048;
        public const int KEY_LENGTH_4096 = 4096;

        private static readonly DateTime _unix_date = new DateTime(1970, 1, 1);

        public PgpSubkeyAlgorithm Algorithm;
        public AlgorithmCapability Capability;
        internal bool cmdSend;

        public int KeyLength { get; set; }
        public DateTime ExpirationDate { get; set; }

        public PgpSubkeyOptions() {
            ExpirationDate = _unix_date;
            KeyLength = KEY_LENGTH_2048;
        }

        public bool IsInfinitely {
            get { return ExpirationDate.Equals(_unix_date); }
        }

        public void MakeInfinitely() {
            ExpirationDate = _unix_date;
        }

        public void SetAlgorithm(KeyAlgorithm algo) {
            switch (algo) {
                case KeyAlgorithm.DSA:
                    Algorithm = PgpSubkeyAlgorithm.DSASignOnly;
                    break;
                case KeyAlgorithm.ELG:
                    Algorithm = PgpSubkeyAlgorithm.ELGEncryptOnly;
                    break;
                case KeyAlgorithm.RSA:
                    Algorithm = PgpSubkeyAlgorithm.RSAUseCapabilities;
                    break;
                default:
                    throw new NotSupportedException("Algorithm is not supported as sub key.",
                        new NotSupportedException("Algorithm is not supported as sub key."));
            }
        }
    }
}