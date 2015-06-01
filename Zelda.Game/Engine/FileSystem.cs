using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Collections;

namespace Zelda.Game.Engine
{
    #region Aliases
    using PHYSFS_uint8 = System.Byte;
    using PHYSFS_sint8 = System.SByte;
    using PHYSFS_uint16 = System.UInt16;
    using PHYSFS_sint16 = System.Int16;
    using PHYSFS_uint32 = System.UInt32;
    using PHYSFS_sint32 = System.Int32;
    using PHYSFS_uint64 = System.UInt64;
    using PHYSFS_sint64 = System.Int64;
    #endregion Aliases

    #region Class Documentation
    #endregion Class Documentation
    [SuppressUnmanagedCodeSecurityAttribute()]
    public static class FileSystem
    {
        private const string PHYSFS_NATIVE_LIBRARY = "physfs.dll";
        private const CallingConvention CALLING_CONVENTION = CallingConvention.Cdecl;

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct PHYSFS_Version
        {
            public PHYSFS_uint8 major;
            public PHYSFS_uint8 minor;
            public PHYSFS_uint8 patch;
            public override string ToString()
            {
                return major.ToString() + "." + minor.ToString() + "." + patch.ToString();
            }
        }

        public class PHYSFS_ArchiveInfo
        {
            public PHYSFS_ArchiveInfo(string extension, string description, string author, string url)
            {
                this.extension = extension;
                this.description = description;
                this.author = author;
                this.url = url;
            }
            public string extension;
            public string description;
            public string author;
            public string url;
            public override string ToString()
            {
                return "{0} - {1} ({2} - {3})".F(extension, description, author, url);
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct PHYSFS_ArchiveInfoInternal
        {
            public IntPtr extension;
            public IntPtr description;
            public IntPtr author;
            public IntPtr url;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct PHYSFS_File
        {
            /// <summary>
            /// That's all you get. Don't touch. 
            /// </summary>
            public IntPtr opaque;
        }

        public const int PHYSFS_VER_MAJOR = 1;
        public const int PHYSFS_VER_MINOR = 0;
        public const int PHYSFS_VER_PATCH = 1;

        private static string DecodeUTF8(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                return null;

            int size = 0;
            for (; ; ++size)
            {
                byte b = Marshal.ReadByte(ptr, size);
                if (b == 0x0)
                    break;
            }
            if (size == 0)
                return String.Empty;

            byte[] bytes = new byte[size];
            Marshal.Copy(ptr, bytes, 0, size);
            return Encoding.UTF8.GetString(bytes);
        }

        private static IntPtr EncodeUTF8(string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            IntPtr ptr = Marshal.AllocHGlobal(bytes.Length + 1);
            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            Marshal.WriteByte(ptr, bytes.Length, 0);
            return ptr;
        }

        [CLSCompliant(false)] // Not CLS Compliant because it's named the same as PHYSFS_Version
        public static void PHYSFS_VERSION(out PHYSFS_Version ver)
        {
            ver.major = PHYSFS_VER_MAJOR;
            ver.minor = PHYSFS_VER_MINOR;
            ver.patch = PHYSFS_VER_PATCH;
        }

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION, EntryPoint = "PHYSFS_addToSearchPath"), SuppressUnmanagedCodeSecurity]
        private extern static int PHYSFS_addToSearchPathInternal(IntPtr newDir, int appendToPath);

        public static int PHYSFS_addToSearchPath(string newDir, int appendToPath)
        {
            IntPtr ptr = EncodeUTF8(newDir);
            int ret = PHYSFS_addToSearchPathInternal(ptr, appendToPath);
            Marshal.FreeHGlobal(ptr);
            return ret;
        }

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_close(IntPtr handle);

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_deinit();

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_delete(string filename);

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION, EntryPoint = "PHYSFS_enumerateFiles"), SuppressUnmanagedCodeSecurity]
        private extern static IntPtr PHYSFS_enumerateFilesInternal(string dir);

