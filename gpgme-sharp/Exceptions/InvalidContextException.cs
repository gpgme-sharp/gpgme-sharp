namespace Libgpgme
{
    public class InvalidContextException : GpgmeException
    {
        public InvalidContextException(string message)
            : base(message) {
        }

        public InvalidContextException() {
        }
    }
}