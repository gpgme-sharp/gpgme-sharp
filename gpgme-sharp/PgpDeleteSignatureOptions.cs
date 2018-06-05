namespace Libgpgme
{
    public class PgpDeleteSignatureOptions
    {
        public bool DeleteSelfSignature;
        public int[] SelectedSignatures;
        public int SelectedUid = 1;
        internal bool cmdSend; // revsig command send to gnupg?
        internal int ndeletenum;
        internal bool uidSend; // Uid selected?
    }
}