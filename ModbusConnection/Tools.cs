﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusConnection
{
    public static class Tools
    {/// <summary>
     /// 將數值拆成高低位元
     /// </summary>
     /// <param name="val"></param>
     /// <returns></returns>
        public static byte[] SplitShortToHighAndLowByte(this ushort val)
        {
            byte[] tmp = new byte[2];
            string binartsaa = Convert.ToString(val, 2).PadLeft(16, '0');
            string highbit = binartsaa.Substring(0, 8);
            string lowbit = binartsaa.Substring(8, 8);
            tmp[0] = Convert.ToByte(Convert.ToInt32(highbit, 2));
            tmp[1] = Convert.ToByte(Convert.ToInt32(lowbit, 2));
            return tmp;
        }
        /// <summary>
        /// 將數值拆成高低位元
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static byte[] SplitIntToHighAndLowByte(this int val)
        {
            byte[] tmp = new byte[2];
            string binartsaa = Convert.ToString(val, 2).PadLeft(16, '0');
            string highbit = binartsaa.Substring(0, 8);
            string lowbit = binartsaa.Substring(8, 8);
            tmp[0] = Convert.ToByte(Convert.ToInt32(highbit, 2));
            tmp[1] = Convert.ToByte(Convert.ToInt32(lowbit, 2));
            return tmp;
        }
        /// <summary>
        /// 將數值拆成高低位元
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static byte[] SplitIntToHighAndLowByte2(this int val)
        {
            byte[] tmp = new byte[2];
            string binartsaa = Convert.ToString(val, 2).PadLeft(16, '0');
            string highbit = binartsaa.Substring(8, 8);
            string lowbit = binartsaa.Substring(0, 8);
            tmp[0] = Convert.ToByte(Convert.ToInt32(highbit, 2));
            tmp[1] = Convert.ToByte(Convert.ToInt32(lowbit, 2));
            return tmp;
        }
        /// <summary>
        /// 將數值拆成高低位元
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static byte[] SplitIntToReverseHighAndLowByte(this int val)
        {
            byte[] tmp = new byte[2];
            string binartsaa = Convert.ToString(val, 2).PadLeft(16, '0');
            string highbit = binartsaa.Substring(0, 8);
            string lowbit = binartsaa.Substring(8, 8);
            tmp[0] = Convert.ToByte(Convert.ToInt32(lowbit, 2));
            tmp[1] = Convert.ToByte(Convert.ToInt32(highbit, 2));
            return tmp;
        }
        /// <summary>
        /// 將數值拆成高低位元
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static byte[] SplitByteToHighAndLowByte(this byte val)
        {
            byte[] tmp = new byte[2];
            string binartsaa = Convert.ToString(val, 2).PadLeft(16, '0');
            string highbit = binartsaa.Substring(0, 8);
            string lowbit = binartsaa.Substring(8, 8);
            tmp[0] = Convert.ToByte(Convert.ToInt32(highbit, 2));
            tmp[1] = Convert.ToByte(Convert.ToInt32(lowbit, 2));
            return tmp;
        }
        /// <summary>
        /// 將數值拆成高低位元
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static byte[] SplitInt32ToHighAndLowByte(this int val)
        {
            byte[] tmp = new byte[2];
            string binartsaa = Convert.ToString(val, 2).PadLeft(32, '0');
            string highbit = binartsaa.Substring(0, 16);
            string lowbit = binartsaa.Substring(8, 16);
            tmp[0] = Convert.ToByte(Convert.ToInt32(highbit, 2));
            tmp[1] = Convert.ToByte(Convert.ToInt32(lowbit, 2));
            return tmp;
        }
        public static string ReverseString(this string val)
        {
            string tmp = string.Empty;
            for (int i = val.Length - 1; i >= 0; i--)
            {
                tmp += val[i];
            }
            return tmp;
        }
    }
}
