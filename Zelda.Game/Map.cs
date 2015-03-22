using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zelda.Game
{
    class Map
    {
        readonly string _id;
        public string Id
        {
            get { return _id; }
        }

        string _destinationName;
        public string DestinationName
        {
            get { return _destinationName; }
            set { _destinationName = value; }
        }

        public Map(string id)
        {
            _id = id;
        }

        public void Load(Game game)
        {
            Console.WriteLine("TODO: Load");
        }
    }
}
