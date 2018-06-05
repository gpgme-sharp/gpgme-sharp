namespace Libgpgme
{
    public class PgpPassphraseOptions
    {
        internal static int MAX_PASSWD_COUNT = 4;
        public bool EmptyOkay;

        // passphrase change
        public char[] NewPassphrase;
        public PassphraseDelegate NewPassphraseCallback;
        public char[] OldPassphrase;
        public PassphraseDelegate OldPassphraseCallback;
        internal bool aborthandler;

        internal int emptypasswdcount;
        internal bool missingpasswd;
        internal bool needoldpw = true;
        internal bool passphraseSendCmd;
    }
}