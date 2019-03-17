using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Libgpgme.Interop;

namespace Libgpgme
{
    public class Subkey : IEnumerable<Subkey>
    {
        private long _expires;
        private long _timestamp;

        public string KeyId { get; private set; }
        /// <summary>
        /// The keygrip of the subkey in hex digit form or NULL if not availabale.
        /// </summary>
        public string Keygrip { get; private set; }
        public string Fingerprint { get; private set; }
        /// <summary>
        /// The name of the curve for ECC algorithms or NULL.
        /// </summary>
        public string Curve { get; private set; }
        /// <summary>
        /// The serial number of a smart card holding this key or NULL.
        /// </summary>
        public string CardNumber { get; private set; }
        public Subkey Next { get; private set; }
        public bool Revoked { get; private set; }
        public bool Expired { get; private set; }
        public bool Disabled { get; private set; }
        public bool Invalid { get; private set; }
        public bool CanEncrypt { get; private set; }
        public bool CanSign { get; private set; }
        public bool CanCertify { get; private set; }
        public bool Secret { get; private set; }
        public bool CanAuthenticate { get; private set; }
        public bool IsQualified { get; private set; }
        public KeyAlgorithm PubkeyAlgorithm { get; private set; }
        public long Length { get; private set; }

        internal Subkey(IntPtr subkeyPtr) {
            if (subkeyPtr == IntPtr.Zero) {
                throw new InvalidPtrException("Invalid subkey pointer. Bad programmer! *spank* *spank*");
            }

            UpdateFromMem(subkeyPtr);
        }

        public DateTime Timestamp {
            get {
                if (_timestamp < 0) {
                    throw new InvalidTimestampException();
                }
                if (_timestamp == 0) {
                    throw new TimestampNotAvailableException();
                }
                return Gpgme.ConvertFromUnix(_timestamp);
            }
        }
        public DateTime TimestampUTC {
            get {
                if (_timestamp < 0) {
                    throw new InvalidTimestampException();
                }
                if (_timestamp == 0) {
                    throw new TimestampNotAvailableException();
                }
                return Gpgme.ConvertFromUnixUTC(_timestamp);
            }
        }

        public DateTime Expires => Gpgme.ConvertFromUnix(_expires);

        public DateTime ExpiresUTC => Gpgme.ConvertFromUnixUTC(_expires);

        public bool IsInfinitely {
            get => _expires == 0;
            set => throw new NotImplementedException();
        }

        #region IEnumerable<Subkey> Members

        public IEnumerator<Subkey> GetEnumerator() {
            //return new SubkeyEnumerator(this);
            Subkey key = this;
            while (key != null) {
                yield return key;
                key = key.Next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion

        private void UpdateFromMem(IntPtr subkeyPtr) {
            var subkey = (_gpgme_subkey)
                Marshal.PtrToStructure(subkeyPtr,
                    typeof(_gpgme_subkey));

            Revoked = subkey.revoked;
            Expired = subkey.expired;
            Disabled = subkey.disabled;
            Invalid = subkey.invalid;
            CanEncrypt = subkey.can_encrypt;
            CanSign = subkey.can_sign;
            CanCertify = subkey.can_certify;
            CanAuthenticate = subkey.can_authenticate;
            IsQualified = subkey.is_qualified;
            Secret = subkey.secret;

            PubkeyAlgorithm = (KeyAlgorithm) subkey.pubkey_algo;
            Length = subkey.length;

            KeyId = Gpgme.PtrToStringAnsi(subkey.keyid);
            Fingerprint = Gpgme.PtrToStringAnsi(subkey.fpr);
            Curve = Gpgme.PtrToStringAnsi(subkey.curve);
            CardNumber = Gpgme.PtrToStringAnsi(subkey.card_number);
            Keygrip = Gpgme.PtrToStringAnsi(subkey.keygrip);
            _timestamp = (long) subkey.timestamp;
            _expires = (long) subkey.expires;

            if (subkey.next != IntPtr.Zero) {
                Next = new Subkey(subkey.next);
            }
        }
    }
}