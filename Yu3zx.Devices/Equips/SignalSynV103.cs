using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Yu3zx.CommNetwork;
using Yu3zx.CommNetwork.Ports;
using Yu3zx.InstructModel;
using Yu3zx.LogHelper;
using Yu3zx.Util;

namespace Yu3zx.Devices.Equips
{
    public class SignalSynV103 : Device
    {
        private Random rd = new Random();

        private BaseBus commbus;
        private bool connFlag = false;//连接标志
        public SignalSynV103()
        {
            if (string.IsNullOrEmpty(this.DevId))
            {
                this.DevId = "Dev" + DateTime.Now.ToString("HHmmssfff") + rd.Next(10000, 99999).ToString();
            }

            this.DevName = "信号同步器V103";

            this.DevDesc = "信号同步器,用于RF43模块同步信号";

            this.DevType = DeviceType.Sync103;
        }

        public SignalSynV103(BaseBus bus)
        {
            if (string.IsNullOrEmpty(this.DevId))
            {
                this.DevId = "Dev" + DateTime.Now.ToString("HHmmssfff") + rd.Next(10000, 99999).ToString();
            }

            this.DevName = "信号同步器V103";

            this.DevDesc = "信号同步器,用于RF43模块同步信号";

            this.DevType = DeviceType.Sync103;

            commbus = bus;

            //commbus.ThrowUpHandle += Commbus_ThrowUpHandle;
        }

        public byte Channel
        {
            get;
            set;
        } = 0;

        private void CommBus_ConnectStateChangeHandle(bool bConnect)
        {
            if (bConnect)
            {
                connFlag = true;
                Console.WriteLine("已经连接！");
            }
            else
            {
                connFlag = false;
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
            else
            {
                if (buffer != null && buffer.Length == 6) //F5 A7 96 01 3D 55
                {
                    int len = buffer.Length;
                    if (ConverterHelper.Bytes2String(buffer) == "F5 A7 96 01 3D 55")
                    {
                        OrderItem item = new OrderItem();
                        item.CmdCode = 0x01;//获取UUID
                        InstructQueue.Enqueue(item);
                    }
                }
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
                    return commbus.Connected && connFlag; //
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
                OrderItem order = new OrderItem();
                order.CmdCode = 0x01;
                order.LocalAddr = 1;
                order.RemoteAddr = 1;

                byte[] cmdBytes = Protocol.PackFrame(order);
                //byte[] tData = TransparentTrace(cmdBytes);
                // Console.WriteLine((CommBus as Communication.Ports.SingleCommBus).CommName + "Send:" + string.Join(" ", cmdBytes.Select(x => x.ToString("X2"))));
                byte[] getUUID = new byte[] { 0xF5, 0x27, 0x00, 0x27, 0x55 };
                Console.WriteLine((CommBus as SingleCommBus).CommName + "Send:" + string.Join(" ", getUUID.Select(x => x.ToString("X2"))));
                if (Write(getUUID))
                {
                    Thread.Sleep(500);
                    if (InstructQueue.Count > 0)
                    {
                        return true;
                    }
                    else
                    {
                        try
                        {
                            CommBus.Close();
                        }
                        catch
                        {
                        }
                    }
                }
            }
            return false;
        }

        public override bool Open()
        {
            //if(string.IsNullOrEmpty(this.PortID))
            //{
            //    Log.LogUtil.Instance.LogWrite("通信接口未配置", MsgLevel.Err);
            //    return false;
            //}
            //if(commbus != null)
            //{
            //    return commbus.Open();
            //}
            //commbus = PortsManager.GetInstance().Ports.Find(x => x.PortId == this.PortID);
            //if (commbus != null)
            //{
            //    return commbus.Open();
            //}
            //else
            //{
            //    return false;
            //}

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
            if (commbus != null)
            {
                return commbus.Close();
            }
            commbus = PortsManager.GetInstance().Ports.Find(x => x.PortId == this.PortID);
            if (commbus != null)
            {
                return commbus.Close();
            }
            else
            {
                return true;
            }
        }

        public override bool Write(byte[] buff)
        {
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

        #region 操作
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
                skii.CmdCode = 0x01;
                skii.RemoteAddr = 0xFFFF;
                skii.LocalAddr = 0x01;

                byte[] i2b = VerifyHelper.IntToBytes(duration * 1000, 4); //秒 * 1000 = 毫秒

                List<byte> lPayload = new List<byte>();

                lPayload.AddRange(new byte[] { 0x00, 0x00 });//预留
                lPayload.AddRange(new byte[] { 0x00, smellid });//气味编号
                lPayload.AddRange(i2b);//持续时间
                //lPayload.AddRange(BitConverter.GetBytes(duration * 1000).Reverse());

                skii.Payload.AddRange(lPayload.ToArray());
                //-------------------
                if (Protocol is Protocols.RealizeProtocols.ScentMultiWirelessProtocol)
                {
                    (Protocol as Protocols.RealizeProtocols.ScentMultiWirelessProtocol).TargetDevType = 0x02;
                    (Protocol as Protocols.RealizeProtocols.ScentMultiWirelessProtocol).DevType = 0x01;
                }

                byte[] cmdBytes = Protocol.PackFrame(skii);

                byte[] tData = TransparentTrace(cmdBytes);//透传包装

                this.Write(tData);

                Console.WriteLine(string.Join(" ", tData.Select(x => x.ToString("X2"))));
            }
            catch (Exception ex)
            {
                LogUtil.Instance.LogWrite(ex);
            }
        }

