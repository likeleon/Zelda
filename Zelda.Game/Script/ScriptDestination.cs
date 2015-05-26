using Zelda.Game.Entities;

namespace Zelda.Game.Script
{
    public class ScriptDestination : ScriptEntity
    {
        readonly Destination _destination;
        internal Destination Destination
        {
            get { return _destination; }
        }

        internal ScriptDestination(Destination destination)
            : base(destination)
        {
            _destination = destination;
        }
    }
}
