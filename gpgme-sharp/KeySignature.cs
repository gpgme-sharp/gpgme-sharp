using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Libgpgme.Interop;

namespace Libgpgme
{
    public class KeySignature : IEnumerable<KeySignature>
    {
        private long _expires;
        private long _timestamp;

        internal KeySignature(IntPtr keysigPtr) {
            if (keysigPtr == IntPtr.Zero) {
                throw new InvalidPtrException("Invalid key signature pointer. Bad programmer! *spank* *spank*");
            }

            UpdateFromMem(keysigPtr);
        }

        public bool Revoked { get; private set; }
        public bool Expired { get; private set; }
        public bool Invalid { get; private set; }
        public bool Exportable { get; private set; }
        public KeyAlgorithm PubkeyAlgorithm { get; private set; }
        public string KeyId { get; private set; }
        public int Status { get; private set; }
        public long SigClass { get; private set; }
        public string Uid { get; private set; }
        public string Name { get; private set; }
        public string Comment { get; private set; }
        public string Email { get; private set; }
        public SignatureNotation Notations { get; private set; }
        public KeySignature Next { get; private set; }

        public DateTime Timestamp {
            get { return Gpgme.ConvertFromUnix(_timestamp); }
        }
        public DateTime TimestampUTC {
            get { return Gpgme.ConvertFromUnixUTC(_timestamp); }
        }

        public DateTime Expires {
            get { return Gpgme.ConvertFromUnix(_expires); }
        }
        public DateTime ExpiresUTC {
            get { return Gpgme.ConvertFromUnixUTC(_expires); }
        }

        public bool IsInfinitely {
            get { return _expires == 0; }
        }
        #region IEnumerable<KeySignature> Members

        public IEnumerator<KeySignature> GetEnumerator() {
            KeySignature keysig = this;
            while (keysig != null) {
                yield return keysig;
                keysig = keysig.Next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion
        private void UpdateFromMem(IntPtr keysigPtr) {
            var keysig = (_gpgme_key_sig) Marshal.PtrToStructure(keysigPtr,
                typeof(_gpgme_key_sig));

            Revoked = keysig.revoked;
            Expired = keysig.expired;
            Invalid = keysig.invalid;
            Exportable = keysig.exportable;

            PubkeyAlgorithm = (KeyAlgorithm) keysig.pubkey_algo;

            KeyId = Gpgme.PtrToStringAnsi(keysig.keyid);
            Uid = Gpgme.PtrToStringAnsi(keysig.uid);
            Name = Gpgme.PtrToStringAnsi(keysig.name);
            Comment = Gpgme.PtrToStringAnsi(keysig.comment);
            Email = Gpgme.PtrToStringAnsi(keysig.email);

            _timestamp = (long) keysig.timestamp;
            _expires = (long) keysig.expires;

            Status = keysig.status;
            SigClass = keysig.sig_class;

            if (keysig.notations != IntPtr.Zero) {
                Notations = new SignatureNotation(keysig.notations);
            }

            if (keysig.next != IntPtr.Zero) {
                Next = new KeySignature(keysig.next);
            }
        }
    }
}