using System;
using System.Runtime.InteropServices;

namespace Libgpgme.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    internal class _gpgme_recipient // gpgme_recipient_t;
    {
        /* Decryption.  */
        public IntPtr next; //_gpgme_recipient

        /* The key ID of key for which the text was encrypted.  */
        public IntPtr keyid; //char *

        /* Internal to GPGME, do not use.  */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
        public byte[] _keyid;

        /* The public key algorithm of the recipient key.  */
        public gpgme_pubkey_algo_t pubkey_algo;

        /* The status of the recipient.  */
        public int status; //gpgme_error_t

        public _gpgme_recipient() {
            _keyid = new byte[16 + 1];
        }
    }
}