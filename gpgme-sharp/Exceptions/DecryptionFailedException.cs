namespace Libgpgme
{
    public class DecryptionFailedException : GpgmeException
    {
        public DecryptionResult DecryptionResult;

        public DecryptionFailedException() {
        }

        public DecryptionFailedException(string message) : base(message) {
        }

        public DecryptionFailedException(DecryptionResult rst) {
            DecryptionResult = rst;
        }
    }
}