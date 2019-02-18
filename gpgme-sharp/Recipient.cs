using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Libgpgme.Interop;

namespace Libgpgme
{
    public class Recipient : IEnumerable<Recipient>
    {
        private int _status;

        internal Recipient(IntPtr recpPtr) {
            if (recpPtr == IntPtr.Zero) {
                throw new InvalidPtrException("An invalid recipient pointer has been given.");
            }

            UpdateFromMem(recpPtr);
        }

        public string KeyId { get; private set; }

        public KeyAlgorithm KeyAlgorithm { get; private set; }

        public int Status => _status;

        public Recipient Next { get; private set; }

        #region IEnumerable<Recipient> Members

        public IEnumerator<Recipient> GetEnumerator() {
            Recipient recp = this;
            while (recp != null) {
                yield return recp;
                recp = recp.Next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion
        private void UpdateFromMem(IntPtr recpPtr) {
            var recp = new _gpgme_recipient();
            Marshal.PtrToStructure(recpPtr, recp);

            KeyId = Gpgme.PtrToStringUTF8(recp.keyid);
            KeyAlgorithm = (KeyAlgorithm) recp.pubkey_algo;
            _status = recp.status;

            if (recp.next != IntPtr.Zero) {
                Next = new Recipient(recp.next);
            }
        }
    }
}