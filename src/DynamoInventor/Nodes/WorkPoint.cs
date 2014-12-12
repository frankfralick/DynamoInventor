using System;
using Inventor;

using Dynamo.Models;
using Dynamo.Utilities;
using Microsoft.FSharp.Collections;
using Value = Dynamo.FScheme.Value;
using Dynamo.FSchemeInterop;
using DynamoInventor;
using InventorServices.Persistence;

namespace Dynamo.Nodes
{
    [NodeName("WorkPoint")]
    [NodeCategory(BuiltinNodeCategories_Inventor.INVENTOR_WORKFEATURES)]
    [NodeDescription("Place a work point given a coordinate.")]
    [IsDesignScriptCompatible]
    public class WorkPoint : InventorTransactionNodeWithOneOutput
    {
        public WorkPoint()
        {
            InPortData.Add(new PortData("x", "X coordinate", typeof(Value.Number)));
            InPortData.Add(new PortData("y", "Y coordinate", typeof(Value.Number)));
            InPortData.Add(new PortData("z", "Z coordinate", typeof(Value.Number)));
            OutPortData.Add(new PortData("wp", "The resulting work point.", typeof(Value.Container)));

            RegisterAllPorts();
        }

        //public override Value Evaluate(FSharpList<Value> args)
        //{
        //    double x = ((Value.Number)args[0]).Item;
        //    double y = ((Value.Number)args[1]).Item;
        //    double z = ((Value.Number)args[2]).Item;

        //    Inventor.WorkPoint wp;
          
        //    //If this node has been run already and there is something in ComponentOccurrenceKeys,
        //    //then modify the object based on the inputs.
        //    //Could input values be stored so that re-evaluation could be skipped?
        //    if (ComponentOccurrenceKeys.Count != 0)
        //    {
        //        //If we find the byte[], and can bind to the object, modify it.
        //        if (InventorUtilities.TryBindReferenceKey(ComponentOccurrenceKeys[0], out wp)) 
        //        {
        //            MoveWorkPoint(x, y, z, wp);
        //        }
                
        //        else 
        //        {
        //            wp = CreateNewWorkPoint(x, y, z);
        //        }
        //    }

            //Otherwise we need to create the thing this node is trying to make, and assign its
            //ReferenceKey byte[] to ComponentOccurrenceKeys[0].
        //    else
        //    {
        //        wp = CreateNewWorkPoint(x, y, z);
        //    }
       
        //    return Value.NewContainer(wp);
        //}

        internal static void MoveWorkPoint(double x, double y, double z, Inventor.WorkPoint wp)
        {
            //Point newLocation = InventorSettings.InventorApplication.TransientGeometry.CreatePoint(x, y, z);
            Point newLocation = PersistenceManager.InventorApplication.TransientGeometry.CreatePoint(x, y, z);
            AssemblyWorkPointDef wpDef = (AssemblyWorkPointDef)wp.Definition;
            wpDef.Point = newLocation;
        }

        internal Inventor.WorkPoint CreateNewWorkPoint(double x, double y, double z)
        {
            this.VerifyContextSettings();
            Inventor.WorkPoint wp;
            //AssemblyDocument assDoc = InventorSettings.ActiveAssemblyDoc;
            AssemblyDocument assDoc = PersistenceManager.ActiveAssemblyDoc;
            //AssemblyDocument assDoc = (AssemblyDocument)InventorSettings.InventorApplication.ActiveDocument;
            AssemblyComponentDefinition compDef = (AssemblyComponentDefinition)assDoc.ComponentDefinition;
            //Point point = InventorSettings.InventorApplication.TransientGeometry.CreatePoint(x, y, z);
            Point point = PersistenceManager.InventorApplication.TransientGeometry.CreatePoint(x, y, z);
            wp = compDef.WorkPoints.AddFixed(point, false);
            
            byte[] refKey = new byte[] { };
            //wp.GetReferenceKey(ref refKey, (int)InventorSettings.KeyContext);
            wp.GetReferenceKey(ref refKey, (int)ReferenceManager.KeyContext);

            ComponentOccurrenceKeys.Add(refKey);
            return wp;
        }
    }
}
