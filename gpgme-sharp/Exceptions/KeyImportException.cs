namespace Libgpgme
{
    public class KeyImportException : GpgmeException
    {
        public KeyImportException() {
        }

        public KeyImportException(string message, int GPGMEError)
            : base(message, GPGMEError) {
        }
    }
}