using System;
using Zelda.Game.Primitives;

namespace Zelda.Game
{
    class SpriteSystem : IDisposable
    {
        static readonly Cache<string, SpriteAnimationSet> _allAnimationSets = new Cache<string, SpriteAnimationSet>(id => new SpriteAnimationSet(id));

        public void Dispose()
        {
            _allAnimationSets.Values.Do(s => s.Dispose());
        }

        public SpriteAnimationSet GetAnimationSet(string id)
        {
            return _allAnimationSets[id];
        }
    }
}
