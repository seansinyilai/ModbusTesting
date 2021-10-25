using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusTesting
{
    public class TempRecipeStruct
    {
        private ushort _temperature;

        public ushort Temperature
        {
            get { return _temperature; }
            set { _temperature = value; }
        }

        private ushort _tempTime;

        public ushort TempTime
        {
            get { return _tempTime; }
            set { _tempTime = value; }
        }
    }
}
