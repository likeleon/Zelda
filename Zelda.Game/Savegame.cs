using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Zelda.Game.Engine;
using Zelda.Game.Script;

namespace Zelda.Game
{
    class Savegame
    {
        public enum Key
        {
            SaveGameVersion,        // 이 세이브게임 파일의 포맷
            StartingMap,            // 게임을 시작할 맵 아이디
            StartingPoint,          // 시작 위치 이름
            KeyboardAction,         // 액션 커맨드에 매핑된 키보드 키
            KeyboardAttack,         // 공격 커맨드에 매핑된 키보드 키
            KeyboardItem1,          // 아이템 1 커맨드에 매핑된 키보드 키
            KeyboardItem2,          // 아이템 2 커맨드에 매핑된 키보드 키
            KeyboardPause,          // 일시 정지 커맨드에 매핑된 키보드 키
            KeyboardRight,          // 우 커맨드에 매핑된 키보드 키
            KeyboardUp,             // 상 커맨드에 매핑된 키보드 키
            KeyboardLeft,           // 좌 커맨드에 매핑된 키보드 키
            KeyboardDown,           // 하 커맨드에 매핑된 키보드 키
            CurrentLife,            // 현재 체력 포인트
            MaxLife,                // 최대 체력 포인트
            AbilityTunic,           // 저항 레벨
            AbilitySword,           // 공격 레벨
            AbilitySwordKnowledge,  // 스핀 공격 레벨
            AbilityShield,         // 보호 레벨
            AbilityLift,            // 들기 레벨
            AbilitySwim,            // 수영 레벨
            AbilityRun,             // 달리기 레벨
            AbilityDetectWeakWalls, // 약한 벽 감지 레벨
        }

        [XmlRoot("SaveData")]
        public class SaveData
        {
            [XmlElement("SavedValue")]
            public Dictionary<string, SavedValue> SavedValues { get; set; }
        }

        static readonly int SaveGameVersion = 2;

        public ScriptGame ScriptGame { get; set; }

        #region 파일 상태
        bool _empty = true;
        public bool IsEmpty
        {
            get { return _empty; }
        }

        public void Initialize()
        {
            string modWriteDir = ModFiles.ModWriteDir;
            Debug.CheckAssertion(!String.IsNullOrWhiteSpace(modWriteDir),
                "The mod write directory for savegames was not set in mod.xml");

            if (!ModFiles.DataFileExists(_fileName))
            {
                _empty = true;
                SetInitialValues();
            }
            else
            {
                _empty = false;
                ImportFromFile();
            }

            Equipment.LoadItems();
        }
        
        readonly string _fileName;
        public string FileName
        {
            get { return _fileName; }
        }
        #endregion

        #region 세이브되지 않는 데이터
        readonly Equipment _equipment;
        public Equipment Equipment
        {
            get { return _equipment; }
        }

        public Game Game { get; set; }

        readonly MainLoop _mainLoop;

        public void NotifyGameFinished()
        {
            _equipment.NotifyGameFinished();
        }
        #endregion

        #region 생성
        public Savegame(MainLoop mainLoop, string fileName)
        {
            _mainLoop = mainLoop;
            _fileName = fileName;
            _equipment = new Equipment(this);
        }
        #endregion

        void SetInitialValues()
        {
            SetInteger(Key.SaveGameVersion, SaveGameVersion);

            SetDefaultKeyboardControls();

            _equipment.SetMaxLife(1);
            _equipment.SetLife(1);
            _equipment.SetAbility(Ability.Tunic, 1);    // 스프라이트 표현을 위해 필수적으로 필요합니다
        }

