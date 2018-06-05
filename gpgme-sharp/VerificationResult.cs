using System;
using System.Runtime.InteropServices;
using Libgpgme.Interop;

namespace Libgpgme
{
    public class VerificationResult
    {
        public string FileName { get; private set; }
        public Signature Signature { get; private set; }

        internal VerificationResult(IntPtr rstPtr) {
            if (rstPtr == IntPtr.Zero) {
                throw new InvalidPtrException("An invalid verify result pointer has been given.");
            }

            UpdateFromMem(rstPtr);
        }

        private void UpdateFromMem(IntPtr sigPtr) {
            var ver = new _gpgme_op_verify_result();
            Marshal.PtrToStructure(sigPtr, ver);
            FileName = Gpgme.PtrToStringUTF8(ver.file_name);

            if (ver.signature != IntPtr.Zero) {
                Signature = new Signature(ver.signature);
            }
        }
    }
}