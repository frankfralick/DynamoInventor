using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Inventor;

using Dynamo;
using Dynamo.FSchemeInterop;
using Dynamo.Interfaces;
using Dynamo.Python;
using Dynamo.UpdateManager;
using InventorServices;
using InventorServices.Persistence;
using InventorLibrary.ModulePlacement;

namespace Dynamo
{
    public class DynamoController_Inventor : DynamoController
    {
        public DynamoController_Inventor(string context, IUpdateManager updateManager)
            : base(
                context,
                updateManager,
                new DefaultWatchHandler(),
                Dynamo.PreferenceSettings.Load())
        {
            EngineController.ImportLibrary("InventorLibrary.dll");

            InitializeIoCContainer();

            SetUpPythonNodeScope();
            
        }

        private static void InitializeIoCContainer()
        {
            //Create and configure IoC container for InventorServices
            PersistenceManager.LetThereBeIoC();

            //Create IoC container for the ModulePlacement portion of the library.
            ModuleIoC.LetThereBeIoC();

            //The compiler can't know about any registration/dependency graph errors.  The container's Verify method 
            //lets SimpleInjector build all of these registrations so the application will fail at startup if we have 
            //made certain mistakes.
            PersistenceManager.IoC.Verify();
        }

        private static void SetUpPythonNodeScope()
        {
            IronPythonCompletionProvider.RegisterPythonStatementsInScope("clr.AddReference('Autodesk.Inventor.interop')\nfrom Inventor import *");
            //TODO: Why isn't using the environment variable working!! Fix this!!
            string apiPath = System.Environment.GetEnvironmentVariable("INVENTORAPI");
            string hardApiPath = (@"C:\Windows\Microsoft.NET\assembly\GAC_MSIL\Autodesk.Inventor.Interop\v4.0_18.0.0.0__d84147f8b4276564\Autodesk.Inventor.interop.dll");
            IronPythonCompletionProvider.RegisterScopeVariable(new Tuple<string, object, Type, string>("app",
                                                                                               PersistenceManager.InventorApplication,
                                                                                               typeof(Inventor.Application),
                                                                                               hardApiPath));
        }
    }
}
