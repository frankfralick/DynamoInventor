using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using Inventor;

using Dynamo.Controls;
using Dynamo.Core;
using Dynamo.Core.Threading;
using Dynamo.Models;
using Dynamo.Services;
using Dynamo.Utilities;
using Dynamo.ViewModels;

using DynamoUnits;

using DynamoUtilities;
using InventorServices.Persistence;
using DynamoInventor.Models;

namespace DynamoInventor
{
    internal class DynamoInventorAddinButton : Button
    {
        ////public static ExecutionEnvironment env;
        ////private static DynamoView dynamoView;
        ////private DynamoController dynamoController;
        private static bool isRunning = false;
        public static double? dynamoViewX = null;
        public static double? dynamoViewY = null;
        public static double? dynamoViewWidth = null;
        public static double? dynamoViewHeight = null;
        private bool handledCrash = false;

		public DynamoInventorAddinButton(string displayName, 
                                         string internalName, 
                                         CommandTypesEnum commandType, 
                                         string clientId, 
                                         string description, 
                                         string tooltip, 
                                         Icon standardIcon, 
                                         Icon largeIcon, 
                                         ButtonDisplayEnum buttonDisplayType)
            : base(displayName, internalName, commandType, clientId, description, tooltip, standardIcon, largeIcon, buttonDisplayType)
		{		
		}

		public DynamoInventorAddinButton(string displayName, 
                                         string internalName, 
                                         CommandTypesEnum commandType, 
                                         string clientId, 
                                         string description, 
                                         string tooltip, 
                                         ButtonDisplayEnum buttonDisplayType)
			: base(displayName, internalName, commandType, clientId, description, tooltip, buttonDisplayType)
		{		
		}

		override protected void ButtonDefinition_OnExecute(NameValueMap context)
		{
			try
			{
                //TODO Refactor Dynamo initialization steps out. 

                if (isRunning == false)
                {
                    //Start Dynamo!  
                    //IntPtr mwHandle = Process.GetCurrentProcess().MainWindowHandle;

                    string inventorContext = "Inventor " + PersistenceManager.InventorApplication.SoftwareVersion.DisplayVersion;

                    AppDomain.CurrentDomain.AssemblyResolve += Analyze.Render.AssemblyHelper.ResolveAssemblies;
                    //Setup base units.  Need to double check what to do.  The ui default for me is inches, but API always must take cm.
                    BaseUnit.AreaUnit = AreaUnit.SquareCentimeter;
                    BaseUnit.LengthUnit = LengthUnit.Centimeter;
                    BaseUnit.VolumeUnit = VolumeUnit.CubicCentimeter;

                    //Setup DocumentManager...this is all taken care of on its own.  Reference to active application will happen
                    //when first call to binder.InventorApplication happens

                    //Create instance of DynamoModel
                    DynamoModel dynamoModel = DynamoModel.Start();
                    DynamoViewModel dynamoViewModel = DynamoViewModel.Start();

                    IntPtr mwHandle = Process.GetCurrentProcess().MainWindowHandle;
                    var dynamoView = new DynamoView(dynamoViewModel);
                    new WindowInteropHelper(dynamoView).Owner = mwHandle;

                    handledCrash = false;
                    dynamoView.Show();
                    isRunning = true;
                }

                else if (isRunning == true)
                {
                    System.Windows.Forms.MessageBox.Show("Dynamo is already running.");
                }

                else
                {
                    System.Windows.Forms.MessageBox.Show("Something terrible happened.");
                }		
			}

			catch(Exception e)
			{
                System.Windows.Forms.MessageBox.Show(e.ToString());
			}
		}

        /// <summary>
        /// Executes after Dynamo closes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //private void dynamoView_Closed(object sender, EventArgs e)
        //{
        //    dynamoView = null;
        //    isRunning = false;
        //}
	}
}
