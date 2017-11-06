using System;
using System.IO;
using System.Runtime.InteropServices;

namespace FFmpeg.AutoGen.Native
{
    public static class LibraryLoader
    {
        /// <summary>
        ///     Attempts to load a native library using platform nammig convention.
        /// </summary>
        /// <param name="path">Path of the library.</param>
        /// <param name="libraryName">Name of the library.</param>
        /// <param name="version">Version of the library.</param>
        /// <returns>
        ///     A handle to the library when found; otherwise, <see cref="IntPtr.Zero" />.
        /// </returns>
        /// <remarks>
        ///     This function may return a null handle. If it does, individual functions loaded from it will throw a
        ///     DllNotFoundException,
        ///     but not until an attempt is made to actually use the function (rather than load it). This matches how PInvokes
        ///     behave.
        /// </remarks>
        public static IntPtr LoadNativeLibraryUsingPlatformNamingConvention(string path, string libraryName, int version)
        {
#if NET45
            var fullName = Path.Combine(path, $"{libraryName}-{version}.dll");
            return LoadNativeLibrary(fullName);
#else
			string fullName = "";
			switch(ffmpeg.GetPlatform()){
				case PlatformID.MacOSX:
					fullName = Path.Combine(path, $"{libraryName}.{version}.dylib");
					return LoadNativeLibrary(fullName);
				case PlatformID.Win32NT:
				case PlatformID.Win32Windows:
				case PlatformID.Win32S:
					fullName = Path.Combine(path, $"{libraryName}-{version}.dll");
					return LoadNativeLibrary(fullName);
				case PlatformID.Unix:
					fullName = Path.Combine(path, $"{libraryName}.so{version}");
					return LoadNativeLibrary(fullName);
			}

            throw new PlatformNotSupportedException();
#endif
        }

        /// <summary>
        ///     Attempts to load a native library.
        /// </summary>
        /// <param name="path">Path of the library.</param>
        /// <param name="libraryName">Name of the library.</param>
        /// <param name="version">Version of the library.</param>
        /// <returns>
        ///     A handle to the library when found; otherwise, <see cref="IntPtr.Zero" />.
        /// </returns>
        /// <remarks>
        ///     This function may return a null handle. If it does, individual functions loaded from it will throw a
        ///     DllNotFoundException,
        ///     but not until an attempt is made to actually use the function (rather than load it). This matches how PInvokes
        ///     behave.
        /// </remarks>
        public static IntPtr LoadNativeLibrary(string libraryName)
        {
#if NET45
            return WindowsNativeMethods.LoadLibrary(libraryName);
#else
			IntPtr lib = IntPtr.Zero;

			switch (ffmpeg.GetPlatform()) {
				case PlatformID.MacOSX:
					lib = MacNativeMethods.dlopen("lib"+libraryName, MacNativeMethods.RTLD_NOW);
					break;
				case PlatformID.Win32NT:
				case PlatformID.Win32Windows:
				case PlatformID.Win32S:
					lib = WindowsNativeMethods.LoadLibrary(libraryName);
					break;
				case PlatformID.Unix:
					//TODO: Should I add lib* on Unix too?
					lib = LinuxNativeMethods.dlopen(libraryName, LinuxNativeMethods.RTLD_NOW);
					break;
				default:
					throw new PlatformNotSupportedException();
			}

			if (lib == IntPtr.Zero) {
				throw new Exception("Unable to load library " + libraryName);
			}

			return lib;
#endif
        }
    }
}