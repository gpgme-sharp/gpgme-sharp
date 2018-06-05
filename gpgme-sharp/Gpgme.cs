#define REQUIRE_GPGME_VERSION

using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Libgpgme.Interop;

namespace Libgpgme
{
    public sealed class Gpgme
    {
        public static GpgmeVersion Version {
            get { return libgpgme.gpgme_version; }
        }

        public static string CheckVersion() {
            return libgpgme.gpgme_version_str;
        }


        public static string GetProtocolName(Protocol proto) {
            IntPtr ret = libgpgme.gpgme_get_protocol_name((gpgme_protocol_t) proto);

            if (ret == IntPtr.Zero) {
                throw new InvalidProtocolException("The specified protocol is invalid.");
            }

            return PtrToStringAnsi(ret);
        }

        public static bool EngineCheckVersion(Protocol proto) {
            int err = libgpgme.gpgme_engine_check_version((gpgme_protocol_t) proto);

            gpg_err_code_t errcode = libgpgme.gpgme_err_code(err);

            if (errcode == gpg_err_code_t.GPG_ERR_NO_ERROR) {
                return true;
            }
            return false;
        }

        public static EngineInfo GetEngineInfo() {
            IntPtr info_ptr;

            int err = libgpgme.gpgme_get_engine_info(out info_ptr);
            gpg_err_code_t errcode = libgpgme.gpgme_err_code(err);

            if (errcode != gpg_err_code_t.GPG_ERR_NO_ERROR) {
                throw new GpgmeException("System error: "
                    + err.ToString(CultureInfo.InvariantCulture), err);
            }

            EngineInfo info = null;
            if (info_ptr != IntPtr.Zero) {
                info = new EngineInfo(info_ptr);
            }

            return info;
        }

        internal static string PtrToStringAnsi(IntPtr ptr) {
            string str = null;
            if (ptr != IntPtr.Zero) {
                str = Marshal.PtrToStringAnsi(ptr);
                if (str != null) {
                    str = str.Replace("\r", "");
                }
            }
            return str;
        }

        internal static string PtrToStringAnsi(IntPtr ptr, int len) {
            string str = null;
            if (ptr != IntPtr.Zero) {
                str = Marshal.PtrToStringAnsi(ptr, len);
            }
            return str;
        }

        internal static string PtrToStringUTF8(IntPtr ptr) {
            if (ptr != IntPtr.Zero) {
                // calculate utf8 string size
                int size = 0;
                while (Marshal.ReadByte((IntPtr) ((long) ptr + size)) != '\0') {
                    size++;
                }
                return PtrToStringUTF8(ptr, size);
            }
            return null;
        }

        internal static string PtrToStringUTF8(IntPtr ptr, int size) {
            if (ptr != IntPtr.Zero && size > 0) {
                var barray = new byte[size];

                // copy utf8 encoded string to byte array
                Marshal.Copy(ptr, barray, 0, size);

                // convert the UTF8 encoded string to unicode
                var utf8_encoding = new UTF8Encoding();
                int newsize = utf8_encoding.GetCharCount(barray);
                var darray = new char[newsize];
                Decoder decoder = utf8_encoding.GetDecoder();
                int bytes_used, chars_used;
                bool completed;
                
                decoder.Convert(
                    barray, 
                    0, 
                    barray.Length, 
                    darray, 
                    0, 
                    newsize, 
                    true, 
                    out bytes_used, 
                    out chars_used,
                    out completed);

                return new string(darray);
            }
            return null;
        }

        internal static IntPtr StringToCoTaskMemUTF8(string str) {
            if (str == null) {
                return IntPtr.Zero;
            }
            var utf8 = new UTF8Encoding();
            char[] carray = str.ToCharArray();

            int size = utf8.GetByteCount(carray);

            // Encode unicode string to UTF8 byte array
            var barray = new byte[size + 1]; // + Null char
            Encoder encoder = utf8.GetEncoder();

            if (size > 0) {
                int bytes_used;
                int chars_used;
                bool completed;
                encoder.Convert(
                    carray, 0, carray.Length, // source (UTF8 encoded string)
                    barray, 0, size, // destination (bytes)
                    true,
                    out chars_used,
                    out bytes_used,
                    out completed);
            }

            IntPtr ptr = Marshal.AllocCoTaskMem(size + 1);
            if (ptr == IntPtr.Zero) {
                throw new OutOfMemoryException("Could not allocate " + size + " bytes of memory.");
            }

            Marshal.Copy(barray, 0, ptr, size + 1);

            return ptr;
        }

        internal static byte[] ConvertCharArrayAnsi(char[] carray) {
            if (carray == null) {
                return null;
            }

            var b = new byte[carray.Length];
            for (int i = 0; i < carray.Length; i++) {
                b[i] = (byte) carray[i];
            }

            return b;
        }

        internal static byte[] ConvertCharArrayToUTF8(char[] carray, int additionalsize) {
            if (carray == null) {
                return null;
            }

            var utf8 = new UTF8Encoding();
            int size = utf8.GetByteCount(carray);

            // Encode unicode string to UTF8 encoded byte array
            var barray = new byte[size + additionalsize];

            Encoder encoder = utf8.GetEncoder();

            int chars_used, bytes_used;
            bool completed;

            encoder.Convert(
                carray, 0, carray.Length, // source
                barray, 0, size, // destination
                true,
                out chars_used,
                out bytes_used,
                out completed);

            return barray;
        }

        internal static IntPtr[] KeyArrayToIntPtrArray(Key[] keyarray) {
            if (keyarray == null) {
                return null;
            }
            var parray = new IntPtr[keyarray.Length + 1];
            for (int i = 0; i < keyarray.Length; i++) {
                parray[i] = keyarray[i].KeyPtr;
            }
            parray[keyarray.Length] = IntPtr.Zero;

            return parray;
        }

        internal static IntPtr[] StringToCoTaskMemUTF8(string[] strarray) {
            if (strarray == null || strarray.Length == 0) {
                return null;
            }
            var parray = new IntPtr[strarray.Length + 1];
            for (int i = 0; i < strarray.Length; i++) {
                parray[i] = StringToCoTaskMemUTF8(strarray[i]);
            }
            parray[strarray.Length] = IntPtr.Zero;

            return parray;
        }

        internal static void FreeStringArray(IntPtr[] parray) {
            if (parray == null) {
                return;
            }
            for (int i = 0; i < parray.Length; i++) {
                if (!(parray[i].Equals(IntPtr.Zero))) {
                    Marshal.FreeCoTaskMem(parray[i]);
                    parray[i] = IntPtr.Zero;
                }
            }
        }

        public static string GetPubkeyAlgoName(KeyAlgorithm algo) {
            IntPtr ret = libgpgme.gpgme_pubkey_algo_name((gpgme_pubkey_algo_t) algo);
            if (ret == IntPtr.Zero) {
                throw new InvalidPubkeyAlgoException("The public key algorithm is unknown.");
            }

            return PtrToStringAnsi(ret);
        }

        public static string GetHashAlgoName(HashAlgorithm algo) {
            IntPtr ret = libgpgme.gpgme_hash_algo_name((gpgme_hash_algo_t) algo);
            if (ret == IntPtr.Zero) {
                throw new InvalidHashAlgoException("The hash algorithm is unknown.");
            }

            return PtrToStringAnsi(ret);
        }

        public static AlgorithmCapability GetAlgorithmCapability<T>(T attr) {
            FieldInfo fieldinf = attr.GetType().GetField(attr.ToString());
            var types = (AlgorithmCapabilityAttribute[])
                fieldinf.GetCustomAttributes(
                    typeof(AlgorithmCapabilityAttribute), false);
            return (types.Length > 0) ? types[0].Type : AlgorithmCapability.CanNothing;
        }

        public static string GetAttrDesc<T>(T attr) {
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

        public static string GetStrError(int err) {
            lock ("_GnuPG.Lib.libgpgme.GetStrError") {
                IntPtr ret = libgpgme.gpgme_strerror(err);

                return PtrToStringUTF8(ret);
            }
        }

        public static void GetStrError(int err, out string message) {
            const int ERANGE = 34;
            int bufsize = 512;
            IntPtr ptr = IntPtr.Zero;
            int reterr;
            do {
                if (!ptr.Equals(IntPtr.Zero)) {
                    Marshal.FreeCoTaskMem(ptr);
                    bufsize *= 2;
                }
                ptr = Marshal.AllocCoTaskMem(bufsize);
                reterr = libgpgme.gpgme_strerror_r(err, ptr, (UIntPtr) bufsize);
            } while (reterr == ERANGE);

            if (ptr != IntPtr.Zero) {
                message = PtrToStringUTF8(ptr);
                Marshal.FreeCoTaskMem(ptr);
            } else {
                message = null;
            }
        }

        public static string GetStrSource(int err) {
            IntPtr ret = libgpgme.gpgme_strsource(err);

            return PtrToStringAnsi(ret);
        }

        /// <summary>
        /// Creates a new GPGME context.
        /// </summary>
        /// <returns></returns>
        public static Context CreateContext() {
            return new Context();
        }

        internal static DateTime ConvertFromUnix(long timestamp) {
            var unixdate = new DateTime(1970, 1, 1, 0, 0, 0, 0);

            // difference between UTC an local time
            var t = new TimeSpan(DateTime.UtcNow.Ticks - DateTime.Now.Ticks);
            return unixdate.AddSeconds(timestamp - t.TotalSeconds);
        }

        internal static DateTime ConvertFromUnixUTC(long timestamp) {
            var unixdate = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return unixdate.AddSeconds(timestamp);
        }

        internal static long ConvertToUnix(DateTime tm) {
            TimeSpan t = (tm - new DateTime(1970, 1, 1));
            var timestamp = (int) t.TotalSeconds;
            return timestamp;
        }
    }
}