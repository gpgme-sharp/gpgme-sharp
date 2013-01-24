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
    public class PgpSubkeyOptions
    {
        public const int KEY_LENGTH_1024 = 1024;
        public const int KEY_LENGTH_2048 = 2048;
        public const int KEY_LENGTH_4096 = 4096;

        private static readonly DateTime _unix_date = new DateTime(1970, 1, 1);

        public PgpSubkeyAlgorithm Algorithm;
        public AlgorithmCapability Capability;
        internal bool cmdSend;

        public int KeyLength { get; set; }
        public DateTime ExpirationDate { get; set; }

        public PgpSubkeyOptions() {
            ExpirationDate = _unix_date;
            KeyLength = KEY_LENGTH_2048;
        }

        public bool IsInfinitely {
            get { return ExpirationDate.Equals(_unix_date); }
        }

        public void MakeInfinitely() {
            ExpirationDate = _unix_date;
        }

        public void SetAlgorithm(KeyAlgorithm algo) {
            switch (algo) {
                case KeyAlgorithm.DSA:
                    Algorithm = PgpSubkeyAlgorithm.DSASignOnly;
                    break;
                case KeyAlgorithm.ELG:
                    Algorithm = PgpSubkeyAlgorithm.ELGEncryptOnly;
                    break;
                case KeyAlgorithm.RSA:
                    Algorithm = PgpSubkeyAlgorithm.RSAUseCapabilities;
                    break;
                default:
                    throw new NotSupportedException("Algorithm is not supported as sub key.",
                        new NotSupportedException("Algorithm is not supported as sub key."));
            }
        }
    }
}