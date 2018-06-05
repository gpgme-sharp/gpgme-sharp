using System;
using System.Runtime.InteropServices;

namespace Libgpgme.Interop
{
    /* A signature on a user ID.  */

    [StructLayout(LayoutKind.Sequential)]
    internal class _gpgme_key_sig //// *gpgme_key_sig_t;
    {
        public IntPtr next;
        /* True if the signature is a revocation signature.  
              unsigned int revoked : 1;

           True if the signature is expired.  
              unsigned int expired : 1;

           True if the signature is invalid.  
              unsigned int invalid : 1;

           True if the signature should be exported.  
              unsigned int exportable : 1;

           Internal to GPGME, do not use.  
              unsigned int _unused : 28;
         */
        public uint flags;

        /* The public key algorithm used to create the signature.  */
        public gpgme_pubkey_algo_t pubkey_algo;

        /* The key ID of key used to create the signature.  */
        public IntPtr keyid;

        /* Internal to GPGME, do not use.  */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
        public byte[] _keyid;

        /* The creation timestamp, -1 if invalid, 0 if not available.  */
        public IntPtr timestamp;

        /* The expiration timestamp, 0 if the subkey does not expire.  */
        public IntPtr expires;

        /* Same as in gpgme_signature_t.  */
        public int status;

        /* Must be set to SIG_CLASS below.  */
        public uint iclass;

        /* The user ID string.  */
        public IntPtr uid; // char*

        /* The name part of the user ID.  */
        public IntPtr name; // char*

        /* The email part of the user ID.  */
        public IntPtr email; // char*

        /* The comment part of the user ID.  */
        public IntPtr comment; // char*

        /* Crypto backend specific signature class.  */
        public uint sig_class;

        /* Notation data and policy URLs.  */
        //gpgme_sig_notation_t notations;
        public IntPtr notations;

        /* Internal to GPGME, do not use.  */
        //gpgme_sig_notation_t _last_notation;
        public IntPtr _last_notation;

        public _gpgme_key_sig() {
            // create buffer!
            _keyid = new byte[17];

            next = IntPtr.Zero;
        }

        public bool revoked {
            get { return ((flags & 1) > 0); }
            set {
                if (value) {
                    flags |= 1;
                } else {
                    flags &= (~(uint) 1);
                }
            }
        }

        public bool expired {
            get { return ((flags & 2) > 0); }
            set {
                if (value) {
                    flags |= 2;
                } else {
                    flags &= (~(uint) 2);
                }
            }
        }


        public bool invalid {
            get { return ((flags & 4) > 0); }
            set {
                if (value) {
                    flags |= 4;
                } else {
                    flags &= (~(uint) 4);
                }
            }
        }

        public bool exportable {
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