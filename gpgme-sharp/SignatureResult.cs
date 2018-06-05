using System;
using System.Runtime.InteropServices;
using Libgpgme.Interop;

namespace Libgpgme
{
    public class SignatureResult
    {
        /* The list of invalid signers.  */
        private InvalidKey _invalid_signers;
        private NewSignature _signatures;

        internal SignatureResult(IntPtr sigrstPtr) {
            if (sigrstPtr.Equals(IntPtr.Zero)) {
                throw new InvalidPtrException("An invalid signature result pointer has been supplied.");
            }
            UpdateFromMem(sigrstPtr);
        }

        public InvalidKey InvalidSigners {
            get { return _invalid_signers; }
        }
        public NewSignature Signatures {
            get { return _signatures; }
        }

        private void UpdateFromMem(IntPtr sigrstPtr) {
            var rst = new _gpgme_op_sign_result();
            Marshal.PtrToStructure(sigrstPtr, rst);

            if (!rst.invalid_signers.Equals(IntPtr.Zero)) {
                _invalid_signers = new InvalidKey(rst.invalid_signers);
            }

            if (!rst.signatures.Equals(IntPtr.Zero)) {
                _signatures = new NewSignature(rst.signatures);
            }
        }
    }
}