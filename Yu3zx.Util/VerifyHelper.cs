namespace Yu3zx.Util
{
    /// <summary>
    /// 校验类
    /// </summary>
    public class VerifyHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] SumCheck(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return new byte[] { 0x00, 0x00 };
            }
            int sum = 0;
            foreach (byte bItem in data)
            {
                sum += bItem;
            }
            sum = sum & 0xFFFF; //进行与计算保留2字节

            return IntToBytes(sum, 2);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="startIndex"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static byte[] SumCheck(byte[] data, int startIndex, int len)
        {
            byte[] checksum = new byte[2];
            int checkData = 0;
            if (data != null)
                for (int j = startIndex, i = 0; i < len; j++, i++)
                {
                    checkData += data[j];
                }
            checksum[0] = (byte)(0xff & (checkData >> 8));
            checksum[1] = (byte)(0xff & checkData);
            return checksum;
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
    }
}
