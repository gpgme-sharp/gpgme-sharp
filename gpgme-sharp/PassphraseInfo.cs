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