using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yu3zx.WorkFlowCore.Items
{
    public class StartItem : ItemBase, IOperate
    {
        public StartItem()
        {
            ItemType = ItemType.Start;
            ItemName = "开始";
            ItemId = WorkFlowUtil.BuildItemId();
        }

        #region 属性区
        /// <summary>
        /// 下一个结点Id;开始结点只有下一个结点Id
        /// </summary>
        public string NextItemId
        {
            set; get;
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
