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
    public class PassphraseInfo
    {
        internal string hintText;
        internal IntPtr hook;
        internal string info;

        public string RequestKeyId { get; private set; }
        public string MainKeyId { get; private set; }
        public KeyAlgorithm PubkeyAlgorithm { get; private set; }
        public int KeyLength { get; private set; }
        public string UidKeyId { get; private set; }
        public string Uid { get; private set; }
        public bool PrevWasBad { get; private set; }

        internal PassphraseInfo(IntPtr hook, string hintText, string info, bool prevwasbad) {
            this.hook = hook;
            this.hintText = hintText;
            this.info = info;
            PrevWasBad = prevwasbad;

            ParseHintText();
            ParseInfo();
        }

        public IntPtr Hook {
            get { return hook; }
        }
        public string HintText {
            get { return hintText; }
        }
        public string Info {
            get { return info; }
        }

        private void ParseHintText() {
            if (hintText != null) {
                int firstspace = hintText.IndexOf(' ');
                if (firstspace > 0) {
                    UidKeyId = hintText.Substring(0, firstspace);
                    Uid = hintText.Substring(
                        firstspace,
                        hintText.Length - firstspace).TrimStart(new[] {' '});
                }
            }
        }

        private void ParseInfo() {
            if (info != null) {
                string[] token = info.Split(' ');
                int len = token.Length;

                if (len > 0) {
                    RequestKeyId = token[0];
                }
                if (len > 1) {
                    MainKeyId = token[1];
                }
                if (len > 2) {
                    int ktype;
                    if (int.TryParse(token[2], out ktype)) {
                        PubkeyAlgorithm = (KeyAlgorithm) ktype;
                    }
                }
                if (len > 3) {
                    int ksize;
                    if (int.TryParse(token[3], out ksize)) {
                        KeyLength = ksize;
                    }
                }
            }
        }
    }
}