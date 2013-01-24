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

namespace Libgpgme
{
    public class PgpExpirationOptions
    {
        private static readonly DateTime _unix_date = new DateTime(1970, 1, 1);
        private DateTime _expiration_date = _unix_date;

        public int[] SelectedSubkeys; // if not set - expire the whole key (pub SC)
        
        internal bool cmdSend;
        internal bool forceQuit;
        internal int nsubkey;

        public bool IsInfinitely {
            get { return _expiration_date.Equals(_unix_date); }
        }

        public DateTime ExpirationDate {
            get { return _expiration_date; }
            set { _expiration_date = value; }
        }

        public void MakeInfinitely() {
            _expiration_date = _unix_date;
        }
    }
}