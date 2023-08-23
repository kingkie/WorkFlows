using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yu3zx.InstructModel;
using Yu3zx.Util;

namespace Yu3zx.Protocols.RealizeProtocols
{
    public class ScentMultiWirelessProtocol : BaseProtocol
    {
        private Random rd = new Random();

        private readonly byte header = 0xF5; //包头

        private readonly byte tailer = 0x55; //包尾

        private byte currentCmd = 0;//未定义

        private const int FrameMinLen = 11;//帧最小长度


        /// <summary>
        /// 构造函数
        /// </summary>
        public ScentMultiWirelessProtocol()
        {
            if (string.IsNullOrEmpty(this.ProtocolId))
            {
                this.ProtocolId = "P" + DateTime.Now.Millisecond.ToString("x2") + rd.Next(10000, 99999).ToString();
            }

            if (string.IsNullOrEmpty(this.ProtocolName))
            {
                this.ProtocolName = "气味播放器多点无线组网通讯协议";
            }
        }

        #region 属性区

        /// <summary>
        /// 帧头默认值
        /// </summary>
        [JsonIgnore]
        public byte Header
        {
            get
            {
                return header;
            }
        }
        /// <summary>
        /// 帧尾默认值
        /// </summary>
        [JsonIgnore]
        public byte Tailer
        {
            get
            {
                return tailer;
            }
        }
        /// <summary>
        /// 指令码
        /// </summary>
        public byte InstructionCode
        {
            get;
            set;
        }

        /// <summary>
        /// 设备类型：0x01 手持遥控器;0x02 板卡;0x03 副柜转接板;0x04 上位机;
        ///           0x05 头戴式;0x06 同步器103;0x07 同步器429
        /// </summary>
        public byte DevType
        {
            get;
            set;
        }
        /// <summary>
        /// 本机设备地址
        /// </summary>
        public int DevAddr
        {
            get; set;
        }
        /// <summary>
        /// 目标设备类型
        /// </summary>
        public byte TargetDevType
        {
            get;
            set;
        }
        /// <summary>
        /// 目标设备地址
        /// </summary>
        public int TargetDevAddr
        {
            get; set;
        }

        #endregion End

        /// <summary>
        /// 验证一条数据
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        public override bool Validity(byte[] source, out byte[] dest)
        {
            dest = null;
            if (source == null || source.Length < FrameMinLen)
            {
                dest = null;
                return false;
            }
            int startFrame = -1;
            int endFrame = -1;

            int dataLen = source.Length;
            for (int i = 0; i < dataLen - 10; i++)
            {
                if (source[i] == Header)
                {
                    startFrame = i;
                    for (int j = i + 1; j < dataLen - 10; j++)
                    {
                        if (source[j] == Tailer && (j - startFrame) >= (FrameMinLen - 1))
                        {
                            byte[] frameData = new byte[j - startFrame + 1];
                            Array.Copy(source, startFrame, frameData, 0, frameData.Length);
                            byte[] checkbit = VerifyHelper.SumCheck(frameData, 1, frameData.Length - 4);
                            if (checkbit[0] == frameData[frameData.Length - 3] && checkbit[0] == frameData[frameData.Length - 2])
                            {
                                dest = frameData;
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 装帧
        /// </summary>
        /// <param name="ins"></param>
        /// <returns></returns>
        public override byte[] PackFrame(OrderItem ins)
        {
            if (ins == null)
            {
                return null;
            }
            List<byte> lSend = new List<byte>();
            List<byte> lCmd = new List<byte>();
            try
            {
                lCmd.Add(DevType);//设备类型
                lCmd.AddRange(ProtocolUtil.IntToBytes(ins.LocalAddr, 2));
                lCmd.Add(TargetDevType);//目标设备类型
                lCmd.AddRange(ProtocolUtil.IntToBytes(ins.RemoteAddr, 2));

                lCmd.Add((byte)ins.CmdCode);

                if (ins.Payload != null && ins.Payload.Count > 0)
                {
                    lCmd.AddRange(ins.Payload); //有承载数据
                }

                lSend.Add(Header);
                lSend.AddRange(lCmd);
                lSend.AddRange(VerifyHelper.SumCheck(lCmd.ToArray()));
                lSend.Add(Tailer);
                return lSend.ToArray();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 拆帧成 指令拆包
        /// </summary>
        /// <param name="data">指令数据</param>
        /// <returns></returns>
        public override OrderItem UnPackFrame(byte[] data)
        {
            if (data == null || data.Length < 11) //帧数据长度不够
            {
                return null;
            }
            if (data[0] == header || data[data.Length] == tailer) //先判断头尾
            {
                byte[] calcBytes = new byte[data.Length - 4];//去掉包头包尾
                Array.Copy(data, 1, calcBytes, 0, calcBytes.Length);
                byte[] crcCalc = VerifyHelper.SumCheck(calcBytes);//累加和
                if (crcCalc[0] == data[data.Length - 3] && crcCalc[1] == data[data.Length - 2])
                {
                    OrderItem skIns = new OrderItem();
                    //skIns.Header = header;
                    skIns.RemoteAddr = calcBytes[1] * 256 + calcBytes[2];

                    skIns.LocalAddr = calcBytes[4] * 256 + calcBytes[5];
                    skIns.CmdCode = calcBytes[6];

                    if (calcBytes.Length - 7 > 0)
                    {
                        byte[] payload = new byte[calcBytes.Length - 7];
                        Array.Copy(calcBytes, 7, payload, 0, payload.Length);
                        skIns.Payload.AddRange(payload);
                    }
                    return skIns;
                }
            }
            return null;
        }
    }
}
