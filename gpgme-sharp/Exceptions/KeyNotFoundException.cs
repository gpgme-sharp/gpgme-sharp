namespace Libgpgme
{
    public class KeyNotFoundException : GpgmeException
    {
        public KeyNotFoundException() {
        }

        public KeyNotFoundException(string message)
            : base(message) {
        }
    }
}