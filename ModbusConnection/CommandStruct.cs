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
            //TransactionID = new byte[2];
            //ProtocolID = new byte[2];
            //length = new byte[2];
            //Address = new byte[1];
            //FunctionCode = new byte[1];
            //StartAddress = new byte[2];
            //data = new byte[65535];
        }
    }
    public class SendStruct
    {
        public ushort TransactionID { get; set; }
        public ushort ProtocolID { get; set; }
        public byte Address { get; set; }
        public byte FunctionCode { get; set; }
        public ushort StartAddress { get; set; }
        public Action ToActLike { get; set; }
        public byte[] data { get; set; }
        public int dataLength { get; set; }
        public SendStruct()
        {
        }
    }
}
