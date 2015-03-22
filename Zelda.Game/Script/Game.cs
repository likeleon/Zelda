using System;
using Zelda.Game.Engine;
using RawSaveGame = Zelda.Game.SaveGame;
using RawGame = Zelda.Game.Game;

namespace Zelda.Game.Script
{
    public class Game
    {
        public int Life
        {
            get 
            {
                return ScriptTools.ExceptionBoundaryHandle<int>(() =>
                {
                    return _rawSaveGame.Equipment.Life;
                });
            }
            set 
            {
                ScriptTools.ExceptionBoundaryHandle(() =>
                {
                    _rawSaveGame.Equipment.Life = value;
                });
            }
        }

        public int MaxLife
        {
            get 
            {
                return ScriptTools.ExceptionBoundaryHandle<int>(() =>
                {
                    return _rawSaveGame.Equipment.MaxLife;
                });
            }
            set
            {
                ScriptTools.ExceptionBoundaryHandle(() =>
                {
                    if (value < 0)
                        throw new ArgumentOutOfRangeException("value", "Invalid life value: max life must be strictly positive");

                    _rawSaveGame.Equipment.MaxLife = value;
                });
            }
        }

        readonly RawSaveGame _rawSaveGame;

        public static bool Exists(string fileName)
        {
            return ScriptTools.ExceptionBoundaryHandle<bool>(() =>
            {
                if (String.IsNullOrWhiteSpace(ModFiles.ModWriteDir))
                    throw new InvalidOperationException("Cannot check savegame: no write directory was specified in mod.xml");

                return ModFiles.DataFileExists(fileName);
            });
        }

        public static Game Load(string fileName)
        {
            return ScriptTools.ExceptionBoundaryHandle<Game>(() =>
            {
                if (String.IsNullOrWhiteSpace(ModFiles.ModWriteDir))
                    throw new InvalidOperationException("Cannot check savegame: no write directory was specified in mod.xml");

                RawSaveGame rawSaveGame = new RawSaveGame(ScriptContext.MainLoop, fileName);
                return new Game(rawSaveGame);
            });
        }

        Game(RawSaveGame rawSaveGame)
        {
            Debug.CheckAssertion(rawSaveGame != null, "rawSaveGame should not be null");

            _rawSaveGame = rawSaveGame;
        }

        public void Start()
        {
            ScriptTools.ExceptionBoundaryHandle(() =>
            {
                if (CurrentMod.GetResources(ResourceType.Map).Count <= 0)
                    throw new InvalidOperationException("Cannot start game: there is no map in this mod");

                RawGame game = _rawSaveGame.Game;
                if (game != null)
                {
                    game.Restart();
                }
                else
                {
                    MainLoop mainLoop = ScriptContext.MainLoop;
                    if (mainLoop.Game != null)
                        mainLoop.Game.Stop();
                    game = new RawGame(mainLoop, _rawSaveGame);
                    mainLoop.SetGame(game);
                }
            });
        }
    }
}
