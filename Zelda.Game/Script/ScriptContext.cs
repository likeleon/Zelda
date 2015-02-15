using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Zelda.Game.Script
{
    class ScriptContext
    {
        readonly MainLoop _mainLoop;
        ObjectCreator _objectCreator;
        ScriptMain _scriptMain;

        public ScriptContext(MainLoop mainLoop)
        {
            _mainLoop = mainLoop;
        }

        public void Initialize()
        {
            _objectCreator = new ObjectCreator(_mainLoop.CurrentMod.Resources, _mainLoop.EngineSystem.ModFiles);

            CreateScriptMain();
            _scriptMain.OnStarted();
        }

        public void Exit()
        {
            if (_scriptMain != null)
                _scriptMain.OnFinished();
        }

        private void CreateScriptMain()
        {
            var mainTypes = _objectCreator.GetTypesImplementing<ScriptMain>();
            if (mainTypes.Count() <= 0)
                throw new InvalidDataException("'ScriptMain' based class not found");

            if (mainTypes.Count() > 1)
                throw new InvalidDataException("'Too many 'ScriptMain' based classes");

            Type mainType = mainTypes.First();
            ConstructorInfo ctor = mainType.GetConstructor(new Type[] { typeof(MainLoop) });
            _scriptMain = (ScriptMain)ctor.Invoke(new object[] { _mainLoop });
        }
    }
}
