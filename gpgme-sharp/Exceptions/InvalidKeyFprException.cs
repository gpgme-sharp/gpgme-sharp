namespace Libgpgme
{
    public class InvalidKeyFprException : GpgmeException
    {
        public InvalidKeyFprException() {
        }

        public InvalidKeyFprException(string message)
            : base(message) {
        }
    }
}