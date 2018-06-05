namespace Libgpgme
{
    public class InvalidPtrException : GpgmeException
    {
        public InvalidPtrException(string message)
            : base(message) {
        }
    }
}