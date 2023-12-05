using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace Libgpgme.Interop
{
    /// <summary>
    /// Handles finding the right path to import the GPGME DLL from. The default paths can be
    /// overridden by adding paths to <see cref="ExtraPotentialPaths"/>.
    /// </summary>
    internal abstract class DllImportResolver
    {
#if !NETCOREAPP
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool SetDllDirectory(string lpPathName);
#endif

        private static string _dllName = Environment.Is64BitProcess
            ? NativeMethods.WINDOWS_LIBRARY_64_BIT_NAME
            : NativeMethods.WINDOWS_LIBRARY_NAME;

        /// <summary>
        /// User-supplied paths to look for the GPGME DLL in.
        /// </summary>
        public static IList<string> ExtraPotentialPaths { get; } = new List<string>();

        public static void Configure()
        {
#if NETCOREAPP
            NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), (name, assembly, path) =>
            {
                if (name != NativeMethods.WINDOWS_LIBRARY_NAME)
                {
                    // Not a request for GPGME - Fall back to standard resolver.
                    return IntPtr.Zero;
                }

                if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                {
                    // Non-Windows platform - Use the UNIX library name.
                    return NativeLibrary.Load(NativeMethods.UNIX_LIBRARY_NAME);
                }
                
                return NativeLibrary.Load(FindDllPath());
            });
#else
            var isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
            if (!isWindows)
            {
                return;
            }

            var dllDir = Path.GetDirectoryName(FindDllPath());
            SetDllDirectory(dllDir);
#endif
        }

        private static string FindDllPath()
        {
            var potentialPaths = GetPotentialPaths()
                .Distinct()
                .Where(path => path != null)
                .Select(path => Path.Combine(path, _dllName));

            // ReSharper disable PossibleMultipleEnumeration - Error case is unlikely
            foreach (var potentialPath in potentialPaths)
            {
                if (File.Exists(potentialPath))
                {
                    return potentialPath;
                }
            }

            throw new LibraryNotFoundException(_dllName, potentialPaths);
            // ReSharper restore PossibleMultipleEnumeration
        }

        private static IEnumerable<string> GetPotentialPaths()
        {
            // Custom user-supplied paths
            foreach (var path in ExtraPotentialPaths)
            {
                yield return path;
            }

            var binDirMaybe64 = Environment.Is64BitProcess ? "bin_64" : "bin";
            using var hklm32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);

            // Paths from registry
            yield return TryGetPathFromRegistry(hklm32, @"SOFTWARE\Gpg4win", "Install Directory", binDirMaybe64);
            yield return TryGetPathFromRegistry(hklm32, @"SOFTWARE\GnuPG", "Install Directory", "bin");
            yield return TryGetPathFromRegistry(hklm32, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Gpg4win", "InstallLocation", binDirMaybe64);
            yield return TryGetPathFromRegistry(hklm32, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\GnuPG", "InstallLocation", "bin");

            // Default paths
            var programFilesx86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            yield return Path.Combine(programFilesx86, @"Gpg4Win", binDirMaybe64);
            yield return Path.Combine(programFilesx86, @"GnuPG", "bin");

            // General fallback
            yield return Environment.GetFolderPath(Environment.SpecialFolder.System);
        }

        private static string TryGetPathFromRegistry(
            RegistryKey registryKey,
            string subKeyName,
            string name,
            string suffix
        )
        {
            try
            {
                using var subKeyView = registryKey.OpenSubKey(subKeyName);
                return subKeyView?.GetValue(name) is not string value 
                    ? null 
                    : Path.Combine(value, suffix);
            }
            catch
            {
                return null;
            }
        }
    }
}
