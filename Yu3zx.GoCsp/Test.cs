using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yu3zx.GoCsp
{
    class Program
    {
        static SharedStrand _strand;
        static Channel<long> _chan1;
        static Channel<long> _chan2;
        static Channel<long> _chan3;
        static CspChannel<long, long> _csp;
        /// <summary>
        /// 通道1生产
        /// </summary>
        /// <returns></returns>
        static async Task Producer1()
        {
            while (true)
            {
                await _chan1.Send(SystemTick.GetTickUs());
                await Generator.sleep(300);
            }
        }
        /// <summary>
        /// 2通道消费
        /// </summary>
        /// <returns></returns>
        static async Task Producer2()
        {
            while (true)
            {
                await _chan2.Receive();
                await Generator.sleep(500);
            }
        }
        /// <summary>
        /// 通道3生产
        /// </summary>
        /// <returns></returns>
        static async Task Producer3()
        {
            while (true)
            {
                await _chan3.Send(SystemTick.GetTickUs());
                await Generator.sleep(1000);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static async Task Producer4()
        {
            while (true)
            {
                long res = await _csp.Invoke(SystemTick.GetTickUs());
                Console.WriteLine("csp return {0}", res);
                await Generator.sleep(1000);
            }
        }
        /// <summary>
        /// 1消费；2生产；3消费
        /// </summary>
        /// <returns></returns>
        static async Task Consumer()
        {
            Console.WriteLine("Receive chan1 {0}", await _chan1.Receive());
            Console.WriteLine("Send chan2 {0}", await _chan2.Send(SystemTick.GetTickUs()));
            Console.WriteLine("Receive chan3 {0}", await _chan3.Receive());
            while (true)
            {
                await Generator.select().case_receive(_chan1, async delegate (long msg)
                {
                    Console.WriteLine("select Receive chan1 {0}", msg);
                    await Generator.sleep(100);
                }).case_send(_chan2, SystemTick.GetTickUs(), async delegate () //
                {
                    Console.WriteLine("select Send chan2");
                    await Generator.sleep(100);
                }).case_receive(_chan3, async delegate (long msg)
                {
                    Console.WriteLine("select Receive chan3 {0}", msg);
                    await Generator.sleep(100);
                }).case_receive(_csp, async delegate (long msg)
                {
                    Console.WriteLine("select csp delay {0}", SystemTick.GetTickUs() - msg);
                    await Generator.sleep(100);
                    return SystemTick.GetTickUs();
                }).End();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cons"></param>
        /// <returns></returns>
        static async Task Producer5(Generator cons)
        {
            for (int i = 0; i < 10; i++)
            {
                await cons.send_msg(i);
                await Generator.sleep(1000);
                await cons.send_msg((long)i);
                await Generator.sleep(1000);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static async Task Consumer2()
        {
            await Generator.Receive().case_of(async delegate (int msg)
            {
                Console.WriteLine("                                   Receive int {0}", msg);
                await Generator.sleep(1);
            }).case_of(async delegate (long msg)
            {
                Console.WriteLine("                                   Receive long {0}", msg);
                await Generator.sleep(1);
            }).End();
        }

        static void Main(string[] args)
        {
            WorkService work = new WorkService();
            _strand = new WorkStrand(work);
            _chan1 = Channel<long>.Make(_strand, 3);
            _chan2 = Channel<long>.Make(_strand, 0);
            _chan3 = Channel<long>.Make(_strand, -1);
            _csp = new CspChannel<long, long>(_strand);
            Generator.Go(_strand, Producer1);//生产者
            Generator.Go(_strand, Producer2);
            Generator.Go(_strand, Producer3);
            Generator.Go(_strand, Producer4);
            Generator.Go(_strand, Consumer);//消费者
            Generator.Go(_strand, () => Producer5(Generator.Tgo(_strand, Consumer2)));
            work.Run();
            //-----------测试二-----------
            WorkService work1 = new WorkService();
            strand = new WorkStrand(work1);
            Generator.Go(strand, MainWorker);
            work1.Run();
            Console.ReadKey();
        }

        //-=-=-=-=-=-=-=-测试二=-=-=-=-=-=-=-=
        static SharedStrand strand;

        static void Log(string msg)
        {
            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} {msg}");
        }

        static async Task Worker(string name, int time = 1000)
        {
            await Generator.sleep(time);
            Log(name);
        }

        //1 A、B、C依次串行
        //A->B->C
        static async Task Worker1()
        {
            await Worker("A");
            await Worker("B");
            await Worker("C");
        }

        //2 A、B、C全部并行，且依赖同一个strand(隐含参数，所有依赖同一个strand的任务都是线程安全的)
        //A
        //B
        //C
        static async Task Worker2()
        {
            Generator.children children = new Generator.children();
            children.Go(() => Worker("A"));
            children.Go(() => Worker("B"));
            children.Go(() => Worker("C"));
            await children.wait_all();
        }

        //3 A执行完后，B、C再并行
        //  -->B
        //  |
        //A->
        //  |
        //  -->C
        static async Task Worker3()
        {
            await Worker("A");
            Generator.children children = new Generator.children();
            children.Go(() => Worker("B"));
            children.Go(() => Worker("C"));
            await children.wait_all();
        }

        //4 B、C都并行执行完后，再执行A
        //B--
        //  |
        //  -->A
        //  |
        //C--
        static async Task Worker4()
        {
            Generator.children children = new Generator.children();
            children.Go(() => Worker("B"));
            children.Go(() => Worker("C"));
            await children.wait_all();
            await Worker("A");
        }

        //5 B、C任意一个执行完后，再执行A
        //B--
        //  |
        //  >-->A
        //  |
        //C--
        static async Task Worker5()
        {
            Generator.children children = new Generator.children();
            var B = children.Tgo(() => Worker("B", 1000));
            var C = children.Tgo(() => Worker("C", 2000));
            var task = await children.wait_any();
            if (task == B)
            {
                Log("B成功");
            }
            else
            {
                Log("C成功");
            }
            await Worker("A");
        }

        //6 等待一个特定任务
        static async Task Worker6()
        {
            Generator.children children = new Generator.children();
            var A = children.Tgo(() => Worker("A"));
            var B = children.Tgo(() => Worker("B"));
            await children.Wait(A);
        }

        //7 超时等待一个特定任务，然后中止所有任务
        static async Task Worker7()
        {
            Generator.children children = new Generator.children();
            var A = children.Tgo(() => Worker("A", 1000));
            var B = children.Tgo(() => Worker("B", 2000));
            if (await children.TimedWait(1500, A))
            {
                Log("成功");
            }
            else
            {
                Log("超时");
            }
            await children.Stop();
        }

        //8 超时等待一组任务，然后中止所有任务
        static async Task Worker8()
        {
            Generator.children children = new Generator.children();
            children.Go(() => Worker("A", 1000));
            children.Go(() => Worker("B", 2000));
            var tasks = await children.timed_wait_all(1500);
            await children.Stop();
            Log($"成功{tasks.Count}个");
        }

        //9 超时等待一组任务，然后中止所有任务，且在中止任务中就地善后处理
        static async Task Worker9()
        {
            Generator.children children = new Generator.children();
            children.Go(() => Worker("A", 1000));
            children.Go(async delegate ()
            {
                try
                {
                    await Worker("B", 2000);
                }
                catch (Generator.StopException)
                {
                    Log("B被中止");
                    await Generator.sleep(500);
                    throw;
                }
                catch (System.Exception)
                {
                }
            });
            var task = await children.timed_wait_all(1500);
            await children.Stop();
            Log($"成功{task.Count}个");
        }

        //10 嵌套任务
        static async Task Worker10()
        {
            Generator.children children = new Generator.children();
            children.Go(async delegate ()
            {
                Generator.children children1 = new Generator.children();
                children1.Go(() => Worker("A"));
                children1.Go(() => Worker("B"));
                await children1.wait_all();
            });
            children.Go(async delegate ()
            {
                Generator.children children1 = new Generator.children();
                children1.Go(() => Worker("C"));
                children1.Go(() => Worker("D"));
                await children1.wait_all();
            });
            await children.wait_all();
        }

        //11 嵌套中止
        static async Task Worker11()
        {
            Generator.children children = new Generator.children();
            children.Go(() => Worker("A", 1000));
            children.Go(async delegate ()
            {
                try
                {
                    Generator.children children1 = new Generator.children();
                    children1.Go(async delegate ()
                    {
                        try
                        {
                            await Worker("B", 2000);
                        }
                        catch (Generator.StopException)
                        {
                            Log("B被中止1");
                            await Generator.sleep(500);
                            throw;
                        }
                        catch (System.Exception)
                        {
                        }
                    });
                    await children1.wait_all();
                }
                catch (Generator.StopException)
                {
                    Log("B被中止2");
                    throw;
                }
                catch (System.Exception)
                {
                }
            });
            await Generator.sleep(1500);
            await children.Stop();
        }

        //12 并行执行且等待一组耗时算法
        static async Task Worker12()
        {
            WaitGroup wg = new WaitGroup();
            for (int i = 0; i < 2; i++)
            {
                wg.add();
                int idx = i;
                var _ = Task.Run(delegate ()
                {
                    try
                    {
                        Log($"执行算法{idx}");
                    }
                    finally
                    {
                        wg.done();
                    }
                });
            }
            await wg.Wait();
            Log("执行算法完成");
        }

        //13 串行执行耗时算法，耗时算法必需放在线程池中执行，否则依赖同一个strand的调度将不能及时
        static async Task Worker13()
        {
            for (int i = 0; i < 2; i++)
            {
                await Generator.send_task(() => Log($"执行算法{i}"));
            }
        }

        //14 生产者->消费者
        static async Task Worker14()
        {
            Channel<int> Channel = Channel<int>.Make(-1);//无限缓存，使用默认strand
            Generator.children children = new Generator.children();
            children.Go(async delegate ()
            {
                for (int i = 0; i < 3; i++)
                {
                    await Channel.Send(i);
                    await Generator.sleep(1000);
                }
                Channel.Close();
            });
            children.Go(async delegate ()
            {
                for (int i = 10; i < 13; i++)
                {
                    await Channel.Send(i);
                    await Generator.sleep(1000);
                }
                Channel.Close();
            });
            children.Go(async delegate ()
            {
                while (true)
                {
                    ChannelReceiveWrap<int> res = await Channel.Receive();
                    if (res.state == ChannelState.closed)
                    {
                        Log($"Channel 已关闭");
                        break;
                    }
                    Log($"recv0 {res.msg}");
                }
            });
            children.Go(async delegate ()
            {
                while (true)
                {
                    ChannelReceiveWrap<int> res = await Channel.Receive();
                    if (res.state == ChannelState.closed)
                    {
                        Log($"Channel 已关闭");
                        break;
                    }
                    Log($"recv1 {res.msg}");
                }
            });
            await children.wait_all();
        }

        //15 超时接收
        static async Task Worker15()
        {
            Channel<int> Channel = Channel<int>.Make(1);//缓存1个
            Generator.children children = new Generator.children();
            children.Go(async delegate ()
            {
                for (int i = 0; i < 3; i++)
                {
                    await Channel.Send(i);
                    await Generator.sleep(1000);
                }
            });
            children.Go(async delegate ()
            {
                while (true)
                {
                    ChannelReceiveWrap<int> res = await Channel.TimedReceive(2000);
                    if (res.state == ChannelState.overtime)
                    {
                        Log($"recv 超时");
                        break;
                    }
                    Log($"recv {res.msg}");
                }
            });
            await children.wait_all();
        }

        //16 超时发送
        static async Task Worker16()
        {
            Channel<int> Channel = Channel<int>.Make(0);//无缓存
            Generator.children children = new Generator.children();
            children.Go(async delegate ()
            {
                for (int i = 0; ; i++)
                {
                    ChannelSendWrap res = await Channel.TimedSend(2000, i);
                    if (res.state == ChannelState.overtime)
                    {
                        Log($"Send 超时");
                        break;
                    }
                    await Generator.sleep(1000);
                }
            });
            children.Go(async delegate ()
            {
                for (int i = 0; i < 3; i++)
                {
                    ChannelReceiveWrap<int> res = await Channel.Receive();
                    Log($"recv0 {res.msg}");
                }
            });
            await children.wait_all();
        }

        //17 过程调用
        static async Task Worker17()
        {
            CspChannel<int, TupleEx<int, int>> csp = new CspChannel<int, TupleEx<int, int>>();
            Generator.children children = new Generator.children();
            children.Go(async delegate ()
            {
                for (int i = 0; i < 3; i++)
                {
                    CspInvokeWrap<int> res = await csp.Invoke(TupleEx.Make(i, i));
                    Log($"csp 返回 {res.result}");
                    await Generator.sleep(1000);
                }
                csp.Close();
            });
            children.Go(async delegate ()
            {
                while (true)
                {
                    ChannelState st = await Generator.csp_wait(csp, async delegate (TupleEx<int, int> p)
                    {
                        Log($"recv {p}");
                        await Generator.sleep(1000);
                        return p.value1 + p.value2;
                    });
                    if (st == ChannelState.closed)
                    {
                        Log($"csp 已关闭");
                        break;
                    }
                }
            });
            await children.wait_all();
        }

        //18 一轮选择一个接收
        static async Task Worker18()
        {
            Channel<int> chan1 = Channel<int>.Make(0);
            Channel<int> chan2 = Channel<int>.Make(0);
            Generator.children children = new Generator.children();
            children.Go(async delegate ()
            {
                for (int i = 0; i < 3; i++)
                {
                    await chan1.Send(i);
                    await Generator.sleep(1000);
                }
                chan1.Close();
            });
            children.Go(async delegate ()
            {
                for (int i = 10; i < 13; i++)
                {
                    await chan2.Send(i);
                    await Generator.sleep(1000);
                }
                chan2.Close();
            });
            children.Go(async delegate ()
            {
                await Generator.select().case_receive(chan1, async delegate (int p)
                {
                    await Generator.sleep(100);
                    Log($"recv1 {p}");
                }).case_receive(chan2, async delegate (int p)
                {
                    await Generator.sleep(100);
                    Log($"recv2 {p}");
                }).loop();
                Log($"Channel 已关闭");
            });
            await children.wait_all();
        }

        //19 一轮选择一个发送
        static async Task Worker19()
        {
            Channel<int> chan1 = Channel<int>.Make(0);
            Channel<int> chan2 = Channel<int>.Make(0);
            Generator.children children = new Generator.children();
            children.Go(async delegate ()
            {
                for (int i = 0; i < 3; i++)
                {
                    await Generator.select(true).case_send(chan1, i, async delegate ()
                    {
                        Log($"send1 {i}");
                        await Generator.sleep(1000);
                    }).case_send(chan2, i, async delegate ()
                    {
                        Log($"send2 {i}");
                        await Generator.sleep(1000);
                    }).End();
                }
                chan1.Close();
                chan2.Close();
            });
            children.Go(async delegate ()
            {
                while (true)
                {
                    ChannelReceiveWrap<int> res = await chan1.Receive();
                    if (res.state == ChannelState.closed)
                    {
                        Log($"chan1 已关闭");
                        break;
                    }
                    Log($"recv1 {res.msg}");
                }
            });
            children.Go(async delegate ()
            {
                while (true)
                {
                    ChannelReceiveWrap<int> res = await chan2.Receive();
                    if (res.state == ChannelState.closed)
                    {
                        Log($"chan2 已关闭");
                        break;
                    }
                    Log($"recv2 {res.msg}");
                }
            });
            await children.wait_all();
        }

        static async Task MainWorker()
        {
            Console.WriteLine("         1");
            await Worker1();
            Console.WriteLine("         2");
            await Worker2();
            Console.WriteLine("         3");
            await Worker3();
            Console.WriteLine("         4");
            await Worker4();
            Console.WriteLine("         5");
            await Worker5();
            Console.WriteLine("         6");
            await Worker6();
            Console.WriteLine("         7");
            await Worker7();
            Console.WriteLine("         8");
            await Worker8();
            Console.WriteLine("         9");
            await Worker9();
            Console.WriteLine("         10");
            await Worker10();
            Console.WriteLine("         11");
            await Worker11();
            Console.WriteLine("          12");
            await Worker12();
            Console.WriteLine("          13");
            await Worker13();
            Console.WriteLine("          14");
            await Worker14();
            Console.WriteLine("          15");
            await Worker15();
            Console.WriteLine("          16");
            await Worker16();
            Console.WriteLine("          17");
            await Worker17();
            Console.WriteLine("          18");
            await Worker18();
            Console.WriteLine("          19");
            await Worker19();
        }

        #region UI类别任务测试
        //static public ControlStrand _mainStrand;
        //Generator _timeAction;
        ///// <summary>
        ///// 
        ///// </summary>
        //private void CtrlTest()
        //{
        //    _mainStrand = new ControlStrand(Form);//需要绑定Contrl
        //    _timeAction = Generator.Tgo(_mainStrand, TimeAction);

        //    Generator.Go(_mainStrand, Functional.Bind(Task1Action, (int)300), () => Console.WriteLine("完成"));
        //}

        //private async Task TimeAction()
        //{
        //    var textBox_Action = new TextBox;
        //    while (true)
        //    {
        //        await Generator.sleep(1);
        //        textBox_Action.Text = DateTime.Now.ToString("yy-MM-dd HH:mm:ss.fff");
        //    }
        //}

        //private async Task Task1Action(int time)
        //{
        //    await Task.Delay(1000);
        //}

        #endregion End
    }
}
