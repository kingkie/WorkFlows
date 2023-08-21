using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Yu3zx.TimeCrontab
{
    static class TestClass
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            var crontab = Crontab.Parse("* * * * *");
            var nextOccurrence = crontab.GetNextOccurrence(DateTime.Now);
            Console.WriteLine("Next Scend:" + nextOccurrence.ToString("yyyy-MM-dd HH:mm:ss.fff"));

            var crontabYear = Crontab.Parse("* * * * * *", CronStringFormat.WithYears);
            var nextOccurrenceYear = crontabYear.GetNextOccurrence(DateTime.Now);
            nextOccurrenceYear = crontabYear.GetNextOccurrence(nextOccurrenceYear);
            Console.WriteLine("Next Year:" + nextOccurrenceYear.ToString("yyyy-MM-dd HH:mm:ss.fff"));

            var crontabScendYear = Crontab.Parse("* * * * * * *", CronStringFormat.WithSecondsAndYears);
            var nextOccurrenceScendYear = crontabScendYear.GetNextOccurrence(DateTime.Now);
            nextOccurrenceScendYear = crontabScendYear.GetNextOccurrence(nextOccurrenceScendYear);
            Console.WriteLine("Next Scend And Year:" + nextOccurrenceScendYear.ToString("yyyy-MM-dd HH:mm:ss.fff"));

            Console.WriteLine("开始---------------1" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));

            Block();

            Console.WriteLine("开始---------------2" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));

            UnBlock();

            Console.WriteLine("结束----------------" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));

            Console.ReadKey();
        }
        /// <summary>
        /// 
        /// </summary>
        private static void Block()
        {
            int iTest = 10;
            var crontab = Crontab.Parse("* * * * * *", CronStringFormat.WithSeconds);
            while (true)
            {
                Thread.Sleep((int)crontab.GetSleepMilliseconds(DateTime.Now));
                Console.WriteLine(DateTime.Now.ToString("G"));
                if (iTest < 1)
                    break;
                iTest--;
            }
        }

        private static void UnBlock()
        {
            int iTest = 10;
            var crontab = Crontab.Parse("* * * * * *", CronStringFormat.WithSeconds);
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    await Task.Delay((int)crontab.GetSleepMilliseconds(DateTime.Now));
                    Console.WriteLine(DateTime.Now.ToString("G"));

                    if (iTest < 1)
                        break;
                    iTest--;
                }
            }, TaskCreationOptions.LongRunning);
        }

        private static void MacroFlag()
        {
            // 静态属性
            var secondly = Crontab.Secondly;    // 每秒
            var minutely = Crontab.Minutely;    // 每分钟
            var hourly = Crontab.Hourly;    // 每小时
            var daily = Crontab.Daily;  // 每天 00:00:00
            var monthly = Crontab.Monthly;  // 每月 1 号 00:00:00
            var weekly = Crontab.Weekly;    // 每周日 00：00：00
            var yearly = Crontab.Yearly;    // 每年 1 月 1 号 00:00:00
            var workday = Crontab.Workday;    // 每周一至周五 00:00:00

            // 每第 3，5，6 秒
            var crontabintervals = Crontab.SecondlyAt(3, 5, 6);

            // 每分钟第 3 秒
            var crontabPm = Crontab.MinutelyAt(3);
            // 每分钟第 3，5，6 秒
            var crontabPms = Crontab.MinutelyAt(3, 5, 6);

            // 每小时第 3 分钟
            var crontabPh = Crontab.HourlyAt(3);
            // 每小时第 3，5，6 分钟
            var crontabPhs = Crontab.HourlyAt(3, 5, 6);

            // 每天第 3 小时正（点）
            var crontabPd = Crontab.DailyAt(3);
            // 每天第 3，5，6 小时正（点）
            var crontabPds = Crontab.DailyAt(3, 5, 6);

            // 每月第 3 天零点正
            var crontabPmd = Crontab.MonthlyAt(3);
            // 每月第 3，5，6 天零点正
            var crontabPmds = Crontab.MonthlyAt(3, 5, 6);

            // 每周星期 3 零点正
            var crontabPw = Crontab.WeeklyAt(3);
            var crontabPwstr = Crontab.WeeklyAt("WED");  // SUN（星期天），MON，TUE，WED，THU，FRI，SAT
                                                    // 每周星期 3，5，6 零点正
            var crontabPws = Crontab.WeeklyAt(3, 5, 6);
            var crontabPwsstr = Crontab.WeeklyAt("WED", "FRI", "SAT");
            // 还支持混合
            var crontabMix = Crontab.WeeklyAt(3, "FRI", 6);

            // 每年第 3 月 1 日零点正
            var crontabPym = Crontab.YearlyAt(3);
            var crontabPymstr = Crontab.YearlyAt("MAR");  // JAN（一月），FEB，MAR，APR，MAY，JUN，JUL，AUG，SEP，OCT，NOV，DEC
                                                    // 每年第 3，5，6 月 1 日零点正
            var crontabPyms = Crontab.YearlyAt(3, 5, 6);
            var crontabPymsstr = Crontab.YearlyAt("MAR", "MAY", "JUN");
            // 还支持混合
            var crontabMixy = Crontab.YearlyAt(3, "MAY", 6);
        }


    }
}
