using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Xml;
using MVOI = Microsoft.VisualStudio.OLE.Interop;
using Inventor;
using System.Windows.Forms;

using Dynamo.Models;
using Dynamo.Utilities;
using InventorServices.Persistence;

namespace DynamoInventor
{
    class InventorUtilities
    {
        //TODO: Move this method to InventorServices
        public static bool TryBindReferenceKey<T>(byte[] key, out T e)
        //where T :  ComponentOccurrence //how can this be constrained and work all the time
        //It is so convenient to Element as a common base in Revit.
        {
            //if (InventorSettings.KeyManager == null)
            if (ReferenceManager.KeyManager == null)
            {
                //TODO Set these once, elsewhere.
                //InventorSettings.ActiveAssemblyDoc = (AssemblyDocument)InventorSettings.InventorApplication.ActiveDocument;
                PersistenceManager.ActiveAssemblyDoc = (AssemblyDocument)PersistenceManager.InventorApplication.ActiveDocument;
                //InventorSettings.KeyManager = InventorSettings.ActiveAssemblyDoc.ReferenceKeyManager;
                ReferenceManager.KeyManager = PersistenceManager.ActiveAssemblyDoc.ReferenceKeyManager;
            }

            try
            {
                object outType = null;
                int keyContext;
                byte[] keyContextArray = new byte[] { };

                //Eventually will need this to work for both BRep objects and all other entity types.  The
                //BRep objects like faces need a context array to be loaded with the reference key.  Saving 
                //and loading the context for non-BReps like workpoints doesn't work.  Context isn't 'ignored'
                //a new one needs to be created for each binding operation.  

                //if (InventorSettings.KeyContextArray != null)
                //{
                //    //keyContext = InventorSettings.KeyManager.LoadContextFromArray(ref keyContextArray);
                //    keyContext = InventorSettings.KeyManager.LoadContextFromArray(InventorSettings.KeyContextArray);
                //    InventorSettings.KeyContext = keyContext;
                //}
                //else //We are in a new file without bound objects.
                //{
                //    if (InventorSettings.KeyContext == null)
                //    {
                //        keyContext = InventorSettings.ActiveAssemblyDoc.ReferenceKeyManager.CreateKeyContext();
                //        InventorSettings.KeyContext = keyContext;
                //    }

                //    //InventorSettings.ActiveAssemblyDoc.ReferenceKeyManager.SaveContextToArray((int)InventorSettings.KeyContext, ref keyContextArray);
                //    //InventorSettings.KeyContextArray = keyContextArray;
                //}

                //keyContext = InventorSettings.ActiveAssemblyDoc.ReferenceKeyManager.CreateKeyContext();
                keyContext = PersistenceManager.ActiveAssemblyDoc.ReferenceKeyManager.CreateKeyContext();
                //InventorSettings.KeyContext = keyContext;
                ReferenceManager.KeyContext = keyContext;
                //T invObject = (T)InventorSettings.KeyManager.BindKeyToObject(ref key, (int)InventorSettings.KeyContext, out outType);
                T invObject = (T)ReferenceManager.KeyManager.BindKeyToObject(ref key, (int)ReferenceManager.KeyContext, out outType);
                e = invObject;
                return invObject != null;
            }
            catch
            {
                //Can't set e to null because it might not be nullable, using default(T) instead.
                e = default(T);
                return false;
            }
        }

        const string DYNAMO_INVENTOR_BINDING_DIRECTORY = @"Autodes\Dynamo\Inventor\BindingFiles\";

        //The private storage read and write methods are courtesy of this:
        //http://adndevblog.typepad.com/manufacturing/2013/03/save-extra-data-in-inventor-file-3.html
        //Enum flags for STGM
        [Flags]
        public enum STGM : int
        {
            DIRECT = 0x00000000,
            TRANSACTED = 0x00010000,
            SIMPLE = 0x08000000,
            READ = 0x00000000,
            WRITE = 0x00000001,
            READWRITE = 0x00000002,
            SHARE_DENY_NONE = 0x00000040,
            SHARE_DENY_READ = 0x00000030,
            SHARE_DENY_WRITE = 0x00000020,
            SHARE_EXCLUSIVE = 0x00000010,
            PRIORITY = 0x00040000,
            DELETEONRELEASE = 0x04000000,
            NOSCRATCH = 0x00100000,
            CREATE = 0x00001000,
            CONVERT = 0x00020000,
            FAILIFTHERE = 0x00000000,
            NOSNAPSHOT = 0x00200000,
            DIRECT_SWMR = 0x00400000,
        }

