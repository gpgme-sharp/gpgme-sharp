using System;
using System.Runtime.InteropServices;
using Libgpgme.Interop;

namespace Libgpgme
{
    public class GenkeyResult
    {
        private string _fingerprint;
        private bool _has_primary;
        private bool _has_sub;

        internal GenkeyResult(IntPtr keyresultPtr) {
            if (keyresultPtr == IntPtr.Zero) {
                throw new InvalidPtrException("An invalid key result pointer has been given." +
                    " Bad programmer! *spank* *spank*");
            }
            UpdateFromMem(keyresultPtr);
        }

        public string Fingerprint {
            get { return _fingerprint; }
        }
        public bool HasPrimary {
            get { return _has_primary; }
        }
        public bool HasSub {
            get { return _has_sub; }
        }

        private void UpdateFromMem(IntPtr keyresultPtr) {
            var result = new _gpgme_op_genkey_result();
            Marshal.PtrToStructure(keyresultPtr, result);
            _fingerprint = Gpgme.PtrToStringAnsi(result.fpr);
            _has_primary = result.primary;
            _has_sub = result.sub;
        }
    }
}