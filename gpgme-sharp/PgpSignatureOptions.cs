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
    public class PgpSignatureOptions
    {
        private static readonly DateTime _unix_date = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        private int _trustdepth = 1;

        public PgpSignatureClass Class = PgpSignatureClass.Generic;

        public DateTime ExpirationDate = _unix_date;
        public bool LocalPromoteOkay = true;
        public int[] SelectedUids;
        public PgpSignatureTrustLevel TrustLevel = PgpSignatureTrustLevel.Marginal;
        public string TrustRegexp = "";
        public PgpSignatureType Type = PgpSignatureType.Normal;

        internal bool cmdSend; // sign command send to gnupg?
        internal bool forceQuit;
        internal int nUid;
        internal bool signAllUids = true;

        public int TrustDepth {
            get { return _trustdepth; }
            set {
                if (value > 0) {
                    _trustdepth = value;
                } else {
                    throw new GpgmeException("You cannot specify a trust level lower than 1.");
                }
            }
        }

        public bool IsInfinitely {
            get { return ExpirationDate.Equals(_unix_date); }
            set {
                if (value) {
                    ExpirationDate = _unix_date;
                }
            }
        }

        internal string GetExpirationDate() {
            if (ExpirationDate.Equals(_unix_date)) {
                return "0";
            }
            return ExpirationDate.ToString("yyyy-MM-dd");
        }
    }
}