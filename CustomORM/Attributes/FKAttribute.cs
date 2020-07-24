using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomORM.Attributes
{
    public class FKAttribute: Attribute
    {
        public string nameFKObject;

        public FKAttribute(string nameFKObject)
        {
            this.nameFKObject = nameFKObject;
        }
    }
}