        void SetDefaultKeyboardControls()
        {
            SetString(Key.KeyboardAction, InputEvent.GetKeyboardKeyName(KeyboardKey.KEY_SPACE));
            SetString(Key.KeyboardAttack, InputEvent.GetKeyboardKeyName(KeyboardKey.KEY_c));
            SetString(Key.KeyboardItem1, InputEvent.GetKeyboardKeyName(KeyboardKey.KEY_x));
            SetString(Key.KeyboardItem2, InputEvent.GetKeyboardKeyName(KeyboardKey.KEY_v));
            SetString(Key.KeyboardPause, InputEvent.GetKeyboardKeyName(KeyboardKey.KEY_d));
            SetString(Key.KeyboardRight, InputEvent.GetKeyboardKeyName(KeyboardKey.KEY_RIGHT));
            SetString(Key.KeyboardUp, InputEvent.GetKeyboardKeyName(KeyboardKey.KEY_UP));
            SetString(Key.KeyboardLeft, InputEvent.GetKeyboardKeyName(KeyboardKey.KEY_LEFT));
            SetString(Key.KeyboardDown, InputEvent.GetKeyboardKeyName(KeyboardKey.KEY_DOWN));
        }

        void ImportFromFile()
        {
            try
            {
                using (MemoryStream stream = ModFiles.DataFileRead(_fileName))
                {
                    SaveData saveData = stream.XmlDeserialize<SaveData>();
                    foreach (var pair in saveData.SavedValues)
                        _savedValues[pair.Key] = pair.Value;
                }
            }
            catch (Exception ex)
            {
                Debug.Die("Failed to load savegame file '{0}': {1}".F(_fileName, ex.Message));
            }
        }

        #region 데이터
        public class SavedValue
        {
            public enum Types
            {
                String,
                Integer,
                Boolean
            }

            public Types Type { get; set; }
            public string StringData { get; set; }
            public int IntData { get; set; }
        }

        readonly Dictionary<string, SavedValue> _savedValues = new Dictionary<string, SavedValue>();
        
        bool TypeIs(string key, SavedValue.Types type)
        {
            SavedValue value;
            if (!_savedValues.TryGetValue(key, out value))
                return false;

            return (value.Type == type);
        }

        public bool IsInteger(string key)
        {
            return TypeIs(key, SavedValue.Types.Integer);
        }

        public void SetInteger(string key, int value)
        {
            _savedValues[key] = new SavedValue()
            {
                Type = SavedValue.Types.Integer,
                IntData = value
            };
        }

        public void SetInteger(Key key, int value)
        {
            SetInteger(key.ToString(), value);
        }

        public int GetInteger(string key)
        {
            int result = 0;
            SavedValue savedValue;
            if (_savedValues.TryGetValue(key, out savedValue))
            {
                Debug.CheckAssertion(savedValue.Type == SavedValue.Types.Integer, "Value '{0}' is not an integer".F(key));
                result = savedValue.IntData;
            }
            return result;
        }

        public int GetInteger(Key key)
        {
            return GetInteger(key.ToString());
        }

        public bool IsString(string key)
        {
            return TypeIs(key, SavedValue.Types.String);
        }

        public void SetString(string key, string value)
        {
            _savedValues[key] = new SavedValue()
            {
                Type = SavedValue.Types.String,
                StringData = value
            };
        }

        public void SetString(Key key, string value)
        {
            SetString(key.ToString(), value);
        }

        public string GetString(string key)
        {
            SavedValue savedValue;
            if (_savedValues.TryGetValue(key, out savedValue))
            {
                Debug.CheckAssertion(savedValue.Type == SavedValue.Types.String, "Value '{0}' is not a string".F(key));
                return savedValue.StringData;
            }
            return String.Empty;
        }

        public string GetString(Key key)
        {
            return GetString(key.ToString());
        }

        public bool IsBoolean(string key)
        {
            return TypeIs(key, SavedValue.Types.Boolean);
        }

        public void SetBoolean(string key, bool value)
        {
            _savedValues[key] = new SavedValue()
            {
                Type = SavedValue.Types.Boolean,
                IntData = value ? 1 : 0
            };
        }

        public void SetBoolean(Key key, bool value)
        {
            SetBoolean(key.ToString(), value);
        }

        public bool GetBoolean(string key)
        {
            SavedValue savedValue;
            if (_savedValues.TryGetValue(key, out savedValue))
            {
                Debug.CheckAssertion(savedValue.Type == SavedValue.Types.Boolean, "Value '{0}' is not a boolean".F(key));
                return (savedValue.IntData != 0);
            }
            return false;
        }

        public bool GetBoolean(Key key)
        {
            return GetBoolean(key.ToString());
        }
        #endregion
    }
}
