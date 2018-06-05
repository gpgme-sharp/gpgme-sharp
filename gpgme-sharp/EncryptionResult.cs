using System;
using System.Runtime.InteropServices;
using Libgpgme.Interop;

namespace Libgpgme
{
    public class EncryptionResult
    {
        private InvalidKey _invalid_recipients;

        internal EncryptionResult(IntPtr rPtr) {
            _invalid_recipients = null;

            if (rPtr == IntPtr.Zero) {
                throw new InvalidPtrException("An invalid pointer for the encrypt_result structure has been supplied.");
            }
            UpdateFromMem(rPtr);
        }

        public InvalidKey InvalidRecipients {
            get { return _invalid_recipients; }
        }

        private void UpdateFromMem(IntPtr rPtr) {
            var rst = new _gpgme_op_encrypt_result();
            Marshal.PtrToStructure(rPtr, rst);

            if (rst.invalid_recipients != IntPtr.Zero) {
                _invalid_recipients = new InvalidKey(rst.invalid_recipients);
            }
        }
    }
}