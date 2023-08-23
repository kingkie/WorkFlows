using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yu3zx.Protocols
{
    public class ProtocolUtil
    {
        /// <summary>
        /// 计算累加和(取最后2字节)
        /// </summary>
        /// <param name="data">计算数据</param>
        /// <returns></returns>
        public static int CalcSum(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return 0;
            }
            int sum = 0;
            foreach (byte bItem in data)
            {
                sum += bItem;
            }
            sum = sum & 0xFFFF; //进行与计算保留2字节
            return sum;
        }

        public static byte[] IntToBytes(int value)
        {
            byte lowByte = (byte)(value & 0xFF);
            byte highByte = (byte)((value >> 8) & 0xFF);
            byte[] val = new byte[] { highByte, lowByte };
            return val;
        }
        /// <summary>
        /// 将int数值转换为占四个字节的byte数组，本方法适用于(高位在前，低位在后)的顺序。  和bytesToInt2（）配套使用
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static byte[] IntToBytes(int value, int num = 4)
        {
            byte[] src = new byte[4];
            src[0] = (byte)((value >> 24) & 0xFF);
            src[1] = (byte)((value >> 16) & 0xFF);
            src[2] = (byte)((value >> 8) & 0xFF);
            src[3] = (byte)(value & 0xFF);

            switch (num)
            {
                case 1:
                    return new byte[] { src[3] };
                case 2:
                    return new byte[] { src[2], src[3] };
                case 3:
                    return new byte[] { src[1], src[2], src[3] };
                case 4:

                    break;
                default:
                    break;
            }
            return src;
        }

        /// <summary>
        /// short转bytes
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static byte[] ShortToBytes(short number)
        {
            byte[] bShort = new byte[2];
            bShort[0] = (byte)(number >> 8);
            bShort[1] = (byte)(number & 0xFF);
            return bShort;
        }
    }
}
