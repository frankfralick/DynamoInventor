using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml;
using Microsoft.FSharp.Collections;
using Inventor;

using Dynamo.Models;
using Value = Dynamo.FScheme.Value;
using InventorServices.Persistence;

namespace DynamoInventor
{
    /// <summary>
    /// This class will be the parent class of nodes that create or modify objects
    /// within Inventor.  One of the main functions of this class is to keep track
    /// of whether or not a node is being run for the first time, how many runs
    /// have occured etc.  Nodes that inherit from this that create objects will
    /// have object creation logic the first time, but on subsequent runs, the object
    /// needs to be modified rather than created.
    /// </summary>
    public abstract class InventorTransactionNode : NodeModel
    {
        private int _runCount;

        protected Inventor.AssemblyDocument AssemblyDocument
        {
            //get { return InventorSettings.ActiveAssemblyDoc; }
            get { return PersistenceManager.ActiveAssemblyDoc; }
        }

        private List<List<byte[]>> elements
        {
            get
            {
                return InventorSettings.ComponentOccurrencesContainers.Peek()[GUID];
            }
        }

        public List<byte[]> compOccKeys = new List<byte[]>();
        public List<byte[]> ComponentOccurrenceKeys
        {
            get
            { 
                return compOccKeys;
            }
            set { value = compOccKeys; }
        }

        public IEnumerable<byte[]> AllComponentOccurrenceKeys
        {
            get
            {
                return elements.SelectMany(x => x);
            }
        }

        protected InventorTransactionNode()
        {
            ArgumentLacing = LacingStrategy.Longest;

            //In DynamoRevit there is 'RegisterAllElementsDeleteHook' and some event stuff here
            //Don't understand the overlap between ElementsContainer methods and those in 
            //RevitTransactionNode.
        }

        //protected override void OnEvaluate()
        //{
        //    base.OnEvaluate();
        //    _runCount++;
        //}

        /// <summary>
        /// Custom save data for your Element. 
        /// </summary>
        /// <param name="xmlDoc">The XmlDocument representing the whole workspace containing this Element.</param>
        /// <param name="nodeElement">The XmlElement representing this Element.</param>
        /// <param name="context">Why is this being called?</param>
        protected override void SaveNode(XmlDocument xmlDoc, XmlElement nodeElement, SaveContext context)
        {
            //if (InventorSettings.ActiveAssemblyDoc == null)
            if (PersistenceManager.ActiveAssemblyDoc == null)
            {
                //InventorSettings.ActiveAssemblyDoc = (AssemblyDocument)InventorSettings.InventorApplication.ActiveDocument;
                PersistenceManager.ActiveAssemblyDoc = (AssemblyDocument)PersistenceManager.InventorApplication.ActiveDocument;
            }

            //If KeyContext hasn't been set ever, what does that mean? Fix this.
            //if (InventorSettings.KeyContextArray == null)
            if (ReferenceManager.KeyContextArray == null)
            {
                //if (InventorSettings.KeyContext == null)
                if (ReferenceManager.KeyContext == null)
                {
                    //InventorSettings.KeyContext = InventorSettings.ActiveAssemblyDoc.ReferenceKeyManager.CreateKeyContext();
                    ReferenceManager.KeyContext = PersistenceManager.ActiveAssemblyDoc.ReferenceKeyManager.CreateKeyContext();
                }
                byte[] keyContextArray = new byte[] { };
                //InventorSettings.ActiveAssemblyDoc.ReferenceKeyManager.SaveContextToArray((int)InventorSettings.KeyContext, ref keyContextArray);
                //InventorSettings.KeyContextArray = keyContextArray;
                PersistenceManager.ActiveAssemblyDoc.ReferenceKeyManager.SaveContextToArray((int)ReferenceManager.KeyContext, ref keyContextArray);
                ReferenceManager.KeyContextArray = keyContextArray;
            }
            try
            {
                var nodeType = Type.GetType(nodeElement.Name);
                var invNodeType = typeof(InventorTransactionNode);
                var objectsKeysList = xmlDoc.CreateElement("objects");
                nodeElement.AppendChild(objectsKeysList);
                foreach (var key in this.ComponentOccurrenceKeys)
                {
                    var objectKey = xmlDoc.CreateElement("object");
                    objectsKeysList.AppendChild(objectKey);
                    string keyString = Convert.ToBase64String(key);
                    //string contextString = Convert.ToBase64String(InventorSettings.KeyContextArray);
                    string contextString = Convert.ToBase64String(ReferenceManager.KeyContextArray);
                    objectKey.SetAttribute("context", contextString);
                    objectKey.SetAttribute("key", keyString);
                }
                
            }

            catch (Exception y)
            {
                System.Windows.Forms.MessageBox.Show(y.ToString());
            }  
        }

        /// <summary>
        /// Custom data for your Element.
        /// SaveNode() in order to write the data when saved.
        /// </summary>
        /// <param name="nodeElement">The XmlNode representing this Element.</param>
        protected override void LoadNode(XmlNode nodeElement)
        {
            //if (InventorSettings.ActiveAssemblyDoc == null)
            if (PersistenceManager.ActiveAssemblyDoc == null)
            {
                //InventorSettings.ActiveAssemblyDoc = (AssemblyDocument)InventorSettings.InventorApplication.ActiveDocument;
                PersistenceManager.ActiveAssemblyDoc = (AssemblyDocument)PersistenceManager.InventorApplication.ActiveDocument;
            }
      
            if (nodeElement.HasChildNodes)
            {
                foreach (XmlNode objectsNode in nodeElement.ChildNodes)
                {
                    if (objectsNode.Name == "objects")
                    {
                        if (objectsNode.HasChildNodes)
                        {
                            foreach (XmlNode objectNode in objectsNode.ChildNodes)
                            {
                                string contextString = objectNode.Attributes["context"].Value;
                                string keyString = objectNode.Attributes["key"].Value;
                                byte[] context = Convert.FromBase64String(contextString);
                                byte[] key = Convert.FromBase64String(keyString);
                                //InventorSettings.KeyContextArray = context;
                                ReferenceManager.KeyContextArray = context;
                                this.ComponentOccurrenceKeys.Add(key);
                            }
                        }
                    }
                }
            }
        }

        protected void VerifyContextSettings()
        {
            //if (InventorSettings.ActiveAssemblyDoc == null)
            if (PersistenceManager.ActiveAssemblyDoc == null)
            {
                //InventorSettings.ActiveAssemblyDoc = (AssemblyDocument)InventorSettings.InventorApplication.ActiveDocument;
                PersistenceManager.ActiveAssemblyDoc = (AssemblyDocument)PersistenceManager.InventorApplication.ActiveDocument;
            }

            //if (InventorSettings.KeyContext == null)
            if (ReferenceManager.KeyContext == null)
            {
                //InventorSettings.KeyContext = InventorSettings.ActiveAssemblyDoc.ReferenceKeyManager.CreateKeyContext();
                ReferenceManager.KeyContext = PersistenceManager.ActiveAssemblyDoc.ReferenceKeyManager.CreateKeyContext();
            }
            
        }

    }




    public abstract class InventorTransactionNodeWithOneOutput : InventorTransactionNode
    {
        //public override void Evaluate(FSharpList<Value> args, Dictionary<PortData, Value> outPuts)
        //{
        //    outPuts[OutPortData[0]] = Evaluate(args);
        //}

        //public abstract Value Evaluate(FSharpList<Value> args);
    }
}