        /// <summary>
        /// 组装成-无用
        /// </summary>
        /// <param name="skIns"></param>
        public void PlaySmell(OrderItem skIns)
        {
            byte[] cmdBytes = Protocol.PackFrame(skIns);
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
                skii.CmdCode = 0x02;

                skii.LocalAddr = 0x01;
                skii.RemoteAddr = 0xFFFF;
                if (Protocol is Protocols.RealizeProtocols.ScentMultiWirelessProtocol)
                {
                    (Protocol as Protocols.RealizeProtocols.ScentMultiWirelessProtocol).TargetDevType = 0x02;
                    (Protocol as Protocols.RealizeProtocols.ScentMultiWirelessProtocol).DevType = 0x01;
                }
                byte[] cmdBytes = Protocol.PackFrame(skii);
                byte[] tData = TransparentTrace(cmdBytes);
                this.Write(tData);
                Console.WriteLine(string.Join(" ", tData.Select(x => x.ToString("X2"))));
            }
            catch (Exception ex)
            {
                LogUtil.Instance.LogWrite(ex);
            }
        }
        /// <summary>
        /// 唤醒
        /// </summary>
        public void WakeUp()
        {
            try
            {
                byte[] wakeUp = new byte[] { 0xF5, 0x70, 0x55 }; //唤醒指令
                byte[] tData = TransparentTrace(wakeUp);
                this.Write(tData);
                Console.WriteLine(string.Join(" ", tData.Select(x => x.ToString("X2"))));
            }
            catch (Exception ex)
            {
                LogUtil.Instance.LogWrite(ex);
            }
        }
        /// <summary>
        /// 透传包装
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        private byte[] TransparentTrace(byte[] payload)
        {
            if (payload == null || payload.Length == 0)
            {
                return null;
            }

            List<byte> lCmd = new List<byte>();
            try
            {
                lCmd.Add(0x51);
                lCmd.Add(Channel);
                lCmd.Add((byte)payload.Length);
                lCmd.AddRange(payload);

                byte[] sumCalc = VerifyHelper.SumCheck(lCmd.ToArray());
                lCmd.AddRange(sumCalc);
                lCmd.Insert(0, 0xF5);
                lCmd.Add(0x55);
                return lCmd.ToArray();
            }
            catch (Exception ex)
            {
                LogUtil.Instance.LogWrite(ex.Message, MsgLevel.Err);
            }
            return null;

        }
        #endregion End
    }
}
