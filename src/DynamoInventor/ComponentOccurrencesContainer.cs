using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DynamoInventor
{
    /// <summary>
    /// This class will hold references to all the component occurrences
    /// in an AssemblyDocument.  This could be a generic class, it will essentially
    /// be the same as ElementsContainer only here we are tracking byte[] instead of
    /// ElementId.  
    /// </summary>
    public class ComponentOccurrencesContainer
    {
        //Not sure about this.  I think we can store the reference key byte array, and when saving a 
        //dynamo definition can call KeyToString, on opening a definition with the original document open
        //can call StringToKey then bind keys back to objects.  If opening a definition in a new assembly,
        //inability to resolve key binding can just be allowed to fail, or notify user 'this is not the original
        //assembly, binding to original assembly will fail if you save, do you want to save a copy'.  Being
        //able to work with Dynamo in an assembly and then come back to it later will be important for routines
        //like placing assemblies that take a while to run, people won't want to start over every time.
        //Alternative if this is too slow might be List<List<ComponentOccurrence>>.  For part files will need 
        //to use reference keys for sure.

        //What would prevent this from being HashSet<byte[]>
        Dictionary<Guid, List<List<byte[]>>> storedElementIds =
            new Dictionary<Guid, List<List<byte[]>>>();

        internal IEnumerable<Guid> Nodes
        {
            get { return storedElementIds.Keys; }
        }

        internal void Clear()
        {
            storedElementIds.Clear();
        }

        public List<List<byte[]>> this[Guid node]
        {
            get
            {
                if (!storedElementIds.ContainsKey(node))
                {
                    storedElementIds[node] = new List<List<byte[]>>()
                    {
                        new List<byte[]>()
                    };
                }
                return storedElementIds[node];
            }
        }

        public void DestroyAll()
        {
            foreach (var e in storedElementIds.Values.SelectMany(x => x.SelectMany(y => y)))
            {
                try
                {
                    //Bind e to object, delete the object.  Need to make key managing thing first.
                }
                catch
                {
                    
                }
            }
            storedElementIds.Clear();
        }

        public bool HasElements(Guid node)
        {
            return storedElementIds.ContainsKey(node);
        }

        //TODO Collection of all top level occurrences

        //TODO Flat collection of all top level occurrences and leaf occurrences
    }
}