        public static bool CreatePrivateStorageAndStream(Document pDoc, string StorageName, string StreamName, string data)
        {
            try
            {
                //Create/get storage. "true" means if create 
                //if it does not exist.
                MVOI.IStorage pStg = (MVOI.IStorage)pDoc.GetPrivateStorage(StorageName, true);
                if (pStg == null)
                {
                    return false;
                }

                //Create stream in the storage
                MVOI.IStream pStream = null;
                pStg.CreateStream(StreamName, (uint)
                   (STGM.DIRECT | STGM.CREATE |
                  STGM.READWRITE | STGM.SHARE_EXCLUSIVE),
                    0, 0, out pStream);

                if (pStream == null)
                {
                    return false;
                }

                byte[] byteVsize = System.BitConverter.GetBytes(data.Length);

                byte[] byteVData = Encoding.Default.GetBytes(data);

                uint dummy;

                //Convert string to byte and store it to the stream
                pStream.Write(byteVsize, (uint)(sizeof(int)), out dummy);
                pStream.Write(byteVData, (uint)(byteVData.Length), out dummy);

                //Save the data          
                pStream.Commit((uint)(MVOI.STGC.STGC_OVERWRITE | MVOI.STGC.STGC_DEFAULT));

                //Don't forget to commit changes also in storage
                pStg.Commit((uint)(MVOI.STGC.STGC_DEFAULT | MVOI.STGC.STGC_OVERWRITE));

                //Force document to be dirty thus
                //the change can be saved when document 
                //is saved.
                pDoc.Dirty = true;
                //pDoc.Save();

                Marshal.ReleaseComObject(pStg);

                return true;
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
        }

        //Read the storge and stream
        public static bool ReadPrivateStorageAndStream(Document pDoc, string StorageName, string StreamName, out string outDataStr)
        {
            outDataStr = "";
            try
            {
                //Get the storage. "false" means do not create 
                //if it does not exist
                MVOI.IStorage pStg = (MVOI.IStorage)pDoc.GetPrivateStorage(StorageName, false);

                if (pStg == null)
                {
                    return false;
                }

                //Open stream to read
                MVOI.IStream pStream = null;
                pStg.OpenStream(StreamName, IntPtr.Zero, (uint)(STGM.DIRECT | STGM.READWRITE | STGM.SHARE_EXCLUSIVE), 0, out pStream);

                if (pStream == null)
                {
                    return false;
                }

                byte[] byteVsize = new byte[16];
                uint intSize = sizeof(int);

                //Read the stream
                uint dummy;
                pStream.Read(byteVsize, (uint)intSize, out dummy);
                int lSize = System.BitConverter.ToInt16(byteVsize, 0);

                byte[] outDataByte = new byte[8192];
                pStream.Read(outDataByte, (uint)lSize, out dummy);

                //Convert byte to string
                outDataStr = Encoding.Default.GetString(outDataByte, 0, lSize);

                Marshal.ReleaseComObject(pStg);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
        }

        private static void SerializeNodeBindings(XmlElement element, SaveContext context)
        {
            if (context != SaveContext.Undo)
            {
                
            }
        }

        public static XmlDocument BindingsXmlGenerator(Dynamo.Models.WorkspaceModel currentModel)
        {
            try
            {
                //create the xml document
                var xmlDoc = new XmlDocument();
                xmlDoc.CreateXmlDeclaration("1.0", null, null);
                var root = xmlDoc.CreateElement("Workspace"); //write the root element
                root.SetAttribute("Description", currentModel.Description);
                root.SetAttribute("Category", currentModel.Category);
                root.SetAttribute("Name", currentModel.Name);

                xmlDoc.AppendChild(root);

                var elementList = xmlDoc.CreateElement("Elements");
                //write the root element
                root.AppendChild(elementList);

                foreach (var el in currentModel.Nodes)
                {
                    //Try to cast the nodes to InventorTransactionNode
                    try
                    {
                        var elType = el.GetType();
                        var invNodeType = typeof(InventorTransactionNode);
                        
                        //If the node has inherited from InventorTransactionNode, check if it is bound to objects.
                        if (elType.IsSubclassOf(invNodeType))
                        {
                            InventorTransactionNode thisNode = (InventorTransactionNode)el;
                            if (thisNode != null)
                            {
                                var typeName = thisNode.GetType().ToString();

                                var dynEl = xmlDoc.CreateElement(typeName);
                                elementList.AppendChild(dynEl);

                                //set the type attribute
                                dynEl.SetAttribute("type", thisNode.GetType().ToString());
                                dynEl.SetAttribute("guid", thisNode.GUID.ToString());
                                var objectsKeysList = xmlDoc.CreateElement("objects");
                                dynEl.AppendChild(objectsKeysList);
                                foreach (var key in thisNode.ComponentOccurrenceKeys)
                                {
                                    var objectKey = xmlDoc.CreateElement("object");
                                    objectsKeysList.AppendChild(objectKey);
                                    string keyString = Convert.ToBase64String(key);
                                    objectKey.SetAttribute("key", keyString);
                                }
                            
                                thisNode.Save(xmlDoc, dynEl, SaveContext.File);
                            } 
                        }     
                    }

                    catch (Exception)
                    {                        
                        throw;
                    }                 
                }

                return xmlDoc;
            }

            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message + " : " + ex.StackTrace);
                return null;
            }
        }

        internal string GetBindingFilesPath()
        {
            string log_dir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            log_dir = System.IO.Path.Combine(log_dir, DYNAMO_INVENTOR_BINDING_DIRECTORY);
            return log_dir;
        }
    }
}
