using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;

using Inventor;


namespace DynamoInventor
{
    /// <summary>
    /// This class holds static references that the application needs.  
    /// </summary>
    /// 
    //TODO: This should probably be deleted entirely.  None of this seems valid anymore.
    public class InventorSettings
    {
        //public static Inventor.Application InventorApplication { get; set; }

        //public static AssemblyDocument ActiveAssemblyDoc { get; set; }

        //public static Stack<ComponentOccurrencesContainer> ComponentOccurrencesContainers
            //= new Stack<ComponentOccurrencesContainer>(new[] { new ComponentOccurrencesContainer() });

        //public static ReferenceKeyManager KeyManager { get; set; }
        
        //public static int? KeyContext { get; set; }

        //public static byte[] KeyContextArray { get; set; }

        //This is the name of the storage for Dynamo object bindings.
        private static string dynamoStorageName = "Dynamo";
        public static string DynamoStorageName
        {
            get { return dynamoStorageName; }
        }
    }
}
