using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamo.Nodes
{
    //I get what's going on with node attributes in dynBaseTypes, but maybe
    //specific api's that are implementing Dynamo should do attribute constants
    //specific to that environment as part of their own project.
    public static class BuiltinNodeCategories_Inventor
    {
        public const string INVENTOR = "Inventor";
        public const string INVENTOR_WORKFEATURES = "Inventor.WorkFeatures";
        //etc.
    }
}
