using System.IO;

namespace Libgpgme
{
    public class GpgmeFileData : GpgmeStreamData
    {
        public GpgmeFileData(string filename)
            : this(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None) {
        }

        public GpgmeFileData(string filename, FileMode mode)
            : this(filename, mode, FileAccess.ReadWrite, FileShare.None) {
        }

        public GpgmeFileData(string filename, FileMode mode, FileAccess access)
            : this(filename, mode, access, FileShare.None) {
        }

        public GpgmeFileData(string filename, FileMode mode, FileAccess access, FileShare share)
            : base(
                (access == FileAccess.Read || access == FileAccess.ReadWrite), // CanRead
                (access == FileAccess.ReadWrite || access == FileAccess.Write), // CanWrite
                (access == FileAccess.Read || access == FileAccess.ReadWrite), // CanSeek
                false) // CanRelease
        {
            var finfo = new FileInfo(filename);
            iostream = finfo.Open(mode, access, share);

            // set default filename 
            FileName = finfo.Name;
        }

        public override void Close() {
            if (iostream != null) {
                ((iostream)).Close();
            }

            base.Close();
        }
    }
}