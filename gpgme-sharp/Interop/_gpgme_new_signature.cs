using System;
using System.Runtime.InteropServices;

namespace Libgpgme.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    internal class _gpgme_new_signature // // gpgme_new_signature_t
    {
        /* Signing.  */
        public IntPtr next;

        /* The type of the signature.  */
        public gpgme_sig_mode_t type;
        /* The public key algorithm used to create the signature.  */
        public gpgme_pubkey_algo_t pubkey_algo;
        /* The hash algorithm used to create the signature.  */
        public gpgme_hash_algo_t hash_algo;
        /* Internal to GPGME, do not use.  Must be set to the same value as
            CLASS below.  */
        public IntPtr _obsolete_class;
        /* Signature creation time.  */

        //TODO - stimmt das wirklich?
        public IntPtr timestamp; // long int
        /* The fingerprint of the signature.  */
        public IntPtr fpr; //char *
        public uint _obsolete_class_2;
        /* Crypto backend specific signature class.  */
        public uint sig_class;
    }
}