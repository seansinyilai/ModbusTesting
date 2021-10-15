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
            connectionEstablishment = new ConnecTemp("127.0.0.1", 502);
            ButtonSEND = new RelayCommand(ToSend, param => true);
        }
        private void ToSend(Object e)
        {
            autoIncrement++;
            //var high = "11001101";
            //var low =  "00000001";
            //var com = high + low;
            //var a = Convert.ToInt32(com, 2);
            ///connectionEstablishment.SENDMsgFormat(autoIncrement, 0, 1, FunctionCode.WriteMultipleRegister, 200, new byte[] { 20, 30 });
            connectionEstablishment.ReadCoilsCommand_SendMsgFormat(autoIncrement, 0, 1, FunctionCode.ReadCoils, ModbusConnection.Action.ToRead, 20, 19);
           // connectionEstablishment.ReadCoilsCommand_SendMsgFormat(autoIncrement, 0, 1, FunctionCode.ReadDiscreteInputs, ModbusConnection.Action.ToRead, 197, 22);
           // connectionEstablishment.ReadCoilsCommand_SendMsgFormat(autoIncrement, 0, 1, FunctionCode.ReadHoldingRegisters, ModbusConnection.Action.ToRead, 108, 3);
           // connectionEstablishment.SendMsgFormat(autoIncrement, 0, 1, FunctionCode.WriteSingleCoil, ModbusConnection.Action.ToWrite, 173, new byte[] { 255,0 });
           // connectionEstablishment.SendMsgFormat(autoIncrement, 0, 1, FunctionCode.WriteSingleRegister, ModbusConnection.Action.ToWrite, 1, new byte[] { 0,3 });
            //var asdasda = (byte)FunctionCode.WriteMultipleCoils;
           // connectionEstablishment.SendMsgFormat(autoIncrement, 0, 1, FunctionCode.WriteMultipleCoils, ModbusConnection.Action.ToWrite, 20, new byte[] { 0, 10, 2, 205, 1 });
           // connectionEstablishment.SendMsgFormat(autoIncrement, 0, 1, FunctionCode.WriteMultipleRegisters, ModbusConnection.Action.ToWrite,2, new byte[] { 0, 2, 4, 0, 10, 1, 2 });
        }
    }
}
