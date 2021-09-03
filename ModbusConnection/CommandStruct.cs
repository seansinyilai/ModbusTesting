using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusConnection
{
    public class CommandStruct
    {
        public byte[] transactionID { get; set; }
        public byte[] protocolID { get; set; }
        public byte[] length { get; set; }
        public byte[] Address { get; set; }
        public byte[] FunctionCode { get; set; }
        public byte[] StartAddress { get; set; }
        public byte[] DataCount { get; set; }
        public byte[] data { get; set; }
        public CommandStruct()
        {
            //transactionID = new byte[2];
            //protocolID = new byte[2];
            //length = new byte[2];
            //Address = new byte[1];
            //FunctionCode = new byte[1];
            //StartAddress = new byte[2];
            //data = new byte[65535];
        }
    }
    public class SendStruct
    {
        public int transactionID { get; set; }
        public int protocolID { get; set; }
        public int Address { get; set; }
        public int FunctionCode { get; set; }
        public int StartAddress { get; set; }
        public byte[] data { get; set; }
        public SendStruct()
        {
        }
    }
}
