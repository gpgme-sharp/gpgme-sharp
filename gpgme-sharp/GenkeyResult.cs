using System;
using System.Runtime.InteropServices;
using Libgpgme.Interop;

namespace Libgpgme
{
    public class GenkeyResult
    {
        private bool _has_sub;

        internal GenkeyResult(IntPtr keyresultPtr) {
            if (keyresultPtr == IntPtr.Zero) {
                throw new InvalidPtrException("An invalid key result pointer has been given." +
                    " Bad programmer! *spank* *spank*");
            }
            UpdateFromMem(keyresultPtr);
        }

        public string Fingerprint { get; private set; }

        public bool HasPrimary { get; private set; }

        public bool HasSub => _has_sub;

        private void UpdateFromMem(IntPtr keyresultPtr) {
            var result = new _gpgme_op_genkey_result();
            Marshal.PtrToStructure(keyresultPtr, result);
            Fingerprint = Gpgme.PtrToStringAnsi(result.fpr);
            HasPrimary = result.primary;
            _has_sub = result.sub;
        }
    }
}