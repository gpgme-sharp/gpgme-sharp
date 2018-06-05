using System;
using System.Runtime.InteropServices;
using Libgpgme.Interop;

namespace Libgpgme
{
    public class TrustItem
    {
        internal IntPtr itemPtr = IntPtr.Zero;

        public string KeyId { get; private set; }
        public TrustItemType Type { get; private set; }
        public int Level { get; private set; }
        public string OwnerTrust { get; private set; }
        public string Validity { get; private set; }
        public string Name { get; private set; }

        internal TrustItem(IntPtr itemPtr) {
            if (itemPtr.Equals(IntPtr.Zero)) {
                throw new InvalidPtrException("An invalid trust item pointer has been supplied.");
            }

            UpdateFromMem(itemPtr);
        }

        ~TrustItem() {
            if (itemPtr != IntPtr.Zero) {
                // remove trust item reference
                libgpgme.gpgme_trust_item_unref(itemPtr);
                itemPtr = IntPtr.Zero;
            }
        }

        private void UpdateFromMem(IntPtr itemPtr) {
            var titem = new _gpgme_trust_item();
            Marshal.PtrToStructure(itemPtr, titem);

            KeyId = Gpgme.PtrToStringAnsi(titem.keyid);
            switch (titem.type) {
                case 1:
                    Type = TrustItemType.Key;
                    break;
                case 2:
                    Type = TrustItemType.UserId;
                    break;
                default:
                    throw new GeneralErrorException("Unknown trust item type value of " + titem.type);
            }
            Level = titem.level;
            OwnerTrust = Gpgme.PtrToStringUTF8(titem.owner_trust);
            Validity = Gpgme.PtrToStringAnsi(titem.validity);
            Name = Gpgme.PtrToStringUTF8(titem.name);
        }
    }
}