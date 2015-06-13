using System;

using Zelda.Game.Engine;

namespace Zelda.Game.Script
{
    public class ScriptGame
    {
        readonly Savegame _savegame;

        public static bool Exists(string fileName)
        {
            return ScriptToCore.Call(() =>
            {
                if (String.IsNullOrWhiteSpace(ModFiles.ModWriteDir))
                    throw new InvalidOperationException("Cannot check savegame: no write directory was specified in mod.xml");

                return ModFiles.DataFileExists(fileName);
            });
        }

        public static ScriptGame Load(string fileName)
        {
            return ScriptToCore.Call(() =>
            {
                if (String.IsNullOrWhiteSpace(ModFiles.ModWriteDir))
                    throw new InvalidOperationException("Cannot check savegame: no write directory was specified in mod.xml");

                Savegame savegame = new Savegame(ScriptContext.MainLoop, fileName);
                savegame.Initialize();
                return new ScriptGame(savegame);
            });
        }

        public static void Delete(string fileName)
        {
            ScriptToCore.Call(() =>
            {
                if (string.IsNullOrEmpty(ModFiles.ModWriteDir))
                    throw new InvalidOperationException("Cannot delete savegame: no mod directory was specified in mod.xml");

                ModFiles.DataFileDelete(fileName);
            });
        }

        ScriptGame(Savegame savegame)
        {
            Debug.CheckAssertion(savegame != null, "rawSaveGame should not be null");

            _savegame = savegame;
            _savegame.ScriptGame = this;
        }

        public int GetLife()
        {
            return ScriptToCore.Call<int>(_savegame.Equipment.GetLife);
        }

        public void SetLife(int life)
        {
            ScriptToCore.Call(() => _savegame.Equipment.SetLife(life));
        }

        public int GetMaxLife()
        {
            return ScriptToCore.Call<int>(_savegame.Equipment.GetMaxLife);
        }

        public void SetMaxLife(int maxLife)
        {
            ScriptToCore.Call(() =>
            {
                if (maxLife < 0)
                    throw new ArgumentOutOfRangeException("value", "Invalid life value: max life must be strictly positive");

                _savegame.Equipment.SetMaxLife(maxLife);
            });
        }

        public ScriptHero Hero
        {
            get { return (_savegame.Game.Hero.ScriptEntity as ScriptHero); }
        }

        public int GetAbility(Ability ability)
        {
            return ScriptToCore.Call(() => _savegame.Equipment.GetAbility(ability));
        }

        public void SetAbility(Ability ability, int level)
        {
            ScriptToCore.Call(() => _savegame.Equipment.SetAbility(ability, level));
        }

        public void Start()
        {
            ScriptToCore.Call(() =>
            {
                if (CurrentMod.GetResources(ResourceType.Map).Count <= 0)
                    throw new InvalidOperationException("Cannot start game: there is no map in this mod");

                Game game = _savegame.Game;
                if (game != null)
                {
                    game.Restart();
                }
                else
                {
                    MainLoop mainLoop = ScriptContext.MainLoop;
                    if (mainLoop.Game != null)
                        mainLoop.Game.Stop();
                    game = new Game(mainLoop, _savegame);
                    mainLoop.SetGame(game);
                }
            });
        }

        public void SetItemAssigned(int slot, ScriptItem item)
        {
            ScriptToCore.Call(() =>
            {
                if (slot < 1 || slot > 2)
                    throw new ArgumentException("The item slot should be 1 or 2", "slot");

                _savegame.Equipment.SetItemAssigned(slot, item._item);
            });
        }

        public bool GetBooleanValue(string key)
        {
            return ScriptToCore.Call(() => _savegame.GetBoolean(key));
        }

        public int GetIntegerValue(string key)
        {
            return ScriptToCore.Call(() => _savegame.GetInteger(key));
        }

        public string GetStringValue(string key)
        {
            return ScriptToCore.Call(() => _savegame.GetString(key));
        }

        public void SetStringValue(string key, string value)
        {
            ScriptToCore.Call(() =>
            {
                ThrowIfInvalidKey(key);
                _savegame.SetString(key, value);
            });
        }

        void ThrowIfInvalidKey(string key)
        {
            if (key[0] == '_')
                throw new ArgumentException("Invalid savegame variable '{0}': names prefixed by '_' are reserved for built-in variables".F(key));
        }

        public void Save()
        {
            ScriptToCore.Call(() =>
            {
                if (String.IsNullOrEmpty(ModFiles.ModWriteDir))
                    throw new InvalidOperationException("Cannot save game: no write directory was specified in mod.xml");
                
                _savegame.Save();
            });
        }

        public void SetStartingLocation(string mapId, string destinationName = null)
        {
            ScriptToCore.Call(() =>
            {
                _savegame.SetString(Savegame.Key.StartingMap, mapId);
                _savegame.SetString(Savegame.Key.StartingPoint, destinationName);
            });
        }

        public ScriptItem GetItem(string itemName)
        {
            return ScriptToCore.Call(() =>
            {
                if (!_savegame.Equipment.ItemExists(itemName))
                    throw new ArgumentException("No such item: '{0}'".F(itemName));
                
                return _savegame.Equipment.GetItem(itemName).ScriptItem;
            });
        }
    }
}
