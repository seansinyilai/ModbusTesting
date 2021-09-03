using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusConnection
{
    public enum MBAPHeader
    {
        ReadMultipleRegister = 3,
        WriteMultipleRegister = 16
    }
}
