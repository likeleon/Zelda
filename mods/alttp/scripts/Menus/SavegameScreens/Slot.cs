using Zelda.Game;
using Zelda.Game.LowLevel;
using Zelda.Game.Script;

namespace Alttp.Menus.SavegameScreens
{
    class Slot
    {
        public string FileName { get; }
        public Savegame Savegame { get; }
        public Surface NumberImg { get; }
        public TextSurface PlayerNameText { get; }

        public Slot(int index)
        {
            FileName = "save{0}.dat".F(index);
            Savegame = Savegame.Load(FileName);
            NumberImg = Surface.Create("menus/selection_menu_save{0}.png".F(index), true);

            var dialogFont = Fonts.GetDialogFont();
            PlayerNameText = TextSurface.Create(font: dialogFont.Id, fontSize: dialogFont.Size);

            if (Savegame.Exists(FileName))
            {
                PlayerNameText.SetText(Savegame.GetString("player_name"));
                //TODO 하트 보여주기
            }
            else
            {
                var name = "- {0} -".F(Core.Mod.GetString("selection_menu.empty"));
                PlayerNameText.SetText(name);
            }
        }
    }
}
