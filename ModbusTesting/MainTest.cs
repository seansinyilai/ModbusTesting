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
            connectionEstablishment.SENDRequest(new SendStruct()
            {
                transactionID = autoIncrement,
                protocolID = 0,
                Address = 1,
                FunctionCode = (int)MBAPHeader.WriteMultipleRegister,
                StartAddress = 200,
                data = new byte[] { 10, 20 }
            });
        }
    }
}
