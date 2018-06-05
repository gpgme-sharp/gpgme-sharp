namespace Libgpgme
{
    public class KeyExportException : GpgmeException
    {
        public KeyExportException() {
        }

        public KeyExportException(string message, int GPGMEError)
            : base(message, GPGMEError) {
        }
    }
}