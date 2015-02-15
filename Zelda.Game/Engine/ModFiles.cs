using System;
using System.IO;
using System.Linq;

namespace Zelda.Game.Engine
{
    public class ModFiles
    {
        public enum DataFileLocation
        {
            None,
            Directory,
            Archive,
            WriteDirectory
        };

        private string _modPath;
        public string ModPath
        {
            get { return _modPath; }
        }

        public string BaseWriteDir
        {
            get { return FileSystem.PHYSFS_getUserDir(); }
        }

        private string _zeldaWriteDir;
        public string ZeldaWriteDir
        {
            get { return _zeldaWriteDir; }
        }

        private string _modWriteDir;
        public string ModWriteDir
        {
            get { return _modWriteDir; }
        }

        public void Initialize(Arguments args)
        {
            string programName = args.ProgramName;
            if (String.IsNullOrWhiteSpace(programName))
                FileSystem.PHYSFS_init(null);
            else
                FileSystem.PHYSFS_init(args.ProgramName);

            _modPath = Properties.Settings.Default.DefaultMod;

            // 명령행 인자로 모드 경로가 지정되었다면 이를 대신 사용합니다
            var options = args.Args;
            if (options.Any() &&
                !String.IsNullOrWhiteSpace(options.Last()) &&
                options.Last()[0] != '-')
                _modPath = options.Last();

            Console.WriteLine("Opening mod '" + _modPath + "'");

            string archiveModPath1 = _modPath + "/Data.zelda";
            string archiveModPath2 = _modPath + "/Data.zelda.zip";

            string baseDir = FileSystem.PHYSFS_getBaseDir();
            FileSystem.PHYSFS_addToSearchPath(_modPath, 1);
            FileSystem.PHYSFS_addToSearchPath(archiveModPath1, 1);
            FileSystem.PHYSFS_addToSearchPath(archiveModPath2, 1);
            FileSystem.PHYSFS_addToSearchPath(baseDir + "/" + _modPath, 1);
            FileSystem.PHYSFS_addToSearchPath(baseDir + "/" + archiveModPath1, 1);
            FileSystem.PHYSFS_addToSearchPath(baseDir + "/" + archiveModPath2, 1);

            // 모드가 존재하는지 확인
            if (!DataFileExists("Mod.xml"))
            {
                Console.Write("No mod was found in the directory '" + _modPath + "'.\n" + 
                              "To specify your mod's path, run: " + (programName ?? "Zelda") + " path/to/mod\n");
                Environment.Exit(0);
            }

            // 엔진 루트 읽기 디렉토리를 설정합니다
            SetZeldaWriteDir(Properties.Settings.Default.WriteDir);
        }

        public void Quit()
        {
            _modPath = null;
            _zeldaWriteDir = null;
            _modWriteDir = null;

            FileSystem.PHYSFS_deinit();
        }

        #region 모드로부터 데이터 파일 읽기
        // 모드 데이터 디렉토리나 엔진 읽기 디렉토리에 지정한 파일이 존재하는지를 확인합니다
        public bool DataFileExists(string fileName)
        {
            return (FileSystem.PHYSFS_exists(fileName) != 0);
        }

        public MemoryStream DataFileRead(string fileName)
        {
            if (FileSystem.PHYSFS_exists(fileName) == 0)
                throw new InvalidDataException("Data file '" + fileName + "' does not exist");

            IntPtr file = FileSystem.PHYSFS_openRead(fileName);
            if (file == IntPtr.Zero)
                throw new InvalidDataException("Cannot open data file '" + fileName + "'");

            long size = FileSystem.PHYSFS_fileLength(file);
            byte[] buffer;

            FileSystem.PHYSFS_read(file, out buffer, 1, (uint)size);
            FileSystem.PHYSFS_close(file);

            return new MemoryStream(buffer);
        }
        #endregion

        #region 파일 쓰기
        // 세이브나 설정 파일과 같이 모드에 종속적인 파일들이 저장될 디렉토리를 설정합니다
        public void SetModWriteDir(string modWriteDir)
        {
            // 이전 모드 디렉토리를 검색 경로에서 제외합니다
            if (!String.IsNullOrWhiteSpace(_modWriteDir))
                FileSystem.PHYSFS_removeFromSearchPath(FileSystem.PHYSFS_getWriteDir());

            _modWriteDir = modWriteDir;

            // 새로운 모드 드렉토리를 생성하기 위해 쓰기 디렉토리를 엔진 디렉토리로 초기화합니다
            string fullWriteDir = BaseWriteDir + "/" + ZeldaWriteDir;
            if (FileSystem.PHYSFS_setWriteDir(fullWriteDir) == 0)
                throw new Exception("Cannot set Zelda write directory to '" + fullWriteDir + "': " + FileSystem.PHYSFS_getLastError());

            if (!String.IsNullOrWhiteSpace(_modWriteDir))
            {
                // 모드 디렉토리를 엔진 쓰기 디렉토리 아래에 생성합니다 (존재하지 않는다면)
                FileSystem.PHYSFS_mkdir(_modWriteDir);

                // 쓰기 디렉토리를 갱신
                fullWriteDir = BaseWriteDir + "/" + ZeldaWriteDir + "/" + modWriteDir;
                FileSystem.PHYSFS_setWriteDir(fullWriteDir);

                // 모드가 세이브나 설정 파일과 같은 데이터 파일들을 읽을 수 있게 해줍니다
                FileSystem.PHYSFS_addToSearchPath(FileSystem.PHYSFS_getWriteDir(), 0);
            }
        }
        #endregion

        // 엔진이 파일들을 쓸 수 있는 디렉토리를 설정합니다
        private void SetZeldaWriteDir(string zeldaWriteDir)
        {
            if (_zeldaWriteDir != null)
                throw new InvalidOperationException("The Zelda write directory already set");

            _zeldaWriteDir = zeldaWriteDir;

            // 디렉토리에 읽기를 수행할 수 있는지 확인합니다
            if (FileSystem.PHYSFS_setWriteDir(BaseWriteDir) == 0)
                throw new Exception("Cannot write in user directory '" + BaseWriteDir + "': " + FileSystem.PHYSFS_getLastError());

            // 디렉토리를 생성합니다
            FileSystem.PHYSFS_mkdir(zeldaWriteDir);

            string fullWriteDir = BaseWriteDir + "/" + zeldaWriteDir;
            if (FileSystem.PHYSFS_setWriteDir(fullWriteDir) == 0)
                throw new Exception("Cannot set Zelda write directory to '" + fullWriteDir + "': " + FileSystem.PHYSFS_getLastError());

            // 모드 디렉토리를 생성합니다
            if (!String.IsNullOrWhiteSpace(_modWriteDir))
                SetModWriteDir(_modWriteDir);
        }
    }
}
