using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Libgpgme.Interop;

namespace Libgpgme
{
    public class SignatureNotation : IEnumerable<SignatureNotation>
    {
        private string _name, _value;
        private bool _critical;
        private SignatureNotationFlags _flags;
        private bool _human_readable;
        private SignatureNotation _next;

        internal SignatureNotation(IntPtr signotPtr) {
            if (signotPtr == IntPtr.Zero) {
                throw new InvalidPtrException(
                    "The signature notation pointer is invalid. Bad programmer! *spank* *spank*");
            }

            UpdateFromMem(signotPtr);
        }

        public SignatureNotation Next {
            get { return _next; }
        }
        public bool Critical {
            get { return _critical; }
        }
        public bool HumanReadable {
            get { return _human_readable; }
        }
        public string Value {
            get { return _value; }
        }
        public string Name {
            get { return _name; }
        }
        public SignatureNotationFlags Flags {
            get { return _flags; }
        }
        #region IEnumerable<SignatureNotation> Members

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public IEnumerator<SignatureNotation> GetEnumerator() {
            SignatureNotation signot = this;
            while (signot != null) {
                yield return signot;
                signot = signot.Next;
            }
        }

        #endregion
        private void UpdateFromMem(IntPtr signotPtr) {
            int len;
            var signot = (_gpgme_sig_notation) Marshal.PtrToStructure(signotPtr,
                typeof(_gpgme_sig_notation));

            if (signot.value != IntPtr.Zero) {
                len = signot.value_len;
                _value = Gpgme.PtrToStringUTF8(signot.value,
                    len);
            }
            if (signot.name != IntPtr.Zero) {
                len = signot.name_len;
                _name = Gpgme.PtrToStringUTF8(signot.name,
                    len);
            }

            _flags = (SignatureNotationFlags) signot.flags;
            _critical = signot.critical;
            _human_readable = signot.human_readable;

            if (signot.next != IntPtr.Zero) {
                _next = new SignatureNotation(signot.next);
            }
        }
    }
}