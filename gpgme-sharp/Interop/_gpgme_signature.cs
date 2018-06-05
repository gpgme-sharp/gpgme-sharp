using System;
using System.Runtime.InteropServices;

namespace Libgpgme.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    internal class _gpgme_signature // gpgme_signature_t
    {
        public IntPtr next; //_gpgme_signature

        /* A summary of the signature status.  */
        public gpgme_sigsum_t summary;

        /* The fingerprint or key ID of the signature.  */
        public IntPtr fpr; // char *

        /* The status of the signature.  */
        public uint status; //gpgme_error_t

        /* Notation data and policy URLs.  */
        public IntPtr notations; // gpgme_sig_notation_t

        /* Signature creation time.  */
        //UIntPtr
        public UIntPtr timestamp;

        /* Signature exipration time or 0.  */
        //UIntPtr
        public UIntPtr exp_timestamp;

        /* Key should not have been used for signing.  
        uint wrong_key_usage : 1; -> 1

        /* PKA status: 0 = not available, 1 = bad, 2 = okay, 3 = RFU. 
        uint pka_trust : 2; -> 2 & 4

        /* Validity has been verified using the chain model. 
        uint chain_model : 1; -> 8

        /* Internal to GPGME, do not use.  
        uint _unused : 28;
        */
        public uint flags;

        public gpgme_validity_t validity;

        public uint validity_reason; //gpgme_error_t

        /* The public key algorithm used to create the signature.  */
        public gpgme_pubkey_algo_t pubkey_algo;

        /* The hash algorithm used to create the signature.  */
        public gpgme_hash_algo_t hash_algo;

        /* The mailbox from the PKA information or NULL. */
        public IntPtr pka_address; // char *

        public bool wrong_key_usage {
            get { return ((flags & 1) > 0); }
            set {
                if (value) {
                    flags |= 1;
                } else {
                    flags &= (~(uint) 1);
                }
            }
        }
        public PkaStatus pka_trust {
            get { return (PkaStatus) ((flags & 6) >> 1); }
            set { flags = (flags & 0xFFFFFFF9) | (((uint) value) << 1); }
        }
        public bool chain_model {
            get { return ((flags & 8) > 0); }
            set {
                if (value) {
                    flags |= 8;
                } else {
                    flags &= (~(uint) 8);
                }
            }
        }
    }
}