using System;
using System.Runtime.InteropServices;
using Libgpgme.Interop;

namespace Libgpgme
{
    public class DecryptionResult
    {
        private string _file_name;
        private Recipient _recipients;
        private string _unsupported_algorithm;

        private bool _wrong_key_usage;

        internal DecryptionResult(IntPtr rstPtr) {
            if (rstPtr == IntPtr.Zero) {
                throw new InvalidPtrException("An invalid decryption result structure pointer has been given.");
            }

            UpdateFromMem(rstPtr);
        }

        public string FileName {
            get { return _file_name; }
        }

        public bool WrongKeyUsage {
            get { return _wrong_key_usage; }
        }

        public string UnsupportedAlgorithm {
            get { return _unsupported_algorithm; }
        }

        public Recipient Recipients {
            get { return _recipients; }
        }

        private void UpdateFromMem(IntPtr rstPtr) {
            var rst = new _gpgme_op_decrypt_result();
            Marshal.PtrToStructure(rstPtr, rst);

            _file_name = Gpgme.PtrToStringUTF8(rst.file_name);
            _wrong_key_usage = rst.wrong_key_usage;
            _unsupported_algorithm = Gpgme.PtrToStringUTF8(rst.unsupported_algorithm);

            if (rst.recipients != IntPtr.Zero) {
                _recipients = new Recipient(rst.recipients);
            }
        }
    }
}