using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModbusConnection;

namespace ModbusTesting
{
    public class ConnecTemp : ModbusMaster
    {
        public ConnecTemp(string hostIP, int port) : base(hostIP, port)
        {
           
        }
       
        public override bool DealWithMsgFunc(string msg)
        {
            return true;
        }
    }
}
