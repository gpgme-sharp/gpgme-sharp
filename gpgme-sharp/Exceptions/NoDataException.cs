namespace Libgpgme
{
    public class NoDataException : GpgmeException
    {
        public VerificationResult VerifyResult;

        public NoDataException() {
        }

        public NoDataException(string message) : base(message) {
        }

        public NoDataException(string message, VerificationResult rst)
            : base(message) {
            VerifyResult = rst;
        }
    }
}