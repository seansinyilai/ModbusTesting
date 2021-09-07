using ModbusConnection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusTesting
{
    public class MainTest
    {
        public RelayCommand ButtonSEND { get; set; }
        ConnecTemp connectionEstablishment { get; set; }
        int autoIncrement = 0;
        public MainTest()
        {
            connectionEstablishment = new ConnecTemp("127.0.0.1", 5002);
            ButtonSEND = new RelayCommand(ToSend, param => true);
        }
        private void ToSend(Object e)
        {
            autoIncrement++;
            connectionEstablishment.SENDMsgFormat(autoIncrement, 0, 1, MBAPHeader.WriteMultipleRegister, 200, new byte[] { 20, 30 });
        }
    }
}
