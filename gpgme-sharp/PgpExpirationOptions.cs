using System;

namespace Libgpgme
{
    public class PgpExpirationOptions
    {
        private static readonly DateTime _unix_date = new DateTime(1970, 1, 1);
        private DateTime _expiration_date = _unix_date;

        public int[] SelectedSubkeys; // if not set - expire the whole key (pub SC)
        
        internal bool cmdSend;
        internal bool forceQuit;
        internal int nsubkey;

        public bool IsInfinitely {
            get { return _expiration_date.Equals(_unix_date); }
        }

        public DateTime ExpirationDate {
            get { return _expiration_date; }
            set { _expiration_date = value; }
        }

        public void MakeInfinitely() {
            _expiration_date = _unix_date;
        }
    }
}