using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Libgpgme
{
    public class PgpRevokeSignatureOptions
    {
        public PgpRevokeSignatureReasonCode ReasonCode = PgpRevokeSignatureReasonCode.NoReason;
        public int[] SelectedSignatures;
        public int SelectedUid = 1;
        internal bool cmdSend; // revsig command send to gnupg?
        internal int nreasonTxt;

        internal int nrevokenum;
        internal bool reasonSend;

        internal string[] reasonTxt;
        internal bool uidSend; // uid selected?

        public string ReasonText {
            get {
                if (reasonTxt == null) {
                    return null;
                }
                var sb = new StringBuilder();
                for (int i = 0; i < reasonTxt.Length; i++) {
                    if (i > 0) {
                        sb.Append("\n");
                    }
                    sb.Append(reasonTxt[i]);
                }
                return sb.ToString();
            }
            set {
                var reader = new StringReader(value);
                var lst = new List<string>();
                string line;
                while ((line = reader.ReadLine()) != null) {
                    lst.Add(line);
                }
                reasonTxt = lst.ToArray();
            }
        }
    }
}