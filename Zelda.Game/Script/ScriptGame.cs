using System;

using Zelda.Game.Engine;

namespace Zelda.Game.Script
{
    public class ScriptGame
    {
        readonly Savegame _savegame;

        ScriptGame(Savegame savegame)
        {
            Debug.CheckAssertion(savegame != null, "rawSaveGame should not be null");

            _savegame = savegame;
            _savegame.ScriptGame = this;
        }

        public int GetLife()
        {
            return ScriptTools.ExceptionBoundaryHandle<int>(_savegame.Equipment.GetLife);
        }

        public void SetLife(int life)
        {
            ScriptTools.ExceptionBoundaryHandle(() => _savegame.Equipment.SetLife(life));
        }

        public int GetMaxLife()
        {
            return ScriptTools.ExceptionBoundaryHandle<int>(_savegame.Equipment.GetMaxLife);
        }

        public void SetMaxLife(int maxLife)
        {
            ScriptTools.ExceptionBoundaryHandle(() =>
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
            return ScriptTools.ExceptionBoundaryHandle<int>(() =>
            {
                return _savegame.Equipment.GetAbility(ability);
            });
        }

        public void SetAbility(Ability ability, int level)
        {
            ScriptTools.ExceptionBoundaryHandle(() =>
            {
                _savegame.Equipment.SetAbility(ability, level);
            });
        }

        public static bool Exists(string fileName)
        {
            return ScriptTools.ExceptionBoundaryHandle<bool>(() =>
            {
                if (String.IsNullOrWhiteSpace(ModFiles.ModWriteDir))
                    throw new InvalidOperationException("Cannot check savegame: no write directory was specified in mod.xml");

                return ModFiles.DataFileExists(fileName);
            });
        }

        public static ScriptGame Load(string fileName)
        {
            return ScriptTools.ExceptionBoundaryHandle<ScriptGame>(() =>
            {
                if (String.IsNullOrWhiteSpace(ModFiles.ModWriteDir))
                    throw new InvalidOperationException("Cannot check savegame: no write directory was specified in mod.xml");

                Savegame savegame = new Savegame(ScriptContext.MainLoop, fileName);
                savegame.Initialize();
                return new ScriptGame(savegame);
            });
        }

        public void Start()
        {
            ScriptTools.ExceptionBoundaryHandle(() =>
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
            ScriptTools.ExceptionBoundaryHandle(() =>
            {
                if (slot < 1 || slot > 2)
                    throw new ArgumentException("The item slot should be 1 or 2", "slot");

                _savegame.Equipment.SetItemAssigned(slot, item._item);
            });
        }
    }
}
