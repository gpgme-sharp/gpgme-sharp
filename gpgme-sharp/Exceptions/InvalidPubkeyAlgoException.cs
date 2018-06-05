namespace Libgpgme
{
    public class InvalidPubkeyAlgoException : GpgmeException
    {
        public InvalidPubkeyAlgoException(string message)
            : base(message) {
        }
    }
}