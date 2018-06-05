using System;

namespace Libgpgme
{
    public class PgpSignatureOptions
    {
        private static readonly DateTime _unix_date = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        private int _trustdepth = 1;

        public PgpSignatureClass Class = PgpSignatureClass.Generic;

        public DateTime ExpirationDate = _unix_date;
        public bool LocalPromoteOkay = true;
        public int[] SelectedUids;
        public PgpSignatureTrustLevel TrustLevel = PgpSignatureTrustLevel.Marginal;
        public string TrustRegexp = "";
        public PgpSignatureType Type = PgpSignatureType.Normal;

        internal bool cmdSend; // sign command send to gnupg?
        internal bool forceQuit;
        internal int nUid;
        internal bool signAllUids = true;

        public int TrustDepth {
            get { return _trustdepth; }
            set {
                if (value > 0) {
                    _trustdepth = value;
                } else {
                    throw new GpgmeException("You cannot specify a trust level lower than 1.");
                }
            }
        }

        public bool IsInfinitely {
            get { return ExpirationDate.Equals(_unix_date); }
            set {
                if (value) {
                    ExpirationDate = _unix_date;
                }
            }
        }

        internal string GetExpirationDate() {
            if (ExpirationDate.Equals(_unix_date)) {
                return "0";
            }
            return ExpirationDate.ToString("yyyy-MM-dd");
        }
    }
}