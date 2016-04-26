using System;
using Zelda.Game.Lowlevel;

namespace Zelda.Game.Script
{
    public class ScriptGame : IMenuContext
    {
        Savegame _savegame;

        public bool IsDialogEnabled
        {
            get
            {
                if (_savegame.Game == null)
                    return false;

                return _savegame.Game.IsDialogEnabled;
            }
        }

        public ScriptMap Map
        {
            get
            {
                if (_savegame.Game == null || !_savegame.Game.HasCurrentMap)
                    return null;

                return _savegame.Game.CurrentMap.ScriptMap;
            }
        }

        public static bool Exists(string fileName)
        {
            return ScriptToCore.Call(() =>
            {
                if (String.IsNullOrWhiteSpace(ModFiles.ModWriteDir))
                    throw new InvalidOperationException("Cannot check savegame: no write directory was specified in mod.xml");

                return ModFiles.DataFileExists(fileName);
            });
        }

        public static T Load<T>(string fileName) where T : ScriptGame
        {
            return ScriptToCore.Call<T>(() =>
            {
                if (String.IsNullOrWhiteSpace(ModFiles.ModWriteDir))
                    throw new InvalidOperationException("Cannot check savegame: no write directory was specified in mod.xml");

                T game = (T)Activator.CreateInstance(typeof(T));
                game._savegame = new Savegame(fileName);
                game._savegame.Initialize();
                game._savegame.ScriptGame = game;
                return game;
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

        public ScriptHero Hero { get { return _savegame.Game.Hero.AsScriptEntity<ScriptHero>(); } }

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
                    Framework framework = ScriptContext.Framework;
                    if (framework.Game != null)
                        framework.Game.Stop();
                    game = new Game(framework, _savegame);
                    framework.SetGame(game);
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

        public void StartDialog(string dialogId, object info, Action<object> callback)
        {
            ScriptToCore.Call(() =>
            {
                if (!CurrentMod.DialogExists(dialogId))
                    throw new ArgumentException("No such dialog: '{0}'".F(dialogId));

                var game = _savegame.Game;
                if (game == null)
                    throw new InvalidOperationException("Cannot start dialog: this game is not running.");

                if (game.IsDialogEnabled)
                    throw new InvalidOperationException("Cannot start dialog: another dialog is already active.");

                game.StartDialog(dialogId, info, callback);
            });
        }

        public void StopDialog(object status = null)
        {
            ScriptToCore.Call(() =>
            {
                var game = _savegame.Game;
                if (game == null)
                    throw new InvalidOperationException("Cannot stop dialog: this game is not running.");

                if (!game.IsDialogEnabled)
                    throw new InvalidOperationException("Cannot stop dialog: no dialog is active.");

                game.StopDialog(status);
            });
        }

        internal void NotifyStarted()
        {
            CoreToScript.Call(OnStarted);
        }

        internal void NotifyFinished()
        {
            CoreToScript.Call(OnFinished);
        }

        internal bool NotifyDialogStarted(Dialog dialog, object info)
        {
            return CoreToScript.Call<bool>(() => OnDialogStarted(dialog, info));
        }

        internal void NotifyDialogFinished(Dialog dialog)
        {
            CoreToScript.Call(() => OnDialogFinished(dialog));
        }

        internal void NotifyDraw(ScriptSurface dstSurface)
        {
            CoreToScript.Call(() => OnDraw(dstSurface));
        }

        internal bool NotifyCommandPressed(GameCommand command)
        {
            return CoreToScript.Call(() => 
            {
                if (OnCommandPressed(command))
                    return true;

                return ScriptMenu.OnCommandPressed(this, command);
            });
        }

        protected virtual void OnStarted()
        {
        }

        protected virtual void OnFinished()
        {
        }

        protected virtual bool OnDialogStarted(Dialog dialog, object info)
        {
            return false;
        }

        protected virtual void OnDialogFinished(Dialog dialog)
        {
        }

        protected virtual void OnDraw(ScriptSurface dstSurface)
        {
        }

        protected virtual bool OnCommandPressed(GameCommand command)
        {
            return false;
        }
    }
}
