using System;

namespace Libgpgme
{
    public class X509Key : Key
    {
        internal X509Key(IntPtr keyPtr)
            : base(keyPtr) {
        }
    }
}