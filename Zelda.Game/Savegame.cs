using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Zelda.Game.LowLevel;
using Zelda.Game.Script;

namespace Zelda.Game
{
    [XmlRoot("SaveData")]
    public class SaveXmlData
    {
        public class KeySavedValue
        {
            public string Key { get; set; }
            public SavedValue Value { get; set; }
        }

        [XmlElement("KeySavedValue")]
        public KeySavedValue[] KeySavedValues { get; set; }
    }

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

        static readonly int SaveGameVersion = 2;

        readonly Dictionary<string, SavedValue> _savedValues = new Dictionary<string, SavedValue>();
        
        public string FileName { get; }
        public bool IsEmpty { get; private set; }

        public ScriptGame ScriptGame { get; set; }
        public Equipment Equipment { get; private set; }
        public Game Game { get; set; }

        public void Initialize()
        {
            Debug.CheckAssertion(!Core.Mod.ModFiles.ModWriteDir.IsNullOrWhiteSpace(),
                "The mod write directory for savegames was not set in mod.xml");

            if (!Core.Mod.ModFiles.DataFileExists(FileName))
            {
                IsEmpty = true;
                SetInitialValues();
            }
            else
            {
                IsEmpty = false;
                ImportFromFile();
            }

            Equipment.LoadItems();
        }

        public void NotifyGameFinished()
        {
            Equipment.NotifyGameFinished();
        }

        public Savegame(string fileName)
        {
            FileName = fileName;
            Equipment = new Equipment(this);
        }

        void SetInitialValues()
        {
            SetInteger(Key.SaveGameVersion, SaveGameVersion);

            SetDefaultKeyboardControls();

            Equipment.SetMaxLife(1);
            Equipment.SetLife(1);
            Equipment.SetAbility(Ability.Tunic, 1);    // 스프라이트 표현을 위해 필수적으로 필요합니다
        }

        void SetDefaultKeyboardControls()
        {
            SetString(Key.KeyboardAction, InputEvent.GetKeyboardKeyName(KeyboardKey.Space));
            SetString(Key.KeyboardAttack, InputEvent.GetKeyboardKeyName(KeyboardKey.C));
            SetString(Key.KeyboardItem1, InputEvent.GetKeyboardKeyName(KeyboardKey.X));
            SetString(Key.KeyboardItem2, InputEvent.GetKeyboardKeyName(KeyboardKey.V));
            SetString(Key.KeyboardPause, InputEvent.GetKeyboardKeyName(KeyboardKey.D));
            SetString(Key.KeyboardRight, InputEvent.GetKeyboardKeyName(KeyboardKey.Right));
            SetString(Key.KeyboardUp, InputEvent.GetKeyboardKeyName(KeyboardKey.Up));
            SetString(Key.KeyboardLeft, InputEvent.GetKeyboardKeyName(KeyboardKey.Left));
            SetString(Key.KeyboardDown, InputEvent.GetKeyboardKeyName(KeyboardKey.Down));
        }

        void ImportFromFile()
        {
            try
            {
                var saveData = Core.Mod.ModFiles.DataFileRead(FileName).XmlDeserialize<SaveXmlData>();
                foreach (var pair in saveData.KeySavedValues)
                    _savedValues[pair.Key] = pair.Value;
            }
            catch (Exception ex)
            {
                Debug.Die("Failed to load savegame file '{0}': {1}".F(FileName, ex.Message));
            }
        }

        public void Save()
        {
            var saveData = new SaveXmlData();
            saveData.KeySavedValues = _savedValues
                .Select(v => new SaveXmlData.KeySavedValue() { Key = v.Key, Value = v.Value })
                .ToArray();

            var stream = new MemoryStream();
            saveData.XmlSerialize(stream);
            Core.Mod.ModFiles.DataFileSave(FileName, stream.GetBuffer(), stream.Length);
            IsEmpty = false;
        }

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
    }
}
