namespace Libgpgme
{
    public class KeyConflictException : GpgmeException
    {
        public KeyConflictException() {
        }

        public KeyConflictException(string message)
            : base(message) {
        }
    }
}