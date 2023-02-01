using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yu3zx.WorkFlowCore.Items
{
    /// <summary>
    /// 单向结点
    /// </summary>
    public class OneWayItem : ItemBase, IOperate
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public OneWayItem()
        {
            ItemType = ItemType.OneWay;
            ItemName = "单向结点";
            ItemId = WorkFlowUtil.BuildItemId();
        }

        #region 属性区
        /// <summary>
        /// 下一个结点Id
        /// </summary>
        public string NextItemId
        {
            get;
            set;
        }
        /// <summary>
        /// 上一个结点Id－可能有多个，变动记录
        /// </summary>
        public string PrevItemId
        {
            get;
            set;
        }

        /// <summary>
        /// 当前页面Url
        /// </summary>
        public string CurrentPage
        {
            get;
            set;
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
