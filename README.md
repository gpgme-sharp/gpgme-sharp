gpgme-sharp
===========

gpgme-sharp is a C# wrapper around [GPGME](https://wiki.gnupg.org/APIs), the recommended way to use GnuPG within an application. It supports .NET Framework 4.0 and higher, and .NET Core 3.1 and higher.

[![NuGet version](http://img.shields.io/nuget/v/gpgme-sharp.svg)](https://www.nuget.org/packages/gpgme-sharp/)&nbsp;

Requirements
============

- On Windows, you will need to install [Gpg4Win](https://www.gpg4win.org). It is recommended to use Gpg4Win 4.2.0 (released 2023-07-14) or above, as this version added a 64-bit build of GPGME.
- On Debian and Ubuntu, install the [libgpgme11 package](https://packages.debian.org/libgpgme11).
- On other Linux distros or other operating systems, install libgpgme using your favourite package manager, or compile it from source. 

On Windows, gpgme-sharp tries to find the GPGME DLL in your Gpg4Win installation directory, and falls back to the `system32` directory. If your `libgpgme-11.dll` (32-bit) or `libgpgme6-11.dll` (64-bit) file is located in a different location, you can pass the path to the `Context` constructor.

Usage
=====

The library can be installed using NuGet:
```
dotnet add package gpgme-sharp
```

See the [Examples](Examples/) directory in this repo for usage examples.

