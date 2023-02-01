using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yu3zx.WorkFlowCore
{
    public class TagNode
    {
        /// <summary>
        /// Tag结点Id
        /// </summary>
        public string NodeId
        {
            set;get;
        }

        /// <summary>
        /// Tag结点名称
        /// </summary>
        public string NodeName
        {
            set;get;
        }

        [JsonIgnore]
        public object NodeValue
        {
            set;
            get;
        }
    }
}
