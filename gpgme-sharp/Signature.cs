using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Libgpgme.Interop;

namespace Libgpgme
{
    public class Signature : IEnumerable<Signature>
    {
        private UIntPtr _exp_timestamp;
        private uint _status; //gpgme_error_t
        private UIntPtr _timestamp;
        private uint _validity_reason; //gpgme_error_t

        internal Signature(IntPtr sigPtr) {
            if (sigPtr == IntPtr.Zero) {
                throw new InvalidPtrException("An invalid signature pointer has been given.");
            }

            UpdateFromMem(sigPtr);
        }

        public Signature Next { get; private set; }

        public SignatureSummary Summary { get; private set; }

        /* The fingerprint or key ID of the signature.  */
        public string Fingerprint { get; private set; }

        /* The status of the signature.  */
        public long Status => _status;

        /* Notation data and policy URLs.  */
        public SignatureNotation Notations { get; private set; }

        /* Signature creation time.  */
        public Validity Validity { get; private set; }

        public long ValidityReason => _validity_reason;

        /* The public key algorithm used to create the signature.  */
        public KeyAlgorithm PubkeyAlgorithm { get; private set; }

        /* The hash algorithm used to create the signature.  */
        public HashAlgorithm HashAlgorithm { get; private set; }

        /* The mailbox from the PKA information or NULL. */
        public string PKAAddress { get; private set; }

        public bool WrongKeyUsage { get; private set; }

        public PkaStatus PKATrust { get; private set; }

        public bool ChainModel { get; private set; }

        public DateTime Timestamp => Gpgme.ConvertFromUnix((long) _timestamp);

        public DateTime TimestampUTC => Gpgme.ConvertFromUnixUTC((long) _timestamp);

        public DateTime ExpTimestamp => Gpgme.ConvertFromUnix((long) _exp_timestamp);

        public DateTime ExpTimestampUTC => Gpgme.ConvertFromUnixUTC((long) _exp_timestamp);

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

                Summary = (SignatureSummary) unixsig.summary;
                Fingerprint = Gpgme.PtrToStringUTF8(unixsig.fpr);
                _status = unixsig.status;
                _timestamp = unixsig.timestamp;
                _exp_timestamp = unixsig.exp_timestamp;
                Validity = (Validity) unixsig.validity;
                _validity_reason = unixsig.validity_reason;
                PubkeyAlgorithm = (KeyAlgorithm) unixsig.pubkey_algo;
                HashAlgorithm = (HashAlgorithm) unixsig.hash_algo;
                PKAAddress = Gpgme.PtrToStringUTF8(unixsig.pka_address);
                WrongKeyUsage = unixsig.wrong_key_usage;
                PKATrust = unixsig.pka_trust;
                ChainModel = unixsig.chain_model;

                if (unixsig.notations != IntPtr.Zero) {
                    Notations = new SignatureNotation(unixsig.notations);
                }

                if (unixsig.next != IntPtr.Zero) {
                    Next = new Signature(unixsig.next);
                }
            } else {
                var winsig = new _gpgme_signature_windows();
                Marshal.PtrToStructure(sigPtr, winsig);

                Summary = (SignatureSummary) winsig.summary;
                Fingerprint = Gpgme.PtrToStringUTF8(winsig.fpr);
                _status = winsig.status;
                _timestamp = winsig.timestamp;
                _exp_timestamp = winsig.exp_timestamp;
                Validity = (Validity) winsig.validity;
                _validity_reason = winsig.validity_reason;
                PubkeyAlgorithm = (KeyAlgorithm) winsig.pubkey_algo;
                HashAlgorithm = (HashAlgorithm) winsig.hash_algo;
                PKAAddress = Gpgme.PtrToStringUTF8(winsig.pka_address);
                WrongKeyUsage = winsig.wrong_key_usage;
                PKATrust = winsig.pka_trust;
                ChainModel = winsig.chain_model;

                if (winsig.notations != IntPtr.Zero) {
                    Notations = new SignatureNotation(winsig.notations);
                }

                if (winsig.next != IntPtr.Zero) {
                    Next = new Signature(winsig.next);
                }
            }
        }
    }
}