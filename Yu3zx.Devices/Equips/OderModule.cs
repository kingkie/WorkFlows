using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Yu3zx.CommNetwork;
using Yu3zx.InstructModel;
using Yu3zx.LogHelper;
using Yu3zx.Protocols.RealizeProtocols;

namespace Yu3zx.Devices.Equips
{
    public class OderModule : Device
    {
        private Random rd = new Random();

        private BaseBus commbus;

        AutoResetEvent evenResetEvent = new AutoResetEvent(false);
        public OderModule()
        {
            if (string.IsNullOrEmpty(this.DevId))
            {
                this.DevId = "Dev" + DateTime.Now.ToString("HHmmssfff") + rd.Next(10000, 99999).ToString();
            }

            this.DevName = "气味模块";

            this.DevDesc = "气味模块，可以根据需要组合";

            this.DevType = DeviceType.OderMod;
        }

        public OderModule(BaseBus bus)
        {
            if (string.IsNullOrEmpty(this.DevId))
            {
                this.DevId = "Dev" + DateTime.Now.ToString("HHmmssfff") + rd.Next(10000, 99999).ToString();
            }

            this.DevName = "气味模块";

            this.DevDesc = "气味模块，可以根据需要组合";

            this.DevType = DeviceType.OderMod;

            CommBus = bus;

            Protocol = new SmellKingdomProtocol();
        }

        private void CommBus_ConnectStateChangeHandle(bool bConnect)
        {
            if (bConnect)
            {
                Console.WriteLine("已经连接！");
            }
            else
            {
                Console.WriteLine("连接未成功！");
            }
        }

