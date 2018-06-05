using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Libgpgme.Interop;

namespace Libgpgme
{
    public class Recipient : IEnumerable<Recipient>
    {
        private string _keyid;
        private Recipient _next;
        private KeyAlgorithm _pubkey_algo;
        private int _status;

        internal Recipient(IntPtr recpPtr) {
            if (recpPtr == IntPtr.Zero) {
                throw new InvalidPtrException("An invalid recipient pointer has been given.");
            }

            UpdateFromMem(recpPtr);
        }

        public string KeyId {
            get { return _keyid; }
        }
        public KeyAlgorithm KeyAlgorithm {
            get { return _pubkey_algo; }
        }

        public int Status {
            get { return _status; }
        }
        public Recipient Next {
            get { return _next; }
        }
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

            _keyid = Gpgme.PtrToStringUTF8(recp.keyid);
            _pubkey_algo = (KeyAlgorithm) recp.pubkey_algo;
            _status = recp.status;

            if (recp.next != IntPtr.Zero) {
                _next = new Recipient(recp.next);
            }
        }
    }
}