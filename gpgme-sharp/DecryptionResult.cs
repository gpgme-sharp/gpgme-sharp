using System;
using System.Runtime.InteropServices;
using Libgpgme.Interop;

namespace Libgpgme
{
    public class DecryptionResult
    {
        private bool _wrong_key_usage;

        internal DecryptionResult(IntPtr rstPtr) {
            if (rstPtr == IntPtr.Zero) {
                throw new InvalidPtrException("An invalid decryption result structure pointer has been given.");
            }

            UpdateFromMem(rstPtr);
        }

        public string FileName { get; private set; }

        public bool WrongKeyUsage => _wrong_key_usage;

        public string UnsupportedAlgorithm { get; private set; }

        public Recipient Recipients { get; private set; }

        private void UpdateFromMem(IntPtr rstPtr) {
            var rst = new _gpgme_op_decrypt_result();
            Marshal.PtrToStructure(rstPtr, rst);

            FileName = Gpgme.PtrToStringUTF8(rst.file_name);
            _wrong_key_usage = rst.wrong_key_usage;
            UnsupportedAlgorithm = Gpgme.PtrToStringUTF8(rst.unsupported_algorithm);

            if (rst.recipients != IntPtr.Zero) {
                Recipients = new Recipient(rst.recipients);
            }
        }
    }
}