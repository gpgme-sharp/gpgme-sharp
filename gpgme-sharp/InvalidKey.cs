/*
 * gpgme-sharp - .NET wrapper classes for libgpgme (GnuPG Made Easy)
 *  Copyright (C) 2008 Daniel Mueller <daniel@danm.de>
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Libgpgme.Interop;

namespace Libgpgme
{
    public class InvalidKey : IEnumerable<InvalidKey>
    {
        private string _fpr;
        private InvalidKey _next;
        private int _reason;

        internal InvalidKey(IntPtr sPtr) {
            if (sPtr == IntPtr.Zero) {
                throw new InvalidPtrException("An invalid pointer for the invalid_key structure has been supplied.");
            }
            UpdateFromMem(sPtr);
        }

        public string Fingerprint {
            get { return _fpr; }
        }

        public int Reason {
            get { return _reason; }
        }
        public InvalidKey Next {
            get { return _next; }
        }
        #region IEnumerable<InvalidKey> Members

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public IEnumerator<InvalidKey> GetEnumerator() {
            InvalidKey key = this;
            while (key != null) {
                yield return key;
                key = key.Next;
            }
        }

        #endregion
        private void UpdateFromMem(IntPtr sPtr) {
            var ikey = new _gpgme_invalid_key();
            Marshal.PtrToStructure(sPtr, ikey);

            _fpr = Gpgme.PtrToStringAnsi(ikey.fpr);
            _reason = ikey.reason;

            if (ikey.next != IntPtr.Zero) {
                _next = new InvalidKey(ikey.next);
            }
        }
    }
}