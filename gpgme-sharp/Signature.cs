using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Libgpgme.Interop;

namespace Libgpgme
{
    public class Signature : IEnumerable<Signature>
    {
        private bool _chain_model;
        private UIntPtr _exp_timestamp;
        private string _fpr;
        private HashAlgorithm _hash_algo;
        private Signature _next;
        private SignatureNotation _notations; // gpgme_sig_notation_t
        private string _pka_address; // char *
        private PkaStatus _pka_trust;
        private KeyAlgorithm _pubkey_algo;
        private uint _status; //gpgme_error_t

        /* A summary of the signature status.  */
        private SignatureSummary _summary;
        private UIntPtr _timestamp;
        private Validity _validity;
        private uint _validity_reason; //gpgme_error_t
        private bool _wrong_key_usage;

        internal Signature(IntPtr sigPtr) {
            if (sigPtr == IntPtr.Zero) {
                throw new InvalidPtrException("An invalid signature pointer has been given.");
            }

            UpdateFromMem(sigPtr);
        }

        public Signature Next {
            get { return _next; }
        }

        public SignatureSummary Summary {
            get { return _summary; }
        }

        /* The fingerprint or key ID of the signature.  */
        public string Fingerprint {
            get { return _fpr; }
        }

        /* The status of the signature.  */
        public long Status {
            get { return _status; }
        }

        /* Notation data and policy URLs.  */
        public SignatureNotation Notations {
            get { return _notations; }
        }

        /* Signature creation time.  */
        public Validity Validity {
            get { return _validity; }
        }

        public long ValidityReason {
            get { return _validity_reason; }
        }

        /* The public key algorithm used to create the signature.  */
        public KeyAlgorithm PubkeyAlgorithm {
            get { return _pubkey_algo; }
        }

        /* The hash algorithm used to create the signature.  */
        public HashAlgorithm HashAlgorithm {
            get { return _hash_algo; }
        }

        /* The mailbox from the PKA information or NULL. */
        public string PKAAddress {
            get { return _pka_address; }
        }

        public bool WrongKeyUsage {
            get { return _wrong_key_usage; }
        }

        public PkaStatus PKATrust {
            get { return _pka_trust; }
        }

        public bool ChainModel {
            get { return _chain_model; }
        }

        public DateTime Timestamp {
            get { return Gpgme.ConvertFromUnix((long) _timestamp); }
        }
        public DateTime TimestampUTC {
            get { return Gpgme.ConvertFromUnixUTC((long) _timestamp); }
        }
        public DateTime ExpTimestamp {
            get { return Gpgme.ConvertFromUnix((long) _exp_timestamp); }
        }
        public DateTime ExpTimestampUTC {
            get { return Gpgme.ConvertFromUnixUTC((long) _exp_timestamp); }
        }
        #region IEnumerable<Signature> Members

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public IEnumerator<Signature> GetEnumerator() {
            Signature sig = this;
            while (sig != null) {
                yield return sig;
                sig = sig.Next;
            }
        }

        #endregion
        private void UpdateFromMem(IntPtr sigPtr) {
            /* Work around memory layout problem (bug?) on Windows systems
             * with libgpgme <= 1.1.8
               //_gpgme_signature sig = new _gpgme_signature();
             * 
             */
            if (!libgpgme.IsWindows ||
                (Gpgme.Version.Major >= 1 &&
                    Gpgme.Version.Minor >= 2)
                ) {
                var unixsig = new _gpgme_signature();
                Marshal.PtrToStructure(sigPtr, unixsig);

                _summary = (SignatureSummary) unixsig.summary;
                _fpr = Gpgme.PtrToStringUTF8(unixsig.fpr);
                _status = unixsig.status;
                _timestamp = unixsig.timestamp;
                _exp_timestamp = unixsig.exp_timestamp;
                _validity = (Validity) unixsig.validity;
                _validity_reason = unixsig.validity_reason;
                _pubkey_algo = (KeyAlgorithm) unixsig.pubkey_algo;
                _hash_algo = (HashAlgorithm) unixsig.hash_algo;
                _pka_address = Gpgme.PtrToStringUTF8(unixsig.pka_address);
                _wrong_key_usage = unixsig.wrong_key_usage;
                _pka_trust = unixsig.pka_trust;
                _chain_model = unixsig.chain_model;

                if (unixsig.notations != IntPtr.Zero) {
                    _notations = new SignatureNotation(unixsig.notations);
                }

                if (unixsig.next != IntPtr.Zero) {
                    _next = new Signature(unixsig.next);
                }
            } else {
                var winsig = new _gpgme_signature_windows();
                Marshal.PtrToStructure(sigPtr, winsig);

                _summary = (SignatureSummary) winsig.summary;
                _fpr = Gpgme.PtrToStringUTF8(winsig.fpr);
                _status = winsig.status;
                _timestamp = winsig.timestamp;
                _exp_timestamp = winsig.exp_timestamp;
                _validity = (Validity) winsig.validity;
                _validity_reason = winsig.validity_reason;
                _pubkey_algo = (KeyAlgorithm) winsig.pubkey_algo;
                _hash_algo = (HashAlgorithm) winsig.hash_algo;
                _pka_address = Gpgme.PtrToStringUTF8(winsig.pka_address);
                _wrong_key_usage = winsig.wrong_key_usage;
                _pka_trust = winsig.pka_trust;
                _chain_model = winsig.chain_model;

                if (winsig.notations != IntPtr.Zero) {
                    _notations = new SignatureNotation(winsig.notations);
                }

                if (winsig.next != IntPtr.Zero) {
                    _next = new Signature(winsig.next);
                }
            }
        }
    }
}