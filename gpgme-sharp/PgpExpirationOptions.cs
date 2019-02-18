using System;

namespace Libgpgme
{
    public class PgpExpirationOptions
    {
        private static readonly DateTime _unix_date = new DateTime(1970, 1, 1);
        public int[] SelectedSubkeys; // if not set - expire the whole key (pub SC)
        
        internal bool cmdSend;
        internal bool forceQuit;
        internal int nsubkey;

        public bool IsInfinitely => ExpirationDate.Equals(_unix_date);

        public DateTime ExpirationDate { get; set; } = _unix_date;

        public void MakeInfinitely() {
            ExpirationDate = _unix_date;
        }
    }
}