using System;
using System.Runtime.InteropServices;

namespace Zelda.Game.LowLevel
{
    public class ModFiles : IDisposable
    {
        public enum DataFileLocation
        {
            None,
            Directory,
            Archive,
            WriteDirectory
        };

        public string ModPath { get; }
        public string BaseWriteDir { get { return FileSystem.PHYSFS_getUserDir(); } }
        public string ZeldaWriteDir { get; private set; }
        public string ModWriteDir { get; private set; }

        readonly CurrentMod _mod;

        public ModFiles(CurrentMod mod, string programName, string modPath)
        {
            _mod = mod;

            FileSystem.PHYSFS_init(programName);

            ModPath = modPath;
            var archiveModPath1 = ModPath + "/data.zelda";
            var archiveModPath2 = ModPath + "/data.zelda.zip";

            var baseDir = FileSystem.PHYSFS_getBaseDir();
            FileSystem.PHYSFS_addToSearchPath(ModPath, 1);
            FileSystem.PHYSFS_addToSearchPath(archiveModPath1, 1);
            FileSystem.PHYSFS_addToSearchPath(archiveModPath2, 1);
            FileSystem.PHYSFS_addToSearchPath(baseDir + "/" + ModPath, 1);
            FileSystem.PHYSFS_addToSearchPath(baseDir + "/" + archiveModPath1, 1);
            FileSystem.PHYSFS_addToSearchPath(baseDir + "/" + archiveModPath2, 1);

            SetZeldaWriteDir(Properties.Settings.Default.WriteDir);

            if (!DataFileExists("mod.xml"))
                throw new Exception("No mod was found in the directory '{0}'".F(ModPath));
        }

        public void Dispose()
        {
            FileSystem.PHYSFS_deinit();
        }

        public bool DataFileExists(string fileName, bool languageSpecific = false)
        {
            string fullFileName;
            if (languageSpecific)
            {
                if (_mod.Language == null)
                    return false;

                fullFileName = "Languages/" + _mod.Language + "/" + fileName;
            }
            else
            {
                fullFileName = fileName;
            }

            return (FileSystem.PHYSFS_exists(fullFileName) != 0);
        }

        public byte[] DataFileRead(string fileName, bool languageSpecific = false)
        {
            var fullFileName = fileName;

            if (languageSpecific)
            {
                if (_mod.Language == null)
                    throw new InvalidOperationException("Cannot open language-specific file '{0}': no language was set".F(fileName));

                fullFileName = "Languages/" + _mod.Language + "/" + fullFileName;
            }

            if (FileSystem.PHYSFS_exists(fullFileName) == 0)
                throw new Exception("Data file '{0}' does not exist".F(fullFileName));

            var file = FileSystem.PHYSFS_openRead(fullFileName);
            if (file == IntPtr.Zero)
                throw new Exception("Cannot open data file '{0}'".F(fullFileName));

            var size = FileSystem.PHYSFS_fileLength(file);
            byte[] buffer;

            try
            {
                FileSystem.PHYSFS_read(file, out buffer, 1, (uint)size);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                FileSystem.PHYSFS_close(file);
            }
            return buffer;
        }

        public bool DataFileDelete(string fileName)
        {
            return (FileSystem.PHYSFS_delete(fileName) == 0);
        }

        // 세이브나 설정 파일과 같이 모드에 종속적인 파일들이 저장될 디렉토리를 설정합니다
        public void SetModWriteDir(string modWriteDir)
        {
            // 이전 모드 디렉토리를 검색 경로에서 제외합니다
            if (ModWriteDir != null)
                FileSystem.PHYSFS_removeFromSearchPath(FileSystem.PHYSFS_getWriteDir());

            ModWriteDir = modWriteDir;

            // 새로운 모드 드렉토리를 생성하기 위해 쓰기 디렉토리를 엔진 디렉토리로 초기화합니다
            var fullWriteDir = BaseWriteDir + "/" + ZeldaWriteDir;
            if (FileSystem.PHYSFS_setWriteDir(fullWriteDir) == 0)
                throw new Exception("Cannot set Zelda write directory to '{0}': {1}".F(fullWriteDir, FileSystem.PHYSFS_getLastError()));

            if (ModWriteDir == null)
                return;

            FileSystem.PHYSFS_mkdir(ModWriteDir);
            fullWriteDir = BaseWriteDir + "/" + ZeldaWriteDir + "/" + modWriteDir;
            FileSystem.PHYSFS_setWriteDir(fullWriteDir);
            FileSystem.PHYSFS_addToSearchPath(FileSystem.PHYSFS_getWriteDir(), 0);
        }

        public void DataFileSave(string fileName, byte[] buffer, long length)
        {
            var file = FileSystem.PHYSFS_openWrite(fileName);
            if (file == IntPtr.Zero)
                throw new Exception("Cannot open file '{0}' for writing: {1}".F(fileName, FileSystem.PHYSFS_getLastError()));

            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                if (FileSystem.PHYSFS_write(file, handle.AddrOfPinnedObject(), (uint)length, 1) == -1)
                    throw new Exception("Cannot write file '{0}': {1}".F(fileName, FileSystem.PHYSFS_getLastError()));
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                handle.Free();
                FileSystem.PHYSFS_close(file);
            }
        }

        // 엔진이 파일들을 쓸 수 있는 디렉토리를 설정합니다
        void SetZeldaWriteDir(string zeldaWriteDir)
        {
            if (ZeldaWriteDir != null)
                throw new InvalidOperationException("The Zelda write directory already set");

            ZeldaWriteDir = zeldaWriteDir;

            if (FileSystem.PHYSFS_setWriteDir(BaseWriteDir) == 0)
                throw new Exception("Cannot write in user directory '{0}': ".F(BaseWriteDir, FileSystem.PHYSFS_getLastError()));

            FileSystem.PHYSFS_mkdir(zeldaWriteDir);

            var fullWriteDir = BaseWriteDir + "/" + zeldaWriteDir;
            if (FileSystem.PHYSFS_setWriteDir(fullWriteDir) == 0)
                throw new Exception("Cannot set Zelda write directory to '{0}': ".F(fullWriteDir, FileSystem.PHYSFS_getLastError()));

            // 모드 디렉토리를 생성합니다
            if (ModWriteDir != null)
                SetModWriteDir(ModWriteDir);
        }
    }
}
