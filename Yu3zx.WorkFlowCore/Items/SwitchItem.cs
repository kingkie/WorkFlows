using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yu3zx.WorkFlowCore.Items
{
    public class SwitchItem : ItemBase, IOperate
    {
        public SwitchItem()
        {
            ItemType = ItemType.Switch;
            ItemName = "多分支选择判断结点";
            ItemId = WorkFlowUtil.BuildItemId();
        }

        #region 属性区
        /// <summary>
        /// 上一个结点Id;开始结点只有下一个结点Id
        /// </summary>
        public string PrevItemId { get; set; }

        /// <summary>
        /// 下一个结点Id，选择时，如果没有匹配结点选择默认结点
        /// </summary>
        public string NextItemId
        {
            set; get;
        }

        /// <summary>
        ///True时下一个结点Id
        /// </summary>
        public List<CaseItem> CaseItems { get; set; }

        /// <summary>
        /// 判断绑定的ID
        /// </summary>
        public string BindItems { get; set; }

        #endregion End

        public bool DoWork()
        {
            return true;
        }

        public string GetNextItem()
        {
            var value = TagNodesManager.CreateInstance().GetNodeValue(BindItems);
            if (value == null)
            {
                throw new Exception(string.Format("绑定的变量【{0}】不能为null", BindItems));
            }
            string strValue = value.ToString(); //不管是数字还是字符串，先转换成字符串
            //string type1 = value.GetType().ToString();

            foreach (var c in CaseItems)
            {
                if (string.Equals(strValue, c.Case))
                {
                    return c.Then;
                }
            }
            if(string.IsNullOrEmpty(NextItemId))
            {
                throw new Exception(string.Format("节点配置错误，没有满足条件【{0}】的分支", value));
            }
            else
            {
                return NextItemId;
            }
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

    public class CaseItem
    {
        public string Case { get; set; }
        public string Then { get; set; }
    }
}
