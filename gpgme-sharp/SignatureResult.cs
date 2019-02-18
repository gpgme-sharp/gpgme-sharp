using System;
using System.Runtime.InteropServices;
using Libgpgme.Interop;

namespace Libgpgme
{
    public class SignatureResult
    {
        private NewSignature _signatures;

        internal SignatureResult(IntPtr sigrstPtr) {
            if (sigrstPtr.Equals(IntPtr.Zero)) {
                throw new InvalidPtrException("An invalid signature result pointer has been supplied.");
            }
            UpdateFromMem(sigrstPtr);
        }

        public InvalidKey InvalidSigners { get; private set; }

        public NewSignature Signatures => _signatures;

        private void UpdateFromMem(IntPtr sigrstPtr) {
            var rst = new _gpgme_op_sign_result();
            Marshal.PtrToStructure(sigrstPtr, rst);

            if (!rst.invalid_signers.Equals(IntPtr.Zero)) {
                InvalidSigners = new InvalidKey(rst.invalid_signers);
            }

            if (!rst.signatures.Equals(IntPtr.Zero)) {
                _signatures = new NewSignature(rst.signatures);
            }
        }
    }
}