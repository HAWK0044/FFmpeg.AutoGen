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
			IntPtr libPtr = IntPtr.Zero;
			switch(ffmpeg.GetPlatform()){
				case FFMpegPlatform.macOS:
					fullName = Path.Combine(path, $"{libraryName}.{version}.dylib");
					libPtr = LoadNativeLibrary(fullName);

					if (libPtr != IntPtr.Zero)
						return libPtr;
                    
                    fullName = Path.Combine(path, $"{libraryName}.{version}.bundle");
                    libPtr = LoadNativeLibrary(fullName);

                    if (libPtr != IntPtr.Zero)
                        return libPtr;

                    fullName = Path.Combine(path, $"lib{libraryName}.{version}.bundle");
                    libPtr = LoadNativeLibrary(fullName);

                    if (libPtr != IntPtr.Zero)
                        return libPtr;
                    
					fullName = Path.Combine(path, $"lib{libraryName}.{version}.dylib");
					return LoadNativeLibrary(fullName);
				case FFMpegPlatform.windows:
					fullName = Path.Combine(path, $"{libraryName}-{version}.dll");
					libPtr = LoadNativeLibrary(fullName);

                    if(libPtr != IntPtr.Zero){
                        return libPtr;
                    }

                    fullName = Path.Combine(path, $"{libraryName}-{version}");
                    libPtr = LoadNativeLibrary(fullName);

                    if (libPtr != IntPtr.Zero) {
                        return libPtr;
                    }
                    break;
				case FFMpegPlatform.unix:
					fullName = Path.Combine(path, $"{libraryName}.so{version}");
					return LoadNativeLibrary(fullName);
				case FFMpegPlatform.iOS:
					fullName = Path.Combine(path, $"{libraryName}.{version}.dylib");

					libPtr = LoadNativeLibrary(fullName);

					if (libPtr != IntPtr.Zero)
						return libPtr;

					fullName = Path.Combine(path, $"{libraryName}.{version}.bundle");
					return LoadNativeLibrary(fullName);
			}

            if(libPtr == IntPtr.Zero){
                if(ffmpeg.GetPlatform() == FFMpegPlatform.windows){
                    throw new Exception("Unable to load " + fullName + " load error " + Marshal.GetLastWin32Error());
                }

                throw new Exception("Unable to load " + fullName);
            }
            throw new PlatformNotSupportedException();
#endif
        }

        public static bool UnloadLibrary(IntPtr handle) {
            if (handle == IntPtr.Zero)
                return false;

            switch (ffmpeg.GetPlatform()) {
                case FFMpegPlatform.macOS:
                    //lib = MacNativeMethods.dlopen("lib" + libraryName, MacNativeMethods.RTLD_NOW);
                    return MacNativeMethods.dlclose(handle) == 0;
                case FFMpegPlatform.windows:
                    return WindowsNativeMethods.FreeLibrary(handle);
                case FFMpegPlatform.unix:
                    return LinuxNativeMethods.dlclose(handle) == 0;
                //case FFMpegPlatform.iOS:
                    //return false;
                    //lib = iOSNativeMethods.dlopen("lib" + libraryName, MacNativeMethods.RTLD_NOW);
                    //break;
                default:
                    throw new PlatformNotSupportedException();
            }

            throw new PlatformNotSupportedException();
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
				case FFMpegPlatform.macOS:
                    lib = MacNativeMethods.dlopen("libff" + libraryName, MacNativeMethods.RTLD_NOW);
					break;
				case FFMpegPlatform.windows:
					lib = WindowsNativeMethods.LoadLibrary(libraryName);
					break;
				case FFMpegPlatform.unix:
					//TODO: Should I add lib* on Unix too?
					lib = LinuxNativeMethods.dlopen(libraryName, LinuxNativeMethods.RTLD_NOW);
					break;
				case FFMpegPlatform.iOS:
					lib = iOSNativeMethods.dlopen("lib" + libraryName, MacNativeMethods.RTLD_NOW);
					break;
				default:
					throw new PlatformNotSupportedException();
			}

            /*
			if (lib == IntPtr.Zero) {
                if(ffmpeg.GetPlatform() == FFMpegPlatform.windows){
                    var winErr = Marshal.GetLastWin32Error();
                    throw new Exception("Unable to load library " + libraryName + " error code " + winErr);
                }
				throw new Exception("Unable to load library " + libraryName);
			}
            */

			return lib;
#endif
        }
    }
}