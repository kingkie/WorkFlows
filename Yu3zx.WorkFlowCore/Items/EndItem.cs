using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yu3zx.WorkFlowCore.Items
{
    public class EndItem : ItemBase, IOperate
    {
        public EndItem()
        {
            ItemType = ItemType.End;
            ItemName = "结束";
            ItemId = WorkFlowUtil.BuildItemId();
        }

        #region 属性区
        /// <summary>
        /// 前个结点Id，记录上一个操作结点，用作返回上一步用
        /// </summary>
        public string PrevItemId
        {
            set; get;
        }
        /// <summary>
        /// 结束后返回的页面
        /// </summary>
        public string ReturnPage
        {
            set;get;
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
