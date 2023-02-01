using Yu3zx.WorkFlowCore.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yu3zx.WorkFlowCore
{
    public delegate void FlowItemWork(object sender);
    /// <summary>
    /// 工作流执行引擎
    /// </summary>
    public class WorkFlowEngine
    {
        private static WorkFlowEngine instance = null;
        private static object Singleton_Lock = new object(); //锁同步
        public static WorkFlowEngine CreateInstance()
        {
            lock (Singleton_Lock)
            {
                if (instance == null)
                {
                    if (instance == null)
                    {
                        instance = new WorkFlowEngine();
                    }
                }
            }
            return instance;
        }

        public event FlowItemWork OnFlowItemWork; //事件的声明

        private string prePages = string.Empty;

        private WorkFlow _CurrentFlow = null;
        private ItemBase _CurrentItem = null;
        private string strHomePage = string.Empty;

        /// <summary>
        /// 首页地址
        /// </summary>
        public string HomePage { get; set; }

        /// <summary>
        /// 当前执行流程
        /// </summary>
        public WorkFlow CurrentWorkFlow
        {
            get { return _CurrentFlow; }
            set { _CurrentFlow = value; }
        }
        /// <summary>
        /// 当前流程类型
        /// </summary>
        public FlowType CurrentFlowType
        {
            get
            {
                if (CurrentWorkFlow != null)
                {
                    return CurrentWorkFlow.FlowType;
                }
                else
                {
                    return FlowType.None;
                }
            }
        }
        /// <summary>
        /// 当前执行结点
        /// </summary>
        public ItemBase CurrentNode
        {
            get { return _CurrentItem; }
            set { _CurrentItem = value; }
        }
        /// <summary>
        /// 前面一个页面
        /// </summary>
        public string PrePageName
        {
            get { return prePages; }
            set { prePages = value; }
        }

        /// <summary>
        /// 是否有流程在执行中
        /// </summary>
        public bool IsDoing
        {
            get;
            protected set;
        }

        /// <summary>
        /// 流程下一个结点
        /// </summary>
        /// <param name="item">当前结点</param>
        /// <param name="currentPage">当前页面</param>
        /// <returns>操作结果</returns>
        public bool DoFlowNext(ItemBase item, string currentPage = "")
        {
            WorkFlowUtil.Logger.Debug("工作流 DoFlowNext:" + item.ItemId + item.ItemName);
            if (item == null)
            {
                return false;
            }

            CurrentNode = item;
            switch (item.ItemType)
            {
                case ItemType.Start:
                    //if (IsDoing)
                    //{
                    //    return false;//
                    //}
                    if (string.IsNullOrEmpty(currentPage))
                    {
                        strHomePage = string.Empty;
                    }
                    else
                    {
                        if (strHomePage == currentPage)
                        {
                            return false;
                        }
                        strHomePage = currentPage;
                    }

                    IsDoing = true;
                    StartItem startItem = item as StartItem;
                    ItemBase iNext = _CurrentFlow.FindItem(startItem.NextItemId);
                    SetPrevItemId(item, iNext);
                    DoFlowNext(iNext);
                    break;
                case ItemType.End:
                    IsDoing = false;
                    CurrentNode = null;
                    CurrentWorkFlow = null;
                    EndItem endItem = item as EndItem;
                    if (!string.IsNullOrEmpty(endItem.ReturnPage))
                    {
                        OnFlowItemWork?.Invoke(endItem.ReturnPage);
                    }
                    break;
                case ItemType.Judge:
                    JudgeItem jI = item as JudgeItem;
                    string strNextId = string.Empty;
                    if (jI.DoWork()) //判断
                    {
                        strNextId = jI.TrueNextItemId;
                    }
                    else
                    {
                        strNextId = jI.FalseNextItemId;
                    }
                    ItemBase iDoNext = _CurrentFlow.FindItem(strNextId);
                    SetPrevItemId(item, iDoNext);
                    DoFlowNext(iDoNext);
                    break;
                case ItemType.Switch:
                    SwitchItem sI = item as SwitchItem;
                    string nextId = sI.GetNextItem();
                    ItemBase switchDoNext = _CurrentFlow.FindItem(nextId);
                    SetPrevItemId(item, switchDoNext);
                    DoFlowNext(switchDoNext);
                    break;
                case ItemType.Operation:
                    ProcessItem pItem = item as ProcessItem;
                    if (!string.IsNullOrEmpty(pItem.CurrentPage))
                    {
                        OnFlowItemWork?.Invoke(pItem.CurrentPage);
                    }
                    break;
                case ItemType.NoPage:
                    ProcessItem nopageItem = item as ProcessItem;
                    if (!string.IsNullOrEmpty(nopageItem.CurrentPage))
                    {
                        Type type = Type.GetType(nopageItem.CurrentPage);
                        IOperate obj = Activator.CreateInstance(type, true) as IOperate;

                        var r = obj.DoWork();
                        if (r)
                        {
                            SubMit();
                        }
                        else
                        {
                            BackDoWork();
                        }
                    }
                    break;
                case ItemType.OneWay:
                    OneWayItem ow = item as OneWayItem;
                    string strDirectId = ow.NextItemId;
                    ItemBase iOneWayDoNext = _CurrentFlow.FindItem(strDirectId); //获取下一个结点
                    SetPrevItemId(item, iOneWayDoNext); //设置当前结点上一步
                    DoFlowNext(iOneWayDoNext);
                    break;
            }
            if (item.ItemType != ItemType.Start)
            {
                strHomePage = string.Empty;
            }
            return true;
        }

        private void SetPrevItemId(ItemBase crntItem, ItemBase nextItem)
        {
            switch (nextItem.ItemType)
            {
                case ItemType.Judge:
                    (nextItem as JudgeItem).PrevItemId = crntItem.ItemId;
                    break;
                case ItemType.Switch:
                    (nextItem as SwitchItem).PrevItemId = crntItem.ItemId;
                    break;
                case ItemType.Operation:
                    (nextItem as ProcessItem).PrevItemId = crntItem.ItemId;
                    break;
                case ItemType.NoPage:
                    (nextItem as ProcessItem).PrevItemId = crntItem.ItemId;
                    break;
                case ItemType.End:
                    (nextItem as EndItem).PrevItemId = crntItem.ItemId;
                    break;
                case ItemType.OneWay:
                    (nextItem as OneWayItem).PrevItemId = crntItem.ItemId;
                    break;
            }
        }
        /// <summary>
        /// 强制结束流程，用于超时或者直接返回主界面
        /// </summary>
        /// <returns>强制结束</returns>
        public bool ForceFinishFlow()
        {
            IsDoing = false;
            CurrentNode = null;
            CurrentWorkFlow = null;
            //增加首页配置，自动转到首页
            OnFlowItemWork?.Invoke(HomePage);
            prePages = string.Empty;
            return true;
        }

        /// <summary>
        /// 页面提交触发下一个流程结点,并设置PrevItemId属性
        /// </summary>
        /// <param name="pagename">页面名称</param>
        public void SubMit(string pagename = "")
        {
            try
            {
                if (!string.IsNullOrEmpty(prePages))
                {
                    if (prePages == pagename)
                    {
                        WorkFlowUtil.Logger.Debug("工作流重复提交:" + prePages);
                        return;
                    }
                }
                if (CurrentNode == null)
                {
                    WorkFlowUtil.Logger.Debug("工作流提交失败，CurrentNode为空");
                    return;
                }
                prePages = pagename;

                WorkFlowUtil.Logger.Debug("工作流 submit:" + CurrentNode?.ItemName);
                ProcessItem processItem = WorkFlowEngine.CreateInstance().CurrentNode as ProcessItem;
                ItemBase iNext = WorkFlowEngine.CreateInstance().CurrentWorkFlow.FindItem(processItem.NextItemId);
                switch (iNext.ItemType)
                {
                    case ItemType.Judge:
                        (iNext as JudgeItem).PrevItemId = processItem.ItemId;
                        break;
                    case ItemType.Switch:
                        (iNext as SwitchItem).PrevItemId = processItem.ItemId;
                        break;
                    case ItemType.Operation:
                        (iNext as ProcessItem).PrevItemId = processItem.ItemId;
                        break;
                    case ItemType.NoPage:
                        (iNext as ProcessItem).PrevItemId = processItem.ItemId;
                        break;
                    case ItemType.OneWay:
                        (iNext as OneWayItem).PrevItemId = processItem.ItemId;
                        break;
                }
                WorkFlowEngine.CreateInstance().DoFlowNext(iNext);
            }
            catch (Exception ex)
            {
                WorkFlowUtil.Logger.Error("工作流提交错误:" + ex.Message, ex);
            }
        }

        /// <summary>
        /// 后退
        /// </summary>
        public void BackDoWork()
        {
            if (CurrentNode != null)
            {
                prePages = string.Empty;
                switch (CurrentNode.ItemType)
                {
                    case ItemType.Judge:
                        ItemBase iPrevJ = WorkFlowEngine.CreateInstance().CurrentWorkFlow.FindItem((CurrentNode as JudgeItem).PrevItemId);
                        if (iPrevJ != null)
                        {
                            if (iPrevJ.ItemType == ItemType.Operation)
                            {
                                DoFlowNext(iPrevJ);
                            }
                            else
                            {
                                CurrentNode = iPrevJ;
                                BackDoWork();
                            }
                        }
                        break;
                    case ItemType.Switch:
                        ItemBase iPrevJ2 = WorkFlowEngine.CreateInstance().CurrentWorkFlow.FindItem((CurrentNode as SwitchItem).PrevItemId);
                        if (iPrevJ2 != null)
                        {
                            if (iPrevJ2.ItemType == ItemType.Operation)
                            {
                                DoFlowNext(iPrevJ2);
                            }
                            else
                            {
                                CurrentNode = iPrevJ2;
                                BackDoWork();
                            }
                        }
                        break;
                    case ItemType.Operation:
                    case ItemType.NoPage:
                        ItemBase iPrevO = WorkFlowEngine.CreateInstance().CurrentWorkFlow.FindItem((CurrentNode as ProcessItem).PrevItemId);
                        if (iPrevO != null)
                        {
                            if (iPrevO.ItemType == ItemType.Operation)
                            {
                                DoFlowNext(iPrevO);
                            }
                            else
                            {
                                CurrentNode = iPrevO;
                                BackDoWork();
                            }
                        }
                        break;
                    case ItemType.OneWay:
                        ForceFinishFlow();
                        break;
                    case ItemType.Start:
                        ForceFinishFlow(); //直接结束流程：还是跑到最后流程中去？
                        break;
                    default:
                        ForceFinishFlow(); //直接结束流程：还是跑到最后流程中去？
                        break;
                }
            }
            else
            {
                //没有当前流程结点，则是流程结束了或者未初始化
                IsDoing = false;
            }
        }
        /// <summary>
        /// 用于人证等返回到当前父页面
        /// </summary>
        public void BackToParent()
        {
            if (CurrentNode != null)
            {
                prePages = string.Empty;
                switch (CurrentNode.ItemType)
                {
                    case ItemType.Judge:
                        break;
                    case ItemType.Switch:
                        break;
                    case ItemType.NoPage:
                        break;
                    case ItemType.Operation:
                        ProcessItem pItem = CurrentNode as ProcessItem;
                        if (!string.IsNullOrEmpty(pItem.CurrentPage))
                        {
                            OnFlowItemWork?.Invoke(pItem.CurrentPage);
                        }
                        break;
                    default:
                        ForceFinishFlow(); //直接结束流程：还是跑到最后流程中去？
                        break;
                }
            }
            else
            {
                //没有当前流程结点，则是流程结束了或者未初始化
            }
        }
    }
}
