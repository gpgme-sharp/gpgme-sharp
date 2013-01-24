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
    public class Recipient : IEnumerable<Recipient>
    {
        private string _keyid;
        private Recipient _next;
        private KeyAlgorithm _pubkey_algo;
        private int _status;

        internal Recipient(IntPtr recpPtr) {
            if (recpPtr == IntPtr.Zero) {
                throw new InvalidPtrException("An invalid recipient pointer has been given.");
            }

            UpdateFromMem(recpPtr);
        }

        public string KeyId {
            get { return _keyid; }
        }
        public KeyAlgorithm KeyAlgorithm {
            get { return _pubkey_algo; }
        }

        public int Status {
            get { return _status; }
        }
        public Recipient Next {
            get { return _next; }
        }
        #region IEnumerable<Recipient> Members

        public IEnumerator<Recipient> GetEnumerator() {
            Recipient recp = this;
            while (recp != null) {
                yield return recp;
                recp = recp.Next;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion
        private void UpdateFromMem(IntPtr recpPtr) {
            var recp = new _gpgme_recipient();
            Marshal.PtrToStructure(recpPtr, recp);

            _keyid = Gpgme.PtrToStringUTF8(recp.keyid);
            _pubkey_algo = (KeyAlgorithm) recp.pubkey_algo;
            _status = recp.status;

            if (recp.next != IntPtr.Zero) {
                _next = new Recipient(recp.next);
            }
        }
    }
}