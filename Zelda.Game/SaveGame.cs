using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Zelda.Game.Engine;

namespace Zelda.Game
{
    class Savegame
    {
        public enum Key
        {
            SaveGameVersion,        // 이 세이브게임 파일의 포맷
            StartingMap,            // 게임을 시작할 맵 아이디
            StartingPoint,          // 시작 위치 이름
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

        [XmlRoot("SaveData")]
        public class SaveData
        {
            [XmlElement("SavedValue")]
            public Dictionary<Key, SavedValue> SavedValues { get; set; }
        }

        static readonly int SaveGameVersion = 2;

        bool _empty = true;
        public bool IsEmpty
        {
            get { return _empty; }
        }

        readonly Equipment _equipment;
        public Equipment Equipment
        {
            get { return _equipment; }
        }

        public Game Game { get; set; }

        readonly MainLoop _mainLoop;
        readonly string _fileName;
        readonly Dictionary<Key, SavedValue> _savedValues = new Dictionary<Key,SavedValue>();

        public Savegame(MainLoop mainLoop, string fileName)
        {
            _mainLoop = mainLoop;
            _fileName = fileName;
            _equipment = new Equipment(this);

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
        }

        private void SetInitialValues()
        {
            SetInteger(Key.SaveGameVersion, SaveGameVersion);

            _equipment.MaxLife = 1;
            _equipment.Life = 1;
            _equipment.SetAbility(Ability.Tunic, 1);    // 스프라이트 표현을 위해 필수적으로 필요합니다
        }

        private void ImportFromFile()
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

        public void SetInteger(Key key, int value)
        {
            _savedValues[key] = new SavedValue()
            {
                Type = SavedValue.Types.Integer,
                IntData = value
            };
        }

        public int GetInteger(Key key)
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

        public void SetString(Key key, string value)
        {
            _savedValues[key] = new SavedValue()
            {
                Type = SavedValue.Types.String,
                StringData = value
            };
        }

        public string GetString(Key key)
        {
            SavedValue savedValue;
            if (_savedValues.TryGetValue(key, out savedValue))
            {
                Debug.CheckAssertion(savedValue.Type != SavedValue.Types.String, "Value '{0}' is not a string".F(key));
                return savedValue.StringData;
            }
            return String.Empty;
        }
    }
}
