namespace Yu3zx.WorkFlowCore
{
    public class JudgeItem : ItemBase, IOperate
    {
        public JudgeItem()
        {
            ItemType = ItemType.Judge;
            ItemName = "判断结点";
            ItemId = WorkFlowUtil.BuildItemId();
        }

        #region 属性区
        /// <summary>
        /// 上一个结点Id;开始结点只有下一个结点Id
        /// </summary>
        public string PrevItemId
        {
            set; get;
        }

        /// <summary>
        ///True时下一个结点Id
        /// </summary>
        public string TrueNextItemId
        {
            set; get;
        }
        /// <summary>
        /// False时下一个结点Id
        /// </summary>
        public string FalseNextItemId
        {
            set; get;
        }
        /// <summary>
        /// 判断绑定的ID
        /// </summary>
        public string BindItems
        {
            set; get;
        }

        #endregion End

        public bool DoWork()
        {
            if (!string.IsNullOrEmpty(BindItems))
            {
                //string[] itemids = BindItems.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                object value = TagNodesManager.CreateInstance().GetNodeValue(BindItems);
                if (value != null && (value.ToString() == "1" || value.ToString().ToLower() == "true"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
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
}
