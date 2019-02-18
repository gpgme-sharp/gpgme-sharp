using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Libgpgme.Interop;

namespace Libgpgme
{
    public class InvalidKey : IEnumerable<InvalidKey>
    {
        private int _reason;

        internal InvalidKey(IntPtr sPtr) {
            if (sPtr == IntPtr.Zero) {
                throw new InvalidPtrException("An invalid pointer for the invalid_key structure has been supplied.");
            }
            UpdateFromMem(sPtr);
        }

        public string Fingerprint { get; private set; }

        public int Reason => _reason;

        public InvalidKey Next { get; private set; }

        #region IEnumerable<InvalidKey> Members

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public IEnumerator<InvalidKey> GetEnumerator() {
            InvalidKey key = this;
            while (key != null) {
                yield return key;
                key = key.Next;
            }
        }

        #endregion
        private void UpdateFromMem(IntPtr sPtr) {
            var ikey = new _gpgme_invalid_key();
            Marshal.PtrToStructure(sPtr, ikey);

            Fingerprint = Gpgme.PtrToStringAnsi(ikey.fpr);
            _reason = ikey.reason;

            if (ikey.next != IntPtr.Zero) {
                Next = new InvalidKey(ikey.next);
            }
        }
    }
}