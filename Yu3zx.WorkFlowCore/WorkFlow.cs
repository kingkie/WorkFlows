using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yu3zx.WorkFlowCore
{
    public class WorkFlow
    {
        public WorkFlow()
        {
            WorkFlowId = WorkFlowUtil.BuildFlowId();
            WorkFlowName = "新流程";
        }
        /// <summary>
        /// 流程Id
        /// </summary>
        public string WorkFlowId
        {
            set;get;
        }
        /// <summary>
        /// 流程名称
        /// </summary>
        public string WorkFlowName
        {
            set;get;
        }

        public FlowType FlowType
        {
            set;get;
        }

        public List<ItemBase> FlowItems = new List<ItemBase>();
        /// <summary>
        /// 根据 Id查找对应结点
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public ItemBase FindItem(string itemId)
        {
            ItemBase item = FlowItems.Find(u => u.ItemId == itemId);
            return item;
        }
        /// <summary>
        /// 开始结点并非存放在第一个结点时
        /// </summary>
        /// <returns></returns>
        public ItemBase FindStartItem()
        {
            ItemBase item = FlowItems.Find(u => u.ItemType == ItemType.Start);
            return item;
        }

        /// <summary>
        /// 默认第一个结点为开始结点
        /// </summary>
        /// <returns></returns>
        public ItemBase GetStartItem()
        {
            if (FlowItems.Count > 0)
            {
                if(FlowItems[0].ItemType == ItemType.Start)
                {
                    return FlowItems[0];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }
}
