gpgme-sharp
===========

gpgme-sharp is a C# wrapper around [GPGME](https://wiki.gnupg.org/APIs), the recommended way to use GnuPG within an application. It supports .NET Framework 4.0 and higher, and [.NET Standard](https://docs.microsoft.com/en-us/dotnet/standard/net-standard) 2.0 (including .NET Core 2.0).

[![NuGet version](http://img.shields.io/nuget/v/gpgme-sharp.svg)](https://www.nuget.org/packages/gpgme-sharp/)&nbsp;

Requirements
============

- On Windows, you will need to install [Gpg4Win](https://www.gpg4win.org). 
- On Debian and Ubuntu, install the [libgpgme11 package](https://packages.debian.org/stretch/libgpgme11).
- On other Linux distros or other operating systems, install libgpgme using your favourite package manager, or compile it from source. 

Note that Gpg4Win currently only distributes a 32-bit build, so on Windows you **must** set your C# app to run in 32-bit mode.

Usage
=====

The library can be installed using NuGet:
```
dotnet add package gpgme-sharp
```

See the [Examples](Examples/) directory in this repo for usage examples.

