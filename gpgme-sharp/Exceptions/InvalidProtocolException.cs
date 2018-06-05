namespace Libgpgme
{
    public class InvalidProtocolException : GpgmeException
    {
        public InvalidProtocolException(string message)
            : base(message) {
        }
    }
}