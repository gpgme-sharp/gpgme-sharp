namespace Libgpgme
{
    public class InvalidKeyException : GpgmeException
    {
        public InvalidKeyException() {
        }

        public InvalidKeyException(string message)
            : base(message) {
        }
    }
}