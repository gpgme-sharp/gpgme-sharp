namespace Libgpgme
{
    public class TimestampNotAvailableException : GpgmeException
    {
        public TimestampNotAvailableException(string message)
            : base(message) {
        }

        public TimestampNotAvailableException() {
        }
    }
}