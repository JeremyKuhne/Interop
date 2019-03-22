// Licensed under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Xunit;

namespace InteropTest
{
    public class DynamicLoad
    {
        internal const string NativeLibrary = "NativeLibrary.dll";

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int DoubleDelegate(int value);

        [Fact]
        public void CreateNativeDelegate()
        {
            IntPtr handle = LoadLibraryExW(NativeLibrary, IntPtr.Zero, LoadLibraryFlags.LOAD_WITH_ALTERED_SEARCH_PATH);
            var doubleDelegate = GetFunctionDelegate<DoubleDelegate>(handle, "Double");
            Assert.Equal(50, doubleDelegate(25));
        }

        public static DelegateType GetFunctionDelegate<DelegateType>(IntPtr libraryHandle, string methodName)
        {
            IntPtr method = GetProcAddress(libraryHandle, methodName);
            if (method == IntPtr.Zero)
                throw new Win32Exception();

            return Marshal.GetDelegateForFunctionPointer<DelegateType>(method);
        }

        // This API is only available in ANSI (which is the default CharSet)
        [DllImport(Libraries.Kernel32, SetLastError = true, ExactSpelling = true, BestFitMapping = false)]
        public static extern IntPtr GetProcAddress(
            IntPtr hModule,
            string methodName);

        [DllImport(Libraries.Kernel32, CharSet = CharSet.Unicode, SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr LoadLibraryExW(
            string lpFileName,
            IntPtr hFile,
            LoadLibraryFlags dwFlags);

        [DllImport(Libraries.Kernel32, SetLastError = true)]
        public static extern bool FreeLibrary(
            IntPtr hModule);

        [Flags]
        public enum LoadLibraryFlags : uint
        {
            DONT_RESOLVE_DLL_REFERENCES = 0x00000001,
            LOAD_LIBRARY_AS_DATAFILE = 0x00000002,
            LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008,
            LOAD_IGNORE_CODE_AUTHZ_LEVEL = 0x00000010,
            LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x00000020,
            LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE = 0x00000040,
            LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR = 0x00000100,
            LOAD_LIBRARY_SEARCH_APPLICATION_DIR = 0x00000200,
            LOAD_LIBRARY_SEARCH_USER_DIRS = 0x00000400,
            LOAD_LIBRARY_SEARCH_SYSTEM32 = 0x00000800,
            LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000
        }
    }
}
