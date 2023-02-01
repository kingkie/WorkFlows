using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yu3zx.WorkFlowCore.Items
{
    public class ProcessItem : ItemBase, IOperate
    {
        public ProcessItem()
        {
            ItemType = ItemType.Operation;
            ItemName = "操作结点";
            ItemId = WorkFlowUtil.BuildItemId();
        }

        #region 属性区
        /// <summary>
        /// 下一个结点Id
        /// </summary>
        public string NextItemId
        {
            set; get;
        }
        /// <summary>
        /// 上一个结点Id－可能有多个，变动记录
        /// </summary>
        public string PrevItemId
        {
            set; get;
        }

        /// <summary>
        /// 当前页面Url
        /// </summary>
        public string CurrentPage
        {
            set;
            get;
        }

        #endregion End

        public bool DoWork()
        {
            return true;
        }

        public override bool Init()
        {
            return true;
        }

        public override bool UnInit()
        {
            return true;
        }

    }
}
