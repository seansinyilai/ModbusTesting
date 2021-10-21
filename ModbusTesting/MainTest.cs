using ModbusConnection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusTesting
{
    public class MainTest : ObservableObject
    {
        public RelayCommand ReadCoilsSEND { get; set; }
        public RelayCommand ReadDiscreteInputsSEND { get; set; }
        public RelayCommand ReadHoldingRegistersSEND { get; set; }
        public RelayCommand SendWriteSingleCoilSEND { get; set; }
        public RelayCommand SendWriteSingleRegisterSEND { get; set; }
        public RelayCommand SendWriteMultipleCoilsSEND { get; set; }
        public RelayCommand SendWriteMultipleRegistersSEND { get; set; }
        public ConnecTemp ConnectionEstablishment { get; set; }
        public MainTest()
        {
            ConnectionEstablishment = new ConnecTemp("192.168.0.112", 502);
            //ConnectionEstablishment = new ConnecTemp("127.0.0.1", 502);
            ReadCoilsSEND = new RelayCommand(ReadCoils, param => true);
            ReadDiscreteInputsSEND = new RelayCommand(ReadDiscreteInputs, param => true);
            ReadHoldingRegistersSEND = new RelayCommand(ReadHoldingRegisters, param => true);
            SendWriteSingleCoilSEND = new RelayCommand(SendWriteSingleCoil, param => true);
            SendWriteSingleRegisterSEND = new RelayCommand(SendWriteSingleRegister, param => true);
            SendWriteMultipleCoilsSEND = new RelayCommand(SendWriteMultipleCoils, param => true);
            SendWriteMultipleRegistersSEND = new RelayCommand(SendWriteMultipleRegisters, param => true);
        }

        private void SendWriteMultipleRegisters(object obj)
        {
            //ConnectionEstablishment.SendWriteMultipleRegistersMsgFormat(1, 2, new byte[] { 0, 3 }, new byte[] { 0, 10 }, new byte[] { 1, 2 }, new byte[] { 1, 2 });
            ConnectionEstablishment.SendWriteMultipleRegistersMsgFormat(2, 15, new byte[] { 0, 3 }, new byte[] { 0, 10 }, new byte[] { 0, 2 }, new byte[] {0, 2 });

        }

        private void SendWriteMultipleCoils(object obj)
        {
            //ConnectionEstablishment.SendWriteMultipleCoilsMsgFormat(1, 20, new byte[] { 0, 40 }, new byte[] { 205, 01 }, new byte[] { 125, 20 });
         var asdf=   ConnectionEstablishment.SendWriteMultipleCoilsMsgFormat(1, 16, new byte[] { 0, 16 }, new byte[] { 205, 01 });
        }

        private void SendWriteSingleRegister(object obj)
        {
            ConnectionEstablishment.SendWriteSingleRegisterMsgFormat(2, 1, new byte[] { 0, 3 });
        }

        private void SendWriteSingleCoil(object obj)
        {
            ConnectionEstablishment.SendWriteSingleCoilMsgFormat(1, 17, new byte[] {0, 0 });
        }

        private void ReadHoldingRegisters(object obj)
        {
            //ConnectionEstablishment.ReadHoldingRegister_SendMsgFormat(1, 10001, 3);
            //ConnectionEstablishment.ReadHoldingRegister_SendMsgFormat(1, 108, 3);
            //ConnectionEstablishment.ReadHoldingRegister_SendMsgFormat(1, 0, 3);
            ConnectionEstablishment.ReadHoldingRegister_SendMsgFormat(2, 15,3);
        }

        private void ReadDiscreteInputs(object obj)
        {
            ConnectionEstablishment.ReadDiscreteInputs_SendMsgFormat(1, 197, 22);
        }

        private void ReadCoils(object e)
        {
            ConnectionEstablishment.ReadCoilsCommand_SendMsgFormat(1, 16, 9);
        }
    }
}
