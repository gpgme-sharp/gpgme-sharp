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
    public class GpgmeVersion
    {
        private readonly int _major;
        private readonly int _minor;
        private readonly int _update;
        private readonly string _version;

        public GpgmeVersion(string version) {
            if (version == null) {
                throw new ArgumentNullException("version");
            }
            _version = version;

            string[] tup = version.Split('.');
            if (tup.Length >= 3) {
                int.TryParse(tup[0], out _major);
                int.TryParse(tup[1], out _minor);
                int.TryParse(tup[2], out _update);
            }
        }

        public int Major {
            get { return _major; }
        }
        public int Minor {
            get { return _minor; }
        }
        public int Update {
            get { return _update; }
        }
        public string Version {
            get { return _version; }
        }
    }
}