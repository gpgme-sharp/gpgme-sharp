namespace Libgpgme
{
    public class AmbiguousKeyException : GpgmeException
    {
        public AmbiguousKeyException() {
        }

        public AmbiguousKeyException(string message)
            : base(message) {
        }
    }
}