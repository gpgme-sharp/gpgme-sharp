using System;
using System.Runtime.InteropServices;
using GPGME.Native.Shared;
using Microsoft.Win32;

namespace Libgpgme.Interop
{
    internal partial class libgpgme
    {
        internal const int GPGME_ERR_SOURCE_DEFAULT = (int) gpg_err_source_t.GPG_ERR_SOURCE_USER_1;
        internal static string REQUIRE_GPGME = "1.1.6";
        internal static string GNUPG_DIRECTORY = @"C:\Program Files\GNU\GnuPG";
        internal static string GNUPG_LIBNAME = @"libgpgme-11.dll";
        internal static bool USE_LFS_ON_UNIX = true;

        internal static bool use_lfs;
        internal static bool IsWindows;
        internal static string gpgme_version_str;
        internal static GpgmeVersion gpgme_version;

        internal static NativeMethodsWrapper NativeMethods { get; private set; }

        static libgpgme() {
            // On Windows systems we have to add the GnuPG directory to DLL search path
            Win32SetLibdir();

            // Version check required (could fail on Windows systems)
            try {
                InitLibgpgme();
            } catch {
            }
            ;
        }

        internal static int gpgme_err_make(gpg_err_source_t source, gpg_err_code_t code) {
            return libgpgerror.gpg_err_make(source, code);
        }

        internal static int gpgme_error(gpg_err_code_t code) {
            return gpgme_err_make((gpg_err_source_t) GPGME_ERR_SOURCE_DEFAULT, code);
        }

        internal static gpg_err_code_t gpgme_err_code(int err) {
            return libgpgerror.gpg_err_code(err);
        }

        internal static gpg_err_source_t gpgme_err_source(int err) {
            return libgpgerror.gpg_err_source(err);
        }

        /* Windows: add GNUPG directory as library path */

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool SetDllDirectory(string lpPathName);


        internal static bool Win32SetLibdir() {
            if (Environment.OSVersion.Platform.ToString().Contains("Win32") ||
                Environment.OSVersion.Platform.ToString().Contains("Win64")) {
                IsWindows = true;
                string gnupgpath = null;
                try {
                    gnupgpath = (string) Registry.GetValue(
						"HKEY_LOCAL_MACHINE\\SOFTWARE\\GNU\\GnuPG",
                        "Install Directory",
                        null);
                } catch {
                }
                if (gnupgpath != null && !(gnupgpath.Equals(string.Empty))) {
                    return SetDllDirectory(gnupgpath);
                }
                return SetDllDirectory(GNUPG_DIRECTORY);
            }

            return true; // always "true" for UNIX
        }

        internal static void InitLibgpgme() {
            // TODO: This should try Win32.NativeMethods and Unix.NativeMethods and use the one
            // that works, rather than inferring the right one to use based on Platform string.
            if (Environment.OSVersion.Platform.ToString().Contains("Win32") ||
                Environment.OSVersion.Platform.ToString().Contains("Win64")) {
                IsWindows = true;
                NativeMethods = GPGME.Native.Win32.NativeMethods.CreateWrapper();
            } else {
                IsWindows = false;
                NativeMethods = GPGME.Native.Unix.NativeMethods.CreateWrapper();
                if (USE_LFS_ON_UNIX) {
                    // See GPGME manual: 2.3 Largefile Support (LFS)
                    use_lfs = true;
                }
            }

#if REQUIRE_GPGME_VERSION
            gpgme_version = new GpgmeVersion(CheckVersion(REQUIRE_GPGME));
#else
            gpgme_version = new GpgmeVersion(CheckVersion(null));
#endif
        }

        internal static string CheckVersion(string ReqVersion) {
            // we are doing this check only once

            if (gpgme_version_str == null) {
                IntPtr reqver_ptr = IntPtr.Zero;

                if (!string.IsNullOrEmpty(ReqVersion)) {
                    // minimun required version
                    reqver_ptr = Gpgme.StringToCoTaskMemUTF8(ReqVersion);
                }

                // retrieve GPGME's version
                IntPtr ver_ptr = NativeMethods.gpgme_check_version(reqver_ptr);

                if (!reqver_ptr.Equals(IntPtr.Zero)) {
                    Marshal.FreeCoTaskMem(reqver_ptr);
                }

                if (!ver_ptr.Equals(IntPtr.Zero)) {
                    gpgme_version_str = Gpgme.PtrToStringUTF8(ver_ptr);
                } else {
                    throw new GeneralErrorException("Could not retrieve a valid GPGME version.\nGot: "
                        + gpgme_version_str
                            + " Minimum required: " + ReqVersion
                        );
                }
            }
            return gpgme_version_str;
        }
    }
}