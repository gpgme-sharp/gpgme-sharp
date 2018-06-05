using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Libgpgme.Interop;

namespace Libgpgme
{
    public class NewSignature : IEnumerable<NewSignature>
    {
        private long _timestamp; // long int
        /* The type of the signature.  */

        internal NewSignature(IntPtr sigPtr) {
            if (sigPtr.Equals(IntPtr.Zero)) {
                throw new InvalidPtrException("The pointer to the new signature structure is invalid.");
            }

            UpdateFromMem(sigPtr);
        }

        public NewSignature Next { get; private set; }

        public SignatureMode Type { get; private set; }
        /// <summary>
        /// The public key algorithm used to create the signature. 
        /// </summary>
        public KeyAlgorithm PubkeyAlgorithm { get; private set; }
        /// <summary>
        /// The hash algorithm used to create the signature.
        /// </summary>
        public HashAlgorithm HashAlgorithm { get; private set; }
        /// <summary>
        /// Crypto backend specific signature class.
        /// </summary>
        public long SignatureClass { get; private set; }
        /// <summary>
        /// The fingerprint of the signature.
        /// </summary>
        public string Fingerprint { get; private set; }

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

        #region IEnumerable<NewSignature> Members

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public IEnumerator<NewSignature> GetEnumerator() {
            NewSignature sig = this;
            while (sig != null) {
                yield return sig;
                sig = sig.Next;
            }
        }

        #endregion
        
        private void UpdateFromMem(IntPtr sigPtr) {
            var newsig = new _gpgme_new_signature();
            Marshal.PtrToStructure(sigPtr, newsig);

            Type = (SignatureMode) newsig.type;
            PubkeyAlgorithm = (KeyAlgorithm) newsig.pubkey_algo;
            HashAlgorithm = (HashAlgorithm) newsig.hash_algo;
            Fingerprint = Gpgme.PtrToStringUTF8(newsig.fpr);
            SignatureClass = newsig.sig_class;
            _timestamp = (long) newsig.timestamp;

            if (!newsig.next.Equals(IntPtr.Zero)) {
                Next = new NewSignature(newsig.next);
            }
        }
    }
}