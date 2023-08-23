using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Yu3zx.CommNetwork;
using Yu3zx.Devices.Equips;
using Yu3zx.InstructModel;
using Yu3zx.LogHelper;
using Yu3zx.Util;

namespace Yu3zx.Devices
{
    class Test
    {
        static void Main()
        {
            ScentDeviceManager.CreateInstance().Init();
            int ChlId = 2;
            int iDura = 10;
            ScentDeviceManager.CreateInstance().PlaySmell("devid",ChlId, iDura);
        }

        public class ScentDeviceManager
        {
            private static ScentDeviceManager instance = null;
            private static object singleLock = new object(); //锁同步

            private Device UsedDev = null;
            /// <summary>
            /// 创建单例
            /// </summary>
            /// <returns>返回单例对象</returns>
            public static ScentDeviceManager CreateInstance()
            {
                lock (singleLock)
                {
                    if (instance == null)
                    {
                        instance = new ScentDeviceManager();
                    }
                }
                return instance;
            }

            #region 属性

            #endregion End

            #region 操作

            public void Init()
            {
                foreach (BaseBus bus in PortsManager.GetInstance().Ports)
                {
                    try
                    {
                        if (!bus.Init())
                        {
                            LogUtil.Instance.LogWrite(string.Format("通信端口{0}初始化失败", bus.PortName));
                        }
                    }
                    catch (Exception ex)
                    {
                        LogUtil.Instance.LogWrite(string.Format("通信端口{0}初始化失败,异常:{1}", bus.PortName, ex.Message));
                    }
                }

                foreach (var dev in DeviceManager.GetInstance().DevicesList)
                {
                    try
                    {
                        dev.Init();
                        Task.Run(() =>
                        {
                            string[] ports = SerialPort.GetPortNames();
                            int len = ports.Length;
                            while (len > 0)
                            {
                                Thread.Sleep(1000);
                                if (dev.Open())
                                {
                                    Console.WriteLine("初始化成功！");
                                    LogUtil.Instance.LogWrite("串口初始化成功！");
                                    return;
                                }
                                else
                                {
                                    len--;
                                }
                            }
                            LogUtil.Instance.LogWrite(dev.DevName + "串口初始化失败！");
                        });
                    }
                    catch (Exception ex)
                    {
                        LogUtil.Instance.LogWrite(dev.DevName + ex.Message);
                    }
                }
            }
            /// <summary>
            /// 获取返回
            /// </summary>
            /// <param name="devID"></param>
            /// <returns></returns>
            public OrderItem GetReturn(string devID)
            {
                OrderItem orderItem = null;
                Device device = DeviceManager.GetInstance().DevicesList.Find(x => x.DevId == devID);
                if (device != null)
                {
                    device.InstructQueue.TryDequeue(out orderItem);
                }
                return orderItem;
            }

            /// <summary>
            /// 播放气味
            /// </summary>
            /// <param name="smell"></param>
            /// <param name="duration">秒</param>
            public void PlaySmell(string devId, int smellid, int duration)
            {
                var devItem = DeviceManager.GetInstance().DevicesList.Find(dev => dev.DevId == devId);
                if (devItem != null)
                {
                    (devItem as OderModule).PlaySmell((byte)smellid, duration);
                }
            }

            /// <summary>
            /// 停止播放
            /// </summary>
            /// <param name="channl"></param>
            public void StopSmell()
            {
                foreach (var dev in DeviceManager.GetInstance().DevicesList)
                {
                    (dev as OderModule).StopPlay();
                }
            }

            public void Close()
            {
                foreach (var dev in DeviceManager.GetInstance().DevicesList)
                {
                    (dev as OderModule).Close();
                }
            }

            /// <summary>
            /// 打开盖板
            /// </summary>
            public void OpenCoverPlate()
            {
                byte[] bOpenCover = new byte[] { 0xF5, 0x00, 0x00, 0x00, 0x00, 0x05, 0x01, 0x01, 0x51, 0xCB, 0x55 };
                foreach (var dev in DeviceManager.GetInstance().DevicesList)
                {
                    (dev as OderModule).Write(bOpenCover);
                }
            }

            /// <summary>
            /// 异步打开盖板
            /// </summary>
            public void AsyncOpenCoverPlate()
            {
                byte[] bOpenCover = new byte[] { 0xF5, 0x00, 0x00, 0x00, 0x00, 0x05, 0x01, 0x01, 0x51, 0xCB, 0x55 };
                Task.Run(async () => {
                    foreach (var dev in DeviceManager.GetInstance().DevicesList)
                    {
                        (dev as OderModule).Write(bOpenCover);
                    }
                    await Task.Delay(200);
                });
            }

            /// <summary>
            /// 关闭盖板
            /// </summary>
            public void CloseCoverPlate()
            {
                byte[] bCloseCover = new byte[] { 0xF5, 0x00, 0x00, 0x00, 0x00, 0x05, 0x01, 0x00, 0x91, 0x0A, 0x55 };
                foreach (var dev in DeviceManager.GetInstance().DevicesList)
                {
                    (dev as OderModule).Write(bCloseCover);
                }
            }
            /// <summary>
            /// 获取设备各通道胶囊状况
            /// </summary>
            public void CapsuleStatus()
            {
                for (int chl = 1; chl <= 20; chl++)
                {
                    foreach (var dev in DeviceManager.GetInstance().DevicesList)
                    {
                        (dev as OderModule).Write(PackCapsuleStatus(chl));
                        Thread.Sleep(100);
                    }
                }
            }

            /// <summary>
            /// 获取设备各通道胶囊状况
            /// </summary>
            public void AsyncCapsuleStatus()
            {
                Task.Run(async () => {
                    await Task.Delay(5000);
                    //获取气味胶囊
                    for (int chl = 1; chl <= 20; chl++)
                    {
                        foreach (var dev in DeviceManager.GetInstance().DevicesList)
                        {
                            (dev as OderModule).Write(PackCapsuleStatus(chl));
                        }
                        await Task.Delay(200);
                    }
                });
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="chl"></param>
            /// <returns></returns>
            private byte[] PackCapsuleStatus(int chlNum)
            {
                byte bchl = (byte)chlNum;
                List<byte> lCmd = new List<byte>();
                List<byte> lSend = new List<byte>();

                lCmd.Add(0x00);//本机地址
                lCmd.Add(0x00);//本机地址
                lCmd.Add(0x00);//小地址
                lCmd.Add(0x00);//小地址

                lCmd.Add(0x11);//指令码
                lCmd.Add(0x01);//数据长度
                lCmd.Add(bchl);//通道

                lSend.Add(0xF5);
                lSend.AddRange(lCmd);
                lSend.AddRange(Crc16.CalcCrc(lCmd.ToArray()));
                lSend.Add(0x55);

                return lSend.ToArray();
            }
            /// <summary>
            /// 
            /// </summary>
            public void CapsuleUidStatus()
            {

            }

            #endregion End
        }
    }
}
