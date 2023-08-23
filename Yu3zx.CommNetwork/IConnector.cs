using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yu3zx.CommNetwork
{
    interface IConnector
    {
        //bool Connected { get; }

        bool Open();

        bool Close();
    }
}
