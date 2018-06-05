namespace Libgpgme
{
    public class InvalidTimestampException : GpgmeException
    {
        public InvalidTimestampException(string message)
            : base(message) {
        }

        public InvalidTimestampException() {
        }
    }
}