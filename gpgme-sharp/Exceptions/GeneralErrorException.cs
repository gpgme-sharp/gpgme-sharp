namespace Libgpgme
{
    public class GeneralErrorException : GpgmeException
    {
        public GeneralErrorException(string message)
            : base(message) {
        }
    }
}