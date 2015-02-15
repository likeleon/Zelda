using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zelda.Game.Engine;

namespace Zelda.Game
{
    class CurrentMod
    {
        readonly ModFiles _modFiles;
        
        ModResources _resources;
        internal ModResources Resources
        {
            get { return _resources; }
        }

        public CurrentMod(ModFiles modFiles)
        {
            _modFiles = modFiles;
        }

        public void Initialize()
        {
            _resources = new ModResources();
            _resources.ImportFromModFile(_modFiles, "ProjectDB.xml");
        }

        public void Quit()
        {
            _resources.Clear();
        }
    }
}
