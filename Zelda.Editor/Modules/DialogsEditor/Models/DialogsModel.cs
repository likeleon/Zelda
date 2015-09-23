using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zelda.Editor.Core.Mods;
using Zelda.Game;

namespace Zelda.Editor.Modules.DialogsEditor.Models
{
    class DialogsModel
    {
        readonly IMod _mod;
        readonly string _languageId;

        public DialogsModel(IMod mod, string languageId)
        {
            if (mod == null)
                throw new ArgumentNullException("mod");

            if (languageId.IsNullOrEmpty())
                throw new ArgumentNullException("languageId");

            _mod = mod;
            _languageId = languageId;

            var path = _mod.GetDialogsPath(languageId);
        }
    }
}
