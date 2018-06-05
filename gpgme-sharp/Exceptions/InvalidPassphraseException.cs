namespace Libgpgme
{
    public class InvalidPassphraseException : GpgmeException
    {
        public InvalidPassphraseException() {
        }

        public InvalidPassphraseException(string message)
            : base(message) {
        }
    }
}