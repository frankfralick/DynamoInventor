using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Dynamo.Models;

namespace DynamoInventor.Models
{
    public class InventorDynamoModel : DynamoModel
    {
        private InventorDynamoModel(StartConfiguration configuration) :
            base(configuration)
        {
        }
    }
}
