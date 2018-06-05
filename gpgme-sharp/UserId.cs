using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Libgpgme.Interop;

namespace Libgpgme
{
    public class UserId : IEnumerable<UserId>
    {
        public bool Revoked { get; private set; }
        public bool Invalid { get; private set; }
        public Validity Validity { get; private set; }
        public string Uid { get; private set; }
        public string Name { get; private set; }
        public string Comment { get; private set; }
        public string Email { get; private set; }
        public KeySignature Signatures { get; private set; }
        public UserId Next { get; private set; }

        internal UserId(IntPtr uidPtr) {
            if (uidPtr == IntPtr.Zero) {
                throw new InvalidPtrException("Invalid user id pointer. Bad programmer! *spank* *spank*");
            }

            UpdateFromMem(uidPtr);
        }
        #region IEnumerable<UserId> Members

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public IEnumerator<UserId> GetEnumerator() {
            UserId id = this;
            while (id != null) {
                yield return id;
                id = id.Next;
            }
        }

        #endregion
        private void UpdateFromMem(IntPtr uidPtr) {
            var userid = (_gpgme_user_id)
                Marshal.PtrToStructure(uidPtr, typeof(_gpgme_user_id));

            Revoked = userid.revoked;
            Invalid = userid.invalid;
            Validity = (Validity) userid.validity;
            Uid = Gpgme.PtrToStringUTF8(userid.uid);
            Name = Gpgme.PtrToStringUTF8(userid.name);
            Comment = Gpgme.PtrToStringUTF8(userid.comment);
            Email = Gpgme.PtrToStringUTF8(userid.email);

            Signatures = userid.signatures != IntPtr.Zero 
                ? new KeySignature(userid.signatures) 
                : null;

            Next = userid.next != IntPtr.Zero 
                ? new UserId(userid.next) 
                : null;
        }

        public override string ToString() {
            var sb = new StringBuilder();
            if (Name != null) {
                sb.Append(Name);
            }
            if (Comment != null) {
                sb.Append(" (");
                sb.Append(Comment);
                sb.Append(")");
            }
            if (Email != null) {
                sb.Append(" <");
                sb.Append(Email);
                sb.Append(">");
            }
            return sb.ToString();
        }
    }
}