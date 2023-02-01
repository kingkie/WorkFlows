using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Yu3zx.WorkFlowCore
{
    /// <summary>
    /// 结点类型
    /// </summary>
    public enum ItemType
    {
        /// <summary>
        /// 无操作结点
        /// </summary>
        None = 0,
        /// <summary>
        /// 流程开始结点
        /// </summary>
        Start = 1,
        /// <summary>
        /// 流程结束结点
        /// </summary>
        End = 2,
        /// <summary>
        /// 流程页面操作结点
        /// </summary>
        Operation = 3,
        /// <summary>
        /// 流程判断结点
        /// </summary>
        Judge = 4,
        /// <summary>
        /// 流程判断汇合结点
        /// </summary>
        JudgeCombine = 5,
        /// <summary>
        /// 暂停动作
        /// </summary>
        Sleeping = 6,
        /// <summary>
        /// 无页面节点
        /// </summary>
        NoPage = 6,
        /// <summary>
        /// 多分支判断结点
        /// </summary>
        Switch = 8,
        /// <summary>
        /// 单向结点，返回上一步就是结束
        /// </summary>
        OneWay = 9,
    }

    /// <summary>
    /// 流程类型
    /// </summary>
    public enum FlowType
    {
        /// <summary>
        /// 未知，未设置
        /// </summary>
        None,
        /// <summary>
        /// 团队会议
        /// </summary>
        TeamMeeting,
        /// <summary>
        /// 预订
        /// </summary>
        Reserve,
        /// <summary>
        /// 无预订
        /// </summary>
        UnReserve,
        /// <summary>
        /// 退房
        /// </summary>
        CheckOut,
        /// <summary>
        /// 续住
        /// </summary>
        Continued,
        /// <summary>
        /// 访客
        /// </summary>
        Visitor,
        /// <summary>
        /// 补充登记
        /// </summary>
        Supplement,
        /// <summary>
        /// 快速通道
        /// </summary>
        FastTrack,
        /// <summary>
        /// 我要入住,包括预定和无预定
        /// </summary>
        CheckIn,
        /// <summary>
        /// 随易住专用
        /// </summary>
        Syzzy,
    }

    public class WorkFlowUtil
    {
        public static ILog Logger { get; }

        private static Random rd = new Random();
        /// <summary>
        /// 生成一个随机Id
        /// </summary>
        /// <returns></returns>
        public static string BuildItemId()
        {
            return "Item_" + DateTime.Now.ToString("MMddHHmmssfff") + rd.Next(10000, 99999).ToString();
        }

        /// <summary>
        /// 生成一个流程随机Id
        /// </summary>
        /// <returns></returns>
        public static string BuildFlowId()
        {
            return "Flow_" + DateTime.Now.ToString("MMddHHmmssfff") + rd.Next(10000, 99999).ToString();
        }

        /// <summary>
        /// 生成一个流程随机Id
        /// </summary>
        /// <returns></returns>
        public static string BuildFlowId(string _Profix)
        {
            return _Profix + DateTime.Now.ToString("MMddHHmmssfff") + rd.Next(10000, 99999).ToString();
        }

        static WorkFlowUtil()
        {
            log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo("Config/log4net.config"));
            Logger = LogManager.GetLogger("Logger");
        }
    }

    /// <summary>
    /// 简单对象间的复制(效率较高)
    /// </summary>
    /// <typeparam name="TIn"></typeparam>
    /// <typeparam name="TOut"></typeparam>
    public static class AutoMapper<TIn, TOut>
    {
        private static readonly Func<TIn, TOut> cache = GetFunc();

        private static Func<TIn, TOut> GetFunc()
        {
            ParameterExpression parameterExpression = Expression.Parameter(typeof(TIn), "p");
            List<MemberBinding> memberBindingList = new List<MemberBinding>();

            foreach (var item in typeof(TOut).GetProperties())
            {
                if (!item.CanWrite)
                {
                    continue;
                }
                if (item.PropertyType.IsClass && item.PropertyType != typeof(string))
                {
                    continue;
                }

                MemberExpression property = Expression.Property(parameterExpression, typeof(TIn).GetProperty(item.Name));
                MemberBinding memberBinding = Expression.Bind(item, property);
                memberBindingList.Add(memberBinding);
            }

            MemberInitExpression memberInitExpression = Expression.MemberInit(Expression.New(typeof(TOut)), memberBindingList.ToArray());
            Expression<Func<TIn, TOut>> lambda = Expression.Lambda<Func<TIn, TOut>>(memberInitExpression, new ParameterExpression[] { parameterExpression });

            return lambda.Compile();
        }

        public static TOut ConvertFrom(TIn fromObj) => cache(fromObj);
    }
}
