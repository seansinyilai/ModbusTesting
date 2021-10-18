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
        // int autoIncrement = 0;
        public MainTest()
        {
            connectionEstablishment = new ConnecTemp("127.0.0.1", 502);
            ButtonSEND = new RelayCommand(ToSend, param => true);
        }
        private void ToSend(Object e)
        {
            // connectionEstablishment.ReadCoilsCommand_SendMsgFormat(1, 20, 19);
            // connectionEstablishment.ReadDiscreteInputs_SendMsgFormat(1, 197, 22);
            // connectionEstablishment.ReadHoldingRegister_SendMsgFormat(1, 108, 3);
            //  connectionEstablishment.SendWriteSingleCoilMsgFormat(1, 173, new byte[] { 255, 0 });
            // connectionEstablishment.SendWriteSingleRegisterMsgFormat(1, 1, new byte[] { 3,3 });
             connectionEstablishment.SendWriteMultipleCoilsMsgFormat(1, 20, new byte[] { 0, 40 }, new byte[] { 205, 01 }, new byte[] { 125, 20 });
           //  connectionEstablishment.SendWriteMultipleRegistersMsgFormat(1, 2, new byte[] { 0, 3}, new byte[] { 0,10}, new byte[] { 1, 2 }, new byte[] { 1, 2 });
        }
    }
}
