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