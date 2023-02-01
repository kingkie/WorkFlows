using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Yu3zx.WorkFlowCore.Items
{
    public class SleepItem : ItemBase, IOperate
    {
        public SleepItem()
        {
            ItemType = ItemType.Sleeping;
            ItemName = "暂停结点";
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
        /// 停顿时间长度(500ms * KeepTimes)
        /// </summary>
        public int KeepTimes
        {
            set;
            get;
        } = 1;

        #endregion End

        public bool DoWork()
        {
            Thread.Sleep(500 * KeepTimes);
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
