namespace Libgpgme
{
    public class InvalidDataBufferException : GpgmeException
    {
        public InvalidDataBufferException() {
        }

        public InvalidDataBufferException(string message)
            : base(message) {
        }
    }
}