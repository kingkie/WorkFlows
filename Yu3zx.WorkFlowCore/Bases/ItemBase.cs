using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yu3zx.WorkFlowCore
{
    public abstract class ItemBase
    {
        /// <summary>
        /// 结点Id
        /// </summary>
        public string ItemId
        {
            set; get;
        }
        /// <summary>
        /// 结点名称
        /// </summary>
        public string ItemName
        {
            set; get;
        }

        /// <summary>
        /// 结点类型,默认为无操作结点
        /// </summary>
        public ItemType ItemType
        {
            set;
            get;
        } = ItemType.None;

        /// <summary>
        /// 结点传参
        /// </summary>
        [JsonIgnore]
        public string Para
        {
            set; get;
        }

        public abstract bool Init();

        public abstract bool UnInit();
    }
}
