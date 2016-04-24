using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Zelda.Game.Engine
{
    static class ModFiles
    {
        public enum DataFileLocation
        {
            None,
            Directory,
            Archive,
            WriteDirectory
        };

        public static string ModPath { get; private set; }
        public static string BaseWriteDir { get { return FileSystem.PHYSFS_getUserDir(); } }
        public static string ZeldaWriteDir { get; private set; }
        public static string ModWriteDir { get; private set; }

        public static void Initialize(Arguments args)
        {
            string programName = args.ProgramName;
            if (String.IsNullOrWhiteSpace(programName))
                FileSystem.PHYSFS_init(null);
            else
                FileSystem.PHYSFS_init(args.ProgramName);

            ModPath = Properties.Settings.Default.DefaultMod;

            // 명령행 인자로 모드 경로가 지정되었다면 이를 대신 사용합니다
            var options = args.Args;
            if (options.Any() &&
                !String.IsNullOrWhiteSpace(options.Last()) &&
                options.Last()[0] != '-')
                ModPath = options.Last();

            Console.WriteLine("Opening mod '" + ModPath + "'");

            string archiveModPath1 = ModPath + "/data.zelda";
            string archiveModPath2 = ModPath + "/data.zelda.zip";

            string baseDir = FileSystem.PHYSFS_getBaseDir();
            FileSystem.PHYSFS_addToSearchPath(ModPath, 1);
            FileSystem.PHYSFS_addToSearchPath(archiveModPath1, 1);
            FileSystem.PHYSFS_addToSearchPath(archiveModPath2, 1);
            FileSystem.PHYSFS_addToSearchPath(baseDir + "/" + ModPath, 1);
            FileSystem.PHYSFS_addToSearchPath(baseDir + "/" + archiveModPath1, 1);
            FileSystem.PHYSFS_addToSearchPath(baseDir + "/" + archiveModPath2, 1);

            // 모드가 존재하는지 확인
            if (!DataFileExists("mod.xml"))
            {
                Console.Write("No mod was found in the directory '" + ModPath + "'.\n" +
                              "To specify your mod's path, run: " + (programName ?? "Zelda") + " path/to/mod\n");
                Environment.Exit(0);
            }

            // 엔진 루트 읽기 디렉토리를 설정합니다
            SetZeldaWriteDir(Properties.Settings.Default.WriteDir);
        }

        public static void Quit()
        {
            ModPath = null;
            ZeldaWriteDir = null;
            ModWriteDir = null;

            FileSystem.PHYSFS_deinit();
        }

        #region 모드로부터 데이터 파일 읽기
        // 모드 데이터 디렉토리나 엔진 읽기 디렉토리에 지정한 파일이 존재하는지를 확인합니다
        public static bool DataFileExists(string fileName, bool languageSpecific = false)
        {
            string fullFileName;
            if (languageSpecific)
            {
                if (CurrentMod.Language.IsNullOrEmpty())
                    return false;

                fullFileName = "Languages/" + CurrentMod.Language + "/" + fileName;
            }
            else
            {
                fullFileName = fileName;
            }

            return (FileSystem.PHYSFS_exists(fullFileName) != 0);
        }

        public static byte[] DataFileRead(string fileName, bool languageSpecific = false)
        {
            string fullFileName;
            if (languageSpecific)
            {
                Debug.CheckAssertion(!CurrentMod.Language.IsNullOrEmpty(),
                    "Cannot open language-specific file '{0}': no language was set".F(fileName));
                fullFileName = "Languages/" + CurrentMod.Language + "/" + fileName;
            }
            else
            {
                fullFileName = fileName;
            }

            Debug.CheckAssertion(FileSystem.PHYSFS_exists(fullFileName) != 0,
                "Data file '{0}' does not exist".F(fullFileName));

            IntPtr file = FileSystem.PHYSFS_openRead(fullFileName);
            Debug.CheckAssertion(file != IntPtr.Zero, "Cannot open data file '{0}'".F(fullFileName));

            long size = FileSystem.PHYSFS_fileLength(file);
            byte[] buffer;

            FileSystem.PHYSFS_read(file, out buffer, 1, (uint)size);
            FileSystem.PHYSFS_close(file);
            return buffer;
        }

        public static bool DataFileDelete(string fileName)
        {
            return (FileSystem.PHYSFS_delete(fileName) == 0);
        }
        #endregion

        #region 파일 쓰기
        // 세이브나 설정 파일과 같이 모드에 종속적인 파일들이 저장될 디렉토리를 설정합니다
        public static void SetModWriteDir(string modWriteDir)
        {
            // 이전 모드 디렉토리를 검색 경로에서 제외합니다
            if (!String.IsNullOrWhiteSpace(ModWriteDir))
                FileSystem.PHYSFS_removeFromSearchPath(FileSystem.PHYSFS_getWriteDir());

            ModWriteDir = modWriteDir;

            // 새로운 모드 드렉토리를 생성하기 위해 쓰기 디렉토리를 엔진 디렉토리로 초기화합니다
            string fullWriteDir = BaseWriteDir + "/" + ZeldaWriteDir;
            if (FileSystem.PHYSFS_setWriteDir(fullWriteDir) == 0)
                Debug.Die("Cannot set Zelda write directory to '{0}': {1}".F(fullWriteDir, FileSystem.PHYSFS_getLastError()));

            if (!String.IsNullOrWhiteSpace(ModWriteDir))
            {
                // 모드 디렉토리를 엔진 쓰기 디렉토리 아래에 생성합니다 (존재하지 않는다면)
                FileSystem.PHYSFS_mkdir(ModWriteDir);

                // 쓰기 디렉토리를 갱신
                fullWriteDir = BaseWriteDir + "/" + ZeldaWriteDir + "/" + modWriteDir;
                FileSystem.PHYSFS_setWriteDir(fullWriteDir);

                // 모드가 세이브나 설정 파일과 같은 데이터 파일들을 읽을 수 있게 해줍니다
                FileSystem.PHYSFS_addToSearchPath(FileSystem.PHYSFS_getWriteDir(), 0);
            }
        }

        public static void DataFileSave(string fileName, byte[] buffer, long length)
        {
            IntPtr file = FileSystem.PHYSFS_openWrite(fileName);
            if (file == IntPtr.Zero)
                Debug.Die("Cannot open file '{0}' for writing: {1}".F(fileName, FileSystem.PHYSFS_getLastError()));

            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            if (FileSystem.PHYSFS_write(file, handle.AddrOfPinnedObject(), (uint)length, 1) == -1)
            {
                handle.Free();
                Debug.Die("Cannot write file '{0}': {1}".F(fileName, FileSystem.PHYSFS_getLastError()));
            }
            handle.Free();
            FileSystem.PHYSFS_close(file);
        }
        #endregion

        // 엔진이 파일들을 쓸 수 있는 디렉토리를 설정합니다
        private static void SetZeldaWriteDir(string zeldaWriteDir)
        {
            Debug.CheckAssertion(ZeldaWriteDir == null, "The Zelda write directory already set");

            ZeldaWriteDir = zeldaWriteDir;

            // 디렉토리에 읽기를 수행할 수 있는지 확인합니다
            if (FileSystem.PHYSFS_setWriteDir(BaseWriteDir) == 0)
                Debug.Die("Cannot write in user directory '{0}': ".F(BaseWriteDir, FileSystem.PHYSFS_getLastError()));

            // 디렉토리를 생성합니다
            FileSystem.PHYSFS_mkdir(zeldaWriteDir);

            string fullWriteDir = BaseWriteDir + "/" + zeldaWriteDir;
            if (FileSystem.PHYSFS_setWriteDir(fullWriteDir) == 0)
                Debug.Die("Cannot set Zelda write directory to '{0}': ".F(fullWriteDir, FileSystem.PHYSFS_getLastError()));

            // 모드 디렉토리를 생성합니다
            if (!String.IsNullOrWhiteSpace(ModWriteDir))
                SetModWriteDir(ModWriteDir);
        }
    }
}
