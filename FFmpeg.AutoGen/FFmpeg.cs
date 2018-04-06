using System;
using System.Collections.Generic;
using FFmpeg.AutoGen.Native;

namespace FFmpeg.AutoGen
{
    public delegate IntPtr GetOrLoadLibrary(string libraryName, int version);
    public delegate bool CloseLibrary(string libraryName, int version);

    public static partial class ffmpeg
    {
        public const int EAGAIN = 11;

        public const int ENOMEM = 12;

        public const int EINVAL = 22;

        static string abs_path = string.Empty;

		private static FFMpegPlatform CurrentPlatform = FFMpegPlatform.windows;
		private static bool platformSet = false;

		/// <summary>
		/// There is mono bug (macOS is displayed as Unix)
		/// </summary>
		/// <param name="platform">Platform.</param>
		public static void SetPlatform(FFMpegPlatform platform){
			CurrentPlatform = platform;
			platformSet = true;
		}

        public static void SetAbsolutePath(string path){
            abs_path = path;
        }

        public static string GetAbsolutePath(){
            return abs_path;
        }

		public static FFMpegPlatform GetPlatform(){
			if (!platformSet){
				throw new Exception("Use ffmpeg.SetPlatform before run");
			}

			return CurrentPlatform;
		}

        static ffmpeg()
        {
            var loadedLibraries = new Dictionary<string, IntPtr>();

            GetOrLoadLibrary = (name, version) =>
            {
                var key = $"{name}{version}";
                if (loadedLibraries.TryGetValue(key, out var ptr)) 
                    return ptr;
                
                ptr = LibraryLoader.LoadNativeLibraryUsingPlatformNamingConvention(abs_path, name, version);
                loadedLibraries.Add(key, ptr);
                return ptr;
            };

            
            CloseLibrary = (name, version) => {
                var key = $"{name}{version}";
                bool ret = false;
                if (loadedLibraries.TryGetValue(key, out var ptr)){
                    ret = LibraryLoader.UnloadLibrary(ptr);
                    loadedLibraries.Remove(key);
                }

                return ret;
            };
        }

        public static GetOrLoadLibrary GetOrLoadLibrary { get; set; }
        public static CloseLibrary CloseLibrary { get; set; }

        public static T GetFunctionDelegate<T>(IntPtr libraryHandle, string functionName)
            => FunctionLoader.GetFunctionDelegate<T>(libraryHandle, functionName);

        public static ulong UINT64_C<T>(T a)
            => Convert.ToUInt64(a);

        public static int AVERROR<T1>(T1 a)
            => -Convert.ToInt32(a);

        public static int MKTAG<T1, T2, T3, T4>(T1 a, T2 b, T3 c, T4 d)
            => (int) (Convert.ToUInt32(a) | (Convert.ToUInt32(b) << 8) | (Convert.ToUInt32(c) << 16) | (Convert.ToUInt32(d) << 24));

        public static int FFERRTAG<T1, T2, T3, T4>(T1 a, T2 b, T3 c, T4 d)
            => -MKTAG(a, b, c, d);

        public static int AV_VERSION_INT<T1, T2, T3>(T1 a, T2 b, T3 c) =>
            (Convert.ToInt32(a) << 16) | (Convert.ToInt32(b) << 8) | Convert.ToInt32(c);

        public static string AV_VERSION_DOT<T1, T2, T3>(T1 a, T2 b, T3 c)
            => $"{a}.{b}.{c}";

        public static string AV_VERSION<T1, T2, T3>(T1 a, T2 b, T3 c)
            => AV_VERSION_DOT(a, b, c);
    }
}