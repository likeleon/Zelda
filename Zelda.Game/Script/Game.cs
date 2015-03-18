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
            get { return _rawSaveGame.Equipment.Life; }
            set { _rawSaveGame.Equipment.Life = value; }
        }

        public int MaxLife
        {
            get { return _rawSaveGame.Equipment.MaxLife; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value", "Invalid life value: max life must be strictly positive");

                _rawSaveGame.Equipment.MaxLife = value;
            }
        }

        readonly RawSaveGame _rawSaveGame;

        public static bool Exists(string fileName)
        {
            if (String.IsNullOrWhiteSpace(ModFiles.ModWriteDir))
                throw new Exception("Cannot check savegame: no write directory was specified in Mod.xml");

            return ModFiles.DataFileExists(fileName);
        }

        public static Game Load(string fileName)
        {
            if (String.IsNullOrWhiteSpace(ModFiles.ModWriteDir))
                throw new Exception("Cannot check savegame: no write directory was specified in Mod.xml");

            RawSaveGame rawSaveGame = new RawSaveGame(ScriptContext.MainLoop, fileName);
            rawSaveGame.Initialize();
            return new Game(rawSaveGame);
        }

        Game(RawSaveGame rawSaveGame)
        {
            if (rawSaveGame == null)
                throw new ArgumentNullException("rawSaveGame");

            _rawSaveGame = rawSaveGame;
        }

        public void Start()
        {
            if (CurrentMod.GetResources(ResourceType.Map).Count <= 0)
                throw new Exception("Cannot start game: there is no map in this mod");

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
        }
    }
}
