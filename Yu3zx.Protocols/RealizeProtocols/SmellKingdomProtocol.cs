using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Yu3zx.InstructModel;
using Yu3zx.Util;

namespace Yu3zx.Protocols.RealizeProtocols
{
    public class SmellKingdomProtocol : BaseProtocol
    {
        private Random rd = new Random();

        private readonly byte header = 0xF5; //包头

        private readonly byte tailer = 0x55; //包尾

        private byte currentCmd = 0;//未定义

        /// <summary>
        /// 构造函数
        /// </summary>
        public SmellKingdomProtocol()
        {
            if (string.IsNullOrEmpty(this.ProtocolId))
            {
                this.ProtocolId = "P" + DateTime.Now.Millisecond.ToString("x2") + rd.Next(10000, 99999).ToString();
            }

            if (string.IsNullOrEmpty(this.ProtocolName))
            {
                this.ProtocolName = "气味王国归一化协议";
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
            dest = new byte[] { };
            if (source == null || source.Length < 10)
            {
                dest = null;
                return false;
            }
            int dataLen = source.Length;
            for (int i = 0; i < dataLen - 10; i++)
            {
                if (source[i] == Header)
                {
                    int frameLen = source[i + 6] + i + 9;
                    if (frameLen <= dataLen)
                    {
                        if (source[frameLen] == Tailer)
                        {
                            byte[] data = new byte[frameLen - i - 3];
                            Array.Copy(source, i + 1, data, 0, frameLen - i - 3);//
                            byte[] crc = Crc16.CalcCrc(data);
                            if (crc[0] == source[frameLen - 2] && crc[1] == source[frameLen - 1])
                            {
                                dest = new byte[frameLen - i + 1];
                                Array.Copy(source, i, dest, 0, frameLen - i + 1);//
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
                lCmd.AddRange(ProtocolUtil.IntToBytes(ins.LocalAddr, 2));
                lCmd.AddRange(ProtocolUtil.IntToBytes(ins.RemoteAddr, 2));

                lCmd.Add((byte)ins.CmdCode);
                if (ins.Payload != null && ins.Payload.Count > 0)
                {
                    lCmd.Add((byte)ins.Payload.Count);
                    lCmd.AddRange(ins.Payload);
                }
                else
                {
                    lCmd.Add(0x00);
                }
                lSend.Add(Header);
                lSend.AddRange(lCmd);
                lSend.AddRange(Crc16.CalcCrc(lCmd.ToArray()));
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
                byte[] calcBytes = new byte[data.Length - 4];
                Array.Copy(data, 1, calcBytes, 0, calcBytes.Length);
                byte[] crcCalc = Crc16.CalcCrc(calcBytes);
                if (crcCalc[0] == data[data.Length - 3] && crcCalc[1] == data[data.Length - 2])
                {
                    OrderItem skIns = new OrderItem();
                    //skIns.Header = header;
                    skIns.RemoteAddr = calcBytes[0] * 256 + calcBytes[1];

                    skIns.LocalAddr = calcBytes[2] * 256 + calcBytes[3];
                    skIns.CmdCode = calcBytes[4];
                    if (calcBytes[5] > 0)
                    {
                        byte[] payload = new byte[calcBytes[5]];
                        Array.Copy(calcBytes, 6, payload, 0, payload.Length);
                        skIns.Payload.AddRange(payload);
                    }
                    //skIns.FrameData = calcBytes.Skip(7).Take(calcBytes.Length - 7).ToArray();
                    //orderItem.LFrameData.AddRange(calcBytes.Skip(7).Take(calcBytes.Length - 7));
                    //是否上抛

                    return skIns;
                }
            }
            return null;
        }
    }
}
