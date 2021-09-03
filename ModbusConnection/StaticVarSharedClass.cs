using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusConnection
{
    public class StaticVarSharedClass
    {
        public static string TransactionID = "TransactionID";
        public static string ProtocalID = "ProtocalID";
        public static string LengthFromAddressStartToDataEnd = "LengthFromAddressStartToDataEnd";
        public static string Address = "Address";
        public static string FunctionCode = "FunctionCode";
        public static string StartRegisterAdd = "StartRegisterAdd";
        public static string Data = "Data";

        public static byte[] AddressByte = new byte[] { 1 };
        public static byte[] FunctionByte = new byte[1] { Convert.ToByte(MBAPHeader.WriteMultipleRegister) };

       
    }
}