        public static string[] PHYSFS_enumerateFiles(string dir)
        {
            ArrayList strings = new ArrayList(); // Use System.Collections.Generic.List if you're on .NET 2.0
            IntPtr p = PHYSFS_enumerateFilesInternal(dir);
            IntPtr original = p;
            unsafe
            {
                int* ptr = (int*)p.ToPointer();
                while (*ptr != 0)
                {
                    string s = Marshal.PtrToStringAnsi(new IntPtr(*ptr));
                    strings.Add(s);
                    p = new IntPtr(ptr + 1);
                    ptr++;
                }
            }
            PHYSFS_freeList(original);
            return (string[])strings.ToArray(typeof(string));
        }

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_eof(IntPtr handle);

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_exists(string fname);

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static PHYSFS_sint64 PHYSFS_fileLength(IntPtr handle);

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_flush(IntPtr handle);

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static void PHYSFS_freeList(IntPtr listVar);

        public static string PHYSFS_getBaseDir()
        {
            return DecodeUTF8(PHYSFS_getBaseDirInternal());
        }

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION, EntryPoint = "PHYSFS_getBaseDir"), SuppressUnmanagedCodeSecurity]
        private extern static IntPtr PHYSFS_getBaseDirInternal();

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION, EntryPoint = "PHYSFS_getCdRomDirs"), SuppressUnmanagedCodeSecurity]
        private extern static IntPtr PHYSFS_getCdRomDirsInternal();

        public static string[] PHYSFS_getCdRomDirs()
        {
            ArrayList strings = new ArrayList(); // Use System.Collections.Generic.List if you're on .NET 2.0
            IntPtr p = PHYSFS_getCdRomDirsInternal();
            IntPtr original = p;
            unsafe
            {
                int* ptr = (int*)p.ToPointer();
                while (*ptr != 0)
                {
                    string s = Marshal.PtrToStringAnsi(new IntPtr(*ptr));
                    strings.Add(s);
                    p = new IntPtr(ptr + 1);
                    ptr++;
                }
            }
            PHYSFS_freeList(original);
            return (string[])strings.ToArray(typeof(string));
        }

        public static string PHYSFS_getDirSeparator()
        {
            return DecodeUTF8(PHYSFS_getDirSeparatorInternal());
        }

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION, EntryPoint = "PHYSFS_getDirSeparator"), SuppressUnmanagedCodeSecurity]
        public extern static IntPtr PHYSFS_getDirSeparatorInternal();

        public static string PHYSFS_getLastError()
        {
            return DecodeUTF8(PHYSFS_getLastErrorInternal());
        }

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION, EntryPoint = "PHYSFS_getLastError"), SuppressUnmanagedCodeSecurity]
        private extern static IntPtr PHYSFS_getLastErrorInternal();

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static PHYSFS_sint64 PHYSFS_getLastModTime(string filename);

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static void PHYSFS_getLinkedVersion(out PHYSFS_Version ver);

        public static string PHYSFS_getRealDir(string filename)
        {
            return DecodeUTF8(PHYSFS_getRealDirInternal(filename));
        }

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION, EntryPoint = "PHYSFS_getRealDir"), SuppressUnmanagedCodeSecurity]
        private extern static IntPtr PHYSFS_getRealDirInternal(string filename);

        public static string[] PHYSFS_getSearchPath()
        {
            ArrayList strings = new ArrayList(); // Use System.Collections.Generic.List if you're on .NET 2.0
            IntPtr p = PHYSFS_getSearchPathInternal();
            IntPtr original = p;
            unsafe
            {
                int* ptr = (int*)p.ToPointer();
                while (*ptr != 0)
                {
                    string s = Marshal.PtrToStringAnsi(new IntPtr(*ptr));
                    strings.Add(s);
                    p = new IntPtr(ptr + 1);
                    ptr++;
                }
            }
            PHYSFS_freeList(original);
            return (string[])strings.ToArray(typeof(string));
        }

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION, EntryPoint = "PHYSFS_getSearchPath"), SuppressUnmanagedCodeSecurity]
        private extern static IntPtr PHYSFS_getSearchPathInternal();

        public static string PHYSFS_getUserDir()
        {
            return DecodeUTF8(PHYSFS_getUserDirInternal());
        }

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION, EntryPoint = "PHYSFS_getUserDir"), SuppressUnmanagedCodeSecurity]
        private extern static IntPtr PHYSFS_getUserDirInternal();

        public static string PHYSFS_getWriteDir()
        {
            return DecodeUTF8(PHYSFS_getWriteDirInternal());
        }

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION, EntryPoint = "PHYSFS_getWriteDir"), SuppressUnmanagedCodeSecurity]
        private extern static IntPtr PHYSFS_getWriteDirInternal();

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_init(string argv0);

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_isDirectory(string filename);

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_isSymbolicLink(string filename);

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_mkdir(string dirName);

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static IntPtr PHYSFS_openAppend(string filename);

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static IntPtr PHYSFS_openRead(string filename);

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static IntPtr PHYSFS_openWrite(string filename);

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static void PHYSFS_permitSymbolicLinks(int allow);

        [CLSCompliant(false)]
        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION, EntryPoint = "PHYSFS_read"), SuppressUnmanagedCodeSecurity]
        public extern static PHYSFS_sint64 PHYSFS_read(IntPtr handle, IntPtr buffer, PHYSFS_uint32 objSize, PHYSFS_uint32 objCount);

        [CLSCompliant(false)]
        public static PHYSFS_sint64 PHYSFS_read(IntPtr handle, out IntPtr buffer, PHYSFS_uint32 objSize, PHYSFS_uint32 objCount)
        {
            buffer = Marshal.AllocHGlobal((int)objSize * (int)objCount);
            return PHYSFS_read(handle, buffer, objSize, objCount);
        }

        [CLSCompliant(false)]
        public static PHYSFS_sint64 PHYSFS_read(IntPtr handle, out byte[] buffer, PHYSFS_uint32 objSize, PHYSFS_uint32 objCount)
        {
            IntPtr ptrBuffer;
            PHYSFS_sint64 ret = PHYSFS_read(handle, out ptrBuffer, objSize, objCount);
            buffer = new byte[objCount];
            Marshal.Copy(ptrBuffer, buffer, 0, (int)objCount);
            Marshal.FreeHGlobal(ptrBuffer);
            return ret;
        }

        [CLSCompliant(false)]
        public static PHYSFS_sint64 PHYSFS_read(IntPtr handle, out char[] buffer, PHYSFS_uint32 objSize, PHYSFS_uint32 objCount)
        {
            IntPtr ptrBuffer;
            PHYSFS_sint64 ret = PHYSFS_read(handle, out ptrBuffer, objSize, objCount);
            buffer = new char[objCount];
            Marshal.Copy(ptrBuffer, buffer, 0, (int)objCount);
            Marshal.FreeHGlobal(ptrBuffer);
            return ret;
        }

        [CLSCompliant(false)]
        public static PHYSFS_sint64 PHYSFS_read(IntPtr handle, out double[] buffer, PHYSFS_uint32 objSize, PHYSFS_uint32 objCount)
        {
            IntPtr ptrBuffer;
            PHYSFS_sint64 ret = PHYSFS_read(handle, out ptrBuffer, objSize, objCount);
            buffer = new double[objCount];
            Marshal.Copy(ptrBuffer, buffer, 0, (int)objCount);
            Marshal.FreeHGlobal(ptrBuffer);
            return ret;
        }

        [CLSCompliant(false)]
        public static PHYSFS_sint64 PHYSFS_read(IntPtr handle, out float[] buffer, PHYSFS_uint32 objSize, PHYSFS_uint32 objCount)
        {
            IntPtr ptrBuffer;
            PHYSFS_sint64 ret = PHYSFS_read(handle, out ptrBuffer, objSize, objCount);
            buffer = new float[objCount];
            Marshal.Copy(ptrBuffer, buffer, 0, (int)objCount);
            Marshal.FreeHGlobal(ptrBuffer);
            return ret;
        }

        [CLSCompliant(false)]
        public static PHYSFS_sint64 PHYSFS_read(IntPtr handle, out int[] buffer, PHYSFS_uint32 objSize, PHYSFS_uint32 objCount)
        {
            IntPtr ptrBuffer;
            PHYSFS_sint64 ret = PHYSFS_read(handle, out ptrBuffer, objSize, objCount);
            buffer = new int[objCount];
            Marshal.Copy(ptrBuffer, buffer, 0, (int)objCount);
            Marshal.FreeHGlobal(ptrBuffer);
            return ret;
        }

        [CLSCompliant(false)]
        public static PHYSFS_sint64 PHYSFS_read(IntPtr handle, out long[] buffer, PHYSFS_uint32 objSize, PHYSFS_uint32 objCount)
        {
            IntPtr ptrBuffer;
            PHYSFS_sint64 ret = PHYSFS_read(handle, out ptrBuffer, objSize, objCount);
            buffer = new long[objCount];
            Marshal.Copy(ptrBuffer, buffer, 0, (int)objCount);
            Marshal.FreeHGlobal(ptrBuffer);
            return ret;
        }

        [CLSCompliant(false)]
        public static PHYSFS_sint64 PHYSFS_read(IntPtr handle, out short[] buffer, PHYSFS_uint32 objSize, PHYSFS_uint32 objCount)
        {
            IntPtr ptrBuffer;
            PHYSFS_sint64 ret = PHYSFS_read(handle, out ptrBuffer, objSize, objCount);
            buffer = new short[objCount];
            Marshal.Copy(ptrBuffer, buffer, 0, (int)objCount);
            Marshal.FreeHGlobal(ptrBuffer);
            return ret;
        }

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_readSBE16(IntPtr handle, ref PHYSFS_sint16 val);

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_readSBE32(IntPtr handle, ref PHYSFS_sint32 val);

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_readSBE64(IntPtr handle, ref PHYSFS_sint64 val);

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_readSLE16(IntPtr handle, ref PHYSFS_sint16 val);

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_readSLE32(IntPtr handle, ref PHYSFS_sint32 val);

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_readSLE64(IntPtr handle, ref PHYSFS_sint64 val);

        [CLSCompliant(false)]
        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_readUBE16(IntPtr handle, ref PHYSFS_uint16 val);

        [CLSCompliant(false)]
        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_readUBE32(IntPtr handle, ref PHYSFS_uint32 val);

        [CLSCompliant(false)]
        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_readUBE64(IntPtr handle, ref PHYSFS_uint64 val);

        [CLSCompliant(false)]
        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_readULE16(IntPtr handle, ref PHYSFS_uint16 val);

        [CLSCompliant(false)]
        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_readULE32(IntPtr handle, ref PHYSFS_uint32 val);

        [CLSCompliant(false)]
        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_readULE64(IntPtr handle, ref PHYSFS_uint32 val);

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_removeFromSearchPath(string oldDir);

        [CLSCompliant(false)]
        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_seek(IntPtr handle, PHYSFS_uint64 pos);

        [CLSCompliant(false)]
        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_setBuffer(IntPtr handle, PHYSFS_uint64 bufsize);

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_setSaneConfig(string organization, string appName, string archiveExt, int includeCdRoms, int archivesFirst);

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION, EntryPoint = "PHYSFS_setWriteDir"), SuppressUnmanagedCodeSecurity]
        private extern static int PHYSFS_setWriteDirInternal(IntPtr newDir);

        public static int PHYSFS_setWriteDir(string newDir)
        {
            IntPtr ptr = EncodeUTF8(newDir);
            int ret = PHYSFS_setWriteDirInternal(ptr);
            Marshal.FreeHGlobal(ptr);
            return ret;
        }

        public static PHYSFS_ArchiveInfo[] PHYSFS_supportedArchiveTypes()
        {
            ArrayList archives = new ArrayList(); // Use System.Collections.Generic.List if you're on .NET 2.0
            IntPtr p = PHYSFS_supportedArchiveTypesInternal();
            unsafe
            {
                int* ptr = (int*)p.ToPointer();
                while (*ptr != 0)
                {
                    PHYSFS_ArchiveInfoInternal info = (PHYSFS_ArchiveInfoInternal)Marshal.PtrToStructure(new IntPtr(*ptr), typeof(PHYSFS_ArchiveInfoInternal));
                    PHYSFS_ArchiveInfo archiveInfo = new PHYSFS_ArchiveInfo(
                        Marshal.PtrToStringAnsi(info.extension),
                        Marshal.PtrToStringAnsi(info.description),
                        Marshal.PtrToStringAnsi(info.author),
                        Marshal.PtrToStringAnsi(info.url));
                    archives.Add(archiveInfo);
                    p = new IntPtr(ptr + 1);
                    ptr++;
                }
            }
            return (PHYSFS_ArchiveInfo[])archives.ToArray(typeof(PHYSFS_ArchiveInfo));
        }

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION, EntryPoint = "PHYSFS_supportedArchiveTypes"), SuppressUnmanagedCodeSecurity]
        private static extern IntPtr PHYSFS_supportedArchiveTypesInternal();

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static PHYSFS_sint16 PHYSFS_swapSBE16(PHYSFS_sint16 val);

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static PHYSFS_sint32 PHYSFS_swapSBE32(PHYSFS_sint32 val);

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static PHYSFS_sint64 PHYSFS_swapSBE64(PHYSFS_sint64 val);

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static PHYSFS_sint16 PHYSFS_swapSLE16(PHYSFS_sint16 val);

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static PHYSFS_sint32 PHYSFS_swapSLE32(PHYSFS_sint32 val);

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static PHYSFS_sint64 PHYSFS_swapSLE64(PHYSFS_sint64 val);

        [CLSCompliant(false)]
        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static PHYSFS_uint16 PHYSFS_swapUBE16(PHYSFS_uint16 val);

        [CLSCompliant(false)]
        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static PHYSFS_uint32 PHYSFS_swapUBE32(PHYSFS_uint32 val);

        [CLSCompliant(false)]
        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static PHYSFS_uint64 PHYSFS_swapUBE64(PHYSFS_uint64 val);

        [CLSCompliant(false)]
        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static PHYSFS_uint16 PHYSFS_swapULE16(PHYSFS_uint16 val);

        [CLSCompliant(false)]
        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static PHYSFS_uint32 PHYSFS_swapULE32(PHYSFS_uint32 val);

        [CLSCompliant(false)]
        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static PHYSFS_uint64 PHYSFS_swapULE64(PHYSFS_uint64 val);

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static PHYSFS_sint64 PHYSFS_tell(IntPtr handle);

        [CLSCompliant(false)]
        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static PHYSFS_sint64 PHYSFS_write(IntPtr handle, IntPtr buffer, PHYSFS_uint32 objSize, PHYSFS_uint32 objCount);

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_writeSBE16(IntPtr file, PHYSFS_sint16 val);

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_writeSBE32(IntPtr file, PHYSFS_sint32 val);

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_writeSBE64(IntPtr file, PHYSFS_sint64 val);

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_writeSLE16(IntPtr file, PHYSFS_sint16 val);

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_writeSLE32(IntPtr file, PHYSFS_sint32 val);

        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_writeSLE64(IntPtr file, PHYSFS_sint64 val);

        [CLSCompliant(false)]
        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_writeUBE16(IntPtr file, PHYSFS_uint16 val);

        [CLSCompliant(false)]
        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_writeUBE32(IntPtr file, PHYSFS_uint32 val);

        [CLSCompliant(false)]
        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_writeUBE64(IntPtr file, PHYSFS_uint64 val);

        [CLSCompliant(false)]
        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_writeULE16(IntPtr file, PHYSFS_uint16 val);

        [CLSCompliant(false)]
        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_writeULE32(IntPtr file, PHYSFS_uint32 val);

        [CLSCompliant(false)]
        [DllImport(PHYSFS_NATIVE_LIBRARY, CallingConvention = CALLING_CONVENTION), SuppressUnmanagedCodeSecurity]
        public extern static int PHYSFS_writeULE64(IntPtr file, PHYSFS_uint64 val);
    }
}
