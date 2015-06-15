﻿using Alttp.Menus;
using System;
using Zelda.Game;
using Zelda.Game.Script;

namespace Alttp.Maps
{
    [Id("0")]
    class Intro : ScriptMap
    {
        int _frescoIndex;
        ScriptSprite _frescoSprite;

        protected override void OnStarted(ScriptDestination destination)
        {
            var fresco = GetEntity<ScriptNpc>("fresco");
            _frescoSprite = fresco.Sprite;
            _frescoSprite.SetIgnoreSuspended(true);
            Game.StartDialog("intro0", null, (_) =>
            {
                ScriptAudio.PlayMusic("legend");
                NextFresco();
            });
        }
        
        void NextFresco()
        {
            if (_frescoIndex < 6)
            {
                ++_frescoIndex;
                Game.StartDialog("intro{0}".F(_frescoIndex), null, (_) =>
                {
                    _frescoSprite.FadeOut();
                    ScriptTimer.Start(null, 600, (Action)NextFresco);
                });
                _frescoSprite.SetAnimation(_frescoIndex.ToString());
                _frescoSprite.FadeIn();
            }
            else
            {
                (Game as PlayGame).DialogBox.SetDialogStyle(DialogBoxStyle.Box);
            }
        }
    }
}