        private void Bus_DataReceiveHandle(string devIds, byte[] buffer)
        {
            if (devIds.Contains(this.DevId))
            {

            }
            string logs = "收到：" +
                "" + ConverterHelper.Bytes2String(buffer);
            Console.WriteLine(logs);

            LogUtil.Instance.LogWrite(logs, MsgLevel.Comm);

            if (Protocol != null && Protocol.Validity(buffer, out byte[] dest))
            {
                if (InstructQueue.Count > 1000)
                {
                    ClsQueue();
                }
                OrderItem order = Protocol.UnPackFrame(dest);
                InstructQueue.Enqueue(order);
                Console.WriteLine("应答：" + order.Payload[0].ToString() + " 收到个数：" + InstructQueue.Count.ToString());
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void ClsQueue()
        {
            int cnt = InstructQueue.Count;
            OrderItem od = null;
            for (int i = 0; i < cnt; i++)
            {
                InstructQueue.TryDequeue(out od);
            }
        }

        [JsonIgnore]
        public override bool Connected
        {
            get
            {
                if (commbus != null)
                {
                    return commbus.Connected;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 公共通信总线
        /// </summary>
        [JsonIgnore]
        public override BaseBus CommBus
        {
            get { return commbus; }
            set
            {
                commbus = value;
            }
        }

        public override bool Init()
        {
            if (string.IsNullOrEmpty(this.PortID))
            {
                LogUtil.Instance.LogWrite("通信接口未配置", MsgLevel.Err);
                return false;
            }
            if (this.Protocol == null)
            {
                this.Protocol = new SmellKingdomProtocol(); //默认使用的协议
            }
            this.CommBus = PortsManager.GetInstance().Ports.Find(x => x.PortId == this.PortID);
            if (this.CommBus != null)
            {
                try
                {
                    //注销掉可能已经增加的
                    CommBus.DataReceiveHandle -= Bus_DataReceiveHandle;
                    CommBus.ConnectStateChangeHandle -= CommBus_ConnectStateChangeHandle;
                }
                catch
                { }
                CommBus.AddDeviceBind(this.DevId);
                CommBus.DataReceiveHandle += Bus_DataReceiveHandle;
                CommBus.ConnectStateChangeHandle += CommBus_ConnectStateChangeHandle;
                CommBus.MessageNtifyHandle += CommBus_MessageNtifyHandle;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void CommBus_MessageNtifyHandle(string msg, int msgNum = 0)
        {
            switch (msgNum)
            {
                case 0:

                    break;
                case 1:
                    Console.WriteLine("没有");
                    break;
                default:

                    break;
            }
        }
        /// <summary>
        /// 自动连接
        /// </summary>
        /// <returns></returns>
        public override bool AutoConnect()
        {
            if (CommBus == null)
            {
                return false;
            }

            if (CommBus.AutoConnect())//说明已经更换
            {
                ClsQueue();
                Thread.Sleep(2000);
                OrderItem order = new OrderItem();
                order.CmdCode = 0x04;
                byte[] cmdBytes = Protocol.PackFrame(order);
                if (Write(cmdBytes))
                {
                    //wait for uuid return
                    Thread.Sleep(1000);
                    if (InstructQueue.Count > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override bool Open()
        {
            if (string.IsNullOrEmpty(this.PortID))
            {
                LogUtil.Instance.LogWrite("通信接口未配置", MsgLevel.Err);
                return false;
            }
            if (CommBus != null)
            {
                return CommBus.Open();
            }
            CommBus = PortsManager.GetInstance().Ports.Find(x => x.PortId == this.PortID);
            if (CommBus != null)
            {
                try
                {
                    //注销掉可能已经增加的
                    CommBus.DataReceiveHandle -= Bus_DataReceiveHandle;
                    CommBus.ConnectStateChangeHandle -= CommBus_ConnectStateChangeHandle;
                }
                catch
                { }
                CommBus.AddDeviceBind(this.DevId);
                CommBus.DataReceiveHandle += Bus_DataReceiveHandle;
                CommBus.ConnectStateChangeHandle += CommBus_ConnectStateChangeHandle;
                return CommBus.Open();
            }
            else
            {
                return false;
            }
        }

        public override bool Close()
        {
            if (string.IsNullOrEmpty(this.PortID))
            {
                LogUtil.Instance.LogWrite("通信接口未配置", MsgLevel.Err);
                return false;
            }
            if (CommBus != null)
            {
                return CommBus.Close();
            }
            CommBus = PortsManager.GetInstance().Ports.Find(x => x.PortId == this.PortID);
            if (CommBus != null)
            {
                return CommBus.Close();
            }
            else
            {
                return true;
            }
        }

        public override bool Write(byte[] buff)
        {
            //byte[] bufs = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };
            if (CommBus != null)
            {
                if (CommBus.Connected)
                {
                    return CommBus.Write(buff);
                }
                else
                {
                    try
                    {
                        CommBus.Open();
                        return CommBus.Write(buff);
                    }
                    catch
                    {

                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 数据接收
        /// </summary>
        /// <param name="dataRev"></param>
        /// <returns></returns>
        public override bool OnDataReceive(byte[] dataRev)
        {
            return base.OnDataReceive(dataRev);
        }

        /// <summary>
        /// 播放单路气味
        /// </summary>
        /// <param name="smellid"></param>
        /// <param name="duration">秒</param>
        public void PlaySmell(byte smellid, int duration)
        {
            try
            {
                OrderItem skii = new OrderItem();
                skii.CmdCode = 0x02;
                List<byte> lPayload = new List<byte>();
                lPayload.Add(smellid);
                lPayload.AddRange(BitConverter.GetBytes(duration * 1000).Reverse());
                //skii.DataLen = (byte)lPayload.Count;
                skii.Payload.AddRange(lPayload.ToArray());
                byte[] cmdBytes = Protocol.PackFrame(skii);

                this.Write(cmdBytes);

                Console.WriteLine(string.Join(" ", cmdBytes.Select(x => x.ToString("X2"))));
            }
            catch (Exception ex)
            {
                LogUtil.Instance.LogWrite(ex);
            }
        }

        /// <summary>
        /// 组装成
        /// </summary>
        /// <param name="skIns"></param>
        public void PlaySmell(OrderItem skIns)
        {
            byte[] cmdBytes = Protocol.PackFrame(skIns);
            Console.WriteLine(string.Join(" ", cmdBytes.Select(x => x.ToString("X2"))));
            this.Write(cmdBytes);
        }

        /// <summary>
        /// 停止播放
        /// </summary>
        public void StopPlay()
        {
            try
            {
                OrderItem skii = new OrderItem();
                skii.CmdCode = 0x00;
                //skii.DataLen = 0x01;
                skii.Payload.AddRange(new byte[] { 0x00 });
                byte[] cmdBytes = Protocol.PackFrame(skii);

                Console.WriteLine("发送停止：" + string.Join(" ", cmdBytes.Select(x => x.ToString("X2"))));

                this.Write(cmdBytes);
            }
            catch (Exception ex)
            {
                LogUtil.Instance.LogWrite(ex);
            }
        }

        public void Test()
        {
            Console.WriteLine(DateTime.Now.ToString("时间yyyyMMddHHmmss:") + "测试！");
        }
    }
}
