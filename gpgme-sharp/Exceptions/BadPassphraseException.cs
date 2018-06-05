namespace Libgpgme
{
    public class BadPassphraseException : GpgmeException
    {
        public DecryptionResult DecryptionResult;
        public PassphraseInfo PassphraseInfo;

        public BadPassphraseException() {
        }

        public BadPassphraseException(DecryptionResult rst) {
            DecryptionResult = rst;
        }

        public BadPassphraseException(PassphraseInfo info) {
            PassphraseInfo = info;
        }
    }
}