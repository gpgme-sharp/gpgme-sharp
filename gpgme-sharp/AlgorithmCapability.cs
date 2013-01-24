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
using System.Collections.Generic;

namespace Libgpgme
{
    public class AlgorithmCapabilityAttribute : Attribute
    {
        protected AlgorithmCapability _type;

        public AlgorithmCapabilityAttribute(AlgorithmCapability type) {
            _type = type;
        }

        public AlgorithmCapability Type {
            get { return _type; }
        }

        internal static string GetKeyUsageText(AlgorithmCapability type) {
            var caps = new List<string>();
            if ((type & AlgorithmCapability.CanAuth) == AlgorithmCapability.CanAuth) {
                caps.Add("auth");
            }
            if ((type & AlgorithmCapability.CanSign) == AlgorithmCapability.CanSign) {
                caps.Add("sign");
            }
            if ((type & AlgorithmCapability.CanEncrypt) == AlgorithmCapability.CanEncrypt) {
                caps.Add("encrypt");
            }
            return caps.Count > 0 
                ? string.Join(",", caps.ToArray()) 
                : null;
        }
    }
}