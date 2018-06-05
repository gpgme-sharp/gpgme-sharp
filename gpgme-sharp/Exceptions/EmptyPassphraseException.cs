namespace Libgpgme
{
    public class EmptyPassphraseException : BadPassphraseException
    {
        public EmptyPassphraseException() {
        }

        public EmptyPassphraseException(PassphraseInfo info) {
            PassphraseInfo = info;
        }
    }
}