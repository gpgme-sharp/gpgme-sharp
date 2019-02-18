using System;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace Libgpgme
{
    public sealed class KeyParameters
    {
        public const int KEY_LENGTH_1024 = 1024;
        public const int KEY_LENGTH_2048 = 2048;
        public const int KEY_LENGTH_4096 = 4096;

        private static readonly DateTime _unixdate = new DateTime(1970, 1, 1);
        private string _comment = "";
        private string _email = "";
        private const string FORMAT = "internal";

        private string _passphrase = "";
        private KeyAlgorithm _pubkeytype = KeyAlgorithm.DSA;
        private string _realname = "";
        private KeyAlgorithm _subkeytype = KeyAlgorithm.ELG_E;

        /* If set to TRUE: everytime the user changes the *keytype 
         * (algorithm) of the pub or sub key the class
         * automatically finds the correct key usage flags.
         */
        public bool AutoKeyUsage { get; set; } = true;

        public KeyAlgorithm PubkeyAlgorithm {
            get => _pubkeytype;
            set {
                _pubkeytype = value;
                if (AutoKeyUsage) {
                    PubkeyUsage = Gpgme.GetAlgorithmCapability(value);
                }
            }
        }
        public AlgorithmCapability PubkeyUsage { get; set; } = AlgorithmCapability.CanSign | AlgorithmCapability.CanCert | AlgorithmCapability.CanAuth;
        public int KeyLength { get; set; } = KEY_LENGTH_1024;
        public int PubkeyLength {
            get => KeyLength;
            set => KeyLength = value;
        }
        public bool NoSubkey { get; set; }
        public KeyAlgorithm SubkeyAlgorithm {
            get => _subkeytype;
            set {
                _subkeytype = value;
                if (AutoKeyUsage) {
                    SubkeyUsage = Gpgme.GetAlgorithmCapability(value);
                }
            }
        }
        public AlgorithmCapability SubkeyUsage { get; set; } = AlgorithmCapability.CanEncrypt;

        public int SubkeyLength { get; set; } = KEY_LENGTH_1024;
        public string RealName {
            get => _realname;
            set {
                if (CheckForInvalidChars(value)) {
                    throw new InvalidPassphraseException("Real name contains invalid chars.");
                }
                _realname = value;
            }
        }
        public string Comment {
            get => _comment;
            set {
                if (CheckForInvalidChars(value)) {
                    throw new InvalidPassphraseException("Comment contains invalid chars.");
                }
                _comment = value;
            }
        }
        public string Email {
            get => _email;
            set {
                if (CheckForInvalidChars(value)) {
                    throw new InvalidPassphraseException("Email contains invalid chars.");
                }
                _email = value;
            }
        }
        public DateTime ExpirationDate { get; set; } = _unixdate;
        public string Passphrase {
            get => _passphrase;
            set {
                if (CheckForInvalidChars(value)) {
                    throw new InvalidPassphraseException("Passphrase contains invalid chars.");
                }
                _passphrase = value;
            }
        }
        public string NameDN { get; set; }
        public bool IsInfinitely => ExpirationDate.Equals(_unixdate);

        internal string GetXmlText(Protocol protocoltype) {
            var sb = new StringBuilder();

            switch (protocoltype) {
                case Protocol.OpenPGP:
                    // obsolete algorithm
                    if (_pubkeytype == KeyAlgorithm.RSA_S) {
                        throw new InvalidPubkeyAlgoException(
                            "RSA-S is obsolete and therefore not supported. [RFC4880#9.1]");
                    }

                    // invalid algorithm for (primary) asymmetric key
                    if (_pubkeytype == KeyAlgorithm.RSA_E ||
                        _pubkeytype == KeyAlgorithm.ELG_E ||
                            _pubkeytype == KeyAlgorithm.ELG) {
                        throw new InvalidPubkeyAlgoException(
                            "The primary key algorithm must be utilizable to sign. Choose DSA or RSA and specify the key usage attributes.");
                    }

                    // invalid capability attribute
                    if ((PubkeyUsage & AlgorithmCapability.CanSign) != AlgorithmCapability.CanSign) {
                        throw new InvalidPubkeyAlgoException("The primary key must have sign capabilies.");
                    }

                    sb.Append("<GnupgKeyParms format=\"" + FORMAT + "\">\n");
                    sb.Append("Key-Type: " + GetAttrDesc(_pubkeytype) + "\n");
                    sb.Append("Key-Usage: ");
                    sb.Append(AlgorithmCapabilityAttribute.GetKeyUsageText(PubkeyUsage));
                    sb.Append("\n");

                    sb.Append("Key-Length: " + KeyLength + "\n");

                    if (!NoSubkey) {
                        if (_subkeytype == KeyAlgorithm.RSA_E || // RSA-S/E are obsolete [RFC4880#9.1]
                            _subkeytype == KeyAlgorithm.RSA_S) {
                            throw new InvalidPubkeyAlgoException(
                                "RSA-S/E is obsolete and therefore not supported. [RFC4880#9.1]");
                        }

                        sb.Append("Subkey-Type: " + GetAttrDesc(_subkeytype) + "\n");

                        sb.Append("Subkey-Usage: ");
                        sb.Append(AlgorithmCapabilityAttribute.GetKeyUsageText(SubkeyUsage));
                        sb.Append("\n");

                        sb.Append("Subkey-Length: " + SubkeyLength + "\n");
                    }

                    sb.Append("Name-Real: " + _realname + "\n");
                    if (!string.IsNullOrEmpty(_comment)) {
                        sb.Append("Name-Comment: " + _comment + "\n");
                    }
                    sb.Append("Name-Email: " + _email + "\n");
                    if (ExpirationDate.Equals(_unixdate)) {
                        // 0 means the key lifetime is infinitely
                        sb.Append("Expire-Date: 0\n");
                    } else {
                        sb.Append("Expire-Date: " + ExpirationDate.ToString("yyyy-MM-dd") + "\n");
                    }
                    if (!string.IsNullOrEmpty(_passphrase)) {
                        sb.Append("Passphrase: " + _passphrase + "\n");
                    }
                    sb.Append("</GnupgKeyParms>");

                    break;
                case Protocol.CMS:
                    sb.Append("<GnupgKeyParms format=\"" + FORMAT + "\">\n");
                    sb.Append("Key-Type: " + GetAttrDesc(_pubkeytype) + "\n");
                    sb.Append("Key-Length: " + KeyLength + "\n");
                    sb.Append("Name-DN: " + NameDN + "\n");
                    sb.Append("Name-Email: " + _email + "\n");
                    sb.Append("</GnupgKeyParms>");
                    break;
                default:
                    throw new InvalidProtocolException("Invalid protocol");
            }
            return sb.ToString();
        }

        private static string GetAttrDesc<T>(T attr) {
            FieldInfo fieldinf = attr.GetType().GetField(attr.ToString());
            try {
                var attributes = (DescriptionAttribute[]) fieldinf.GetCustomAttributes(
                    typeof(DescriptionAttribute),
                    false);
                return (attributes.Length > 0) ? attributes[0].Description : attr.ToString();
            } catch {
                return "Unknown attribute/description.";
            }
        }

        public void MakeInfinitely() {
            ExpirationDate = _unixdate;
        }

        private bool CheckForInvalidChars(string str) {
            if (str.Contains("\n") ||
                str.Contains("\0")) {
                return true;
            }
            return false;
        }
    }
}