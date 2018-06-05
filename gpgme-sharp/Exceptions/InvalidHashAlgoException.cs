namespace Libgpgme
{
    public class InvalidHashAlgoException : GpgmeException
    {
        public InvalidHashAlgoException(string message)
            : base(message) {
        }
    }
}