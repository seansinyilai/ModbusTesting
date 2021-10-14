using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusConnection
{
    public enum FunctionCode
    {
        ReadCoil = 1,
        ReadMultipleRegister = 3,
        WriteMultipleRegister = 16
    }
}
