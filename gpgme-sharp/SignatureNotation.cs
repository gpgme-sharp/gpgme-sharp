using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Libgpgme.Interop;

namespace Libgpgme
{
    public class SignatureNotation : IEnumerable<SignatureNotation>
    {
        private string _value;
        private SignatureNotation _next;

        internal SignatureNotation(IntPtr signotPtr) {
            if (signotPtr == IntPtr.Zero) {
                throw new InvalidPtrException(
                    "The signature notation pointer is invalid. Bad programmer! *spank* *spank*");
            }

            UpdateFromMem(signotPtr);
        }

        public SignatureNotation Next => _next;

        public bool Critical { get; private set; }

        public bool HumanReadable { get; private set; }

        public string Value => _value;

        public string Name { get; private set; }

        public SignatureNotationFlags Flags { get; private set; }

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
                Name = Gpgme.PtrToStringUTF8(signot.name,
                    len);
            }

            Flags = (SignatureNotationFlags) signot.flags;
            Critical = signot.critical;
            HumanReadable = signot.human_readable;

            if (signot.next != IntPtr.Zero) {
                _next = new SignatureNotation(signot.next);
            }
        }
    }
}