using Zelda.Game;

namespace Sample.Maps
{
    [Id("first_map")]
    class FirstMap : Map
    {
        [ObjectCreator.UseCtor]
        public FirstMap(string id)
            : base(id)
        {
        }
    }
}
