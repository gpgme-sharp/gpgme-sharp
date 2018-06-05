namespace Libgpgme
{
    public class CombinedResult
    {
        public DecryptionResult DecryptionResult { get; private set; }

        public VerificationResult VerificationResult { get; private set; }

        internal CombinedResult(DecryptionResult decrst, VerificationResult verrst) {
            DecryptionResult = decrst;
            VerificationResult = verrst;
        }
    }
}