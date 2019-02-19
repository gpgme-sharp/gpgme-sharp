using System;
using System.Runtime.InteropServices;
using Libgpgme.Interop;

namespace Libgpgme
{
    public class EncryptionResult
    {
        internal EncryptionResult(IntPtr rPtr) {
            InvalidRecipients = null;

            if (rPtr == IntPtr.Zero) {
                throw new InvalidPtrException("An invalid pointer for the encrypt_result structure has been supplied.");
            }
            UpdateFromMem(rPtr);
        }

        public InvalidKey InvalidRecipients { get; private set; }

        private void UpdateFromMem(IntPtr rPtr) {
            var rst = new _gpgme_op_encrypt_result();
            Marshal.PtrToStructure(rPtr, rst);

            if (rst.invalid_recipients != IntPtr.Zero) {
                InvalidRecipients = new InvalidKey(rst.invalid_recipients);
            }
        }
    }
}