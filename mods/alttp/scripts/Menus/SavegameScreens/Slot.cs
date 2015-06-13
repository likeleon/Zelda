﻿using Zelda.Game;
using Zelda.Game.Script;

namespace Alttp.Menus.SavegameScreens
{
    class Slot
    {
        readonly ScriptTextSurface _playerNameText;

        public string FileName { get; private set; }
        public PlayGame Savegame { get; private set; }
        public ScriptSurface NumberImg { get; private set; }
        public ScriptTextSurface PlayerNameText { get { return _playerNameText; } }

        public Slot(int index)
        {
            FileName = "save{0}.dat".F(index);
            Savegame = ScriptGame.Load<PlayGame>(FileName);
            NumberImg = ScriptSurface.Create("menus/selection_menu_save{0}.png".F(index));

            var dialogFont = Fonts.GetDialogFont();
            _playerNameText = ScriptTextSurface.Create(font: dialogFont.Id, fontSize: dialogFont.Size);

            if (ScriptGame.Exists(FileName))
            {
                _playerNameText.SetText(Savegame.GetStringValue("player_name"));
                //TODO 하트 보여주기
            }
            else
            {
                var name = "- {0} -".F(ScriptLanguage.GetString("selection_menu.empty"));
                _playerNameText.SetText(name);
            }
        }
    }
}