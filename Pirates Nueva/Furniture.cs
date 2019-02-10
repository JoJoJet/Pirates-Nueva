using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pirates_Nueva
{
    public class Furniture
    {
        public Ship Ship { get; }

        public FurnitureDef Def { get; }
        public string ID => Def.ID;
    }
}
