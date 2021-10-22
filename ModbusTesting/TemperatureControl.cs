using ModbusConnection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusTesting
{
    public abstract class TemperatureControl : ModbusMaster
    {
        public event Action<bool> ConnectStatusChanged;
        private string _response;
          private int idx = 0;

        public string ResponseResult
        {
            get { return _response; }
            set
            {
                _response = value;
                NotifyPropertyChanged();
            }
        }
        private bool _ConnectionStatus;

        public bool ConnectionStatus
        {
            get { return _ConnectionStatus; }
            set
            {
                _ConnectionStatus = value;
                NotifyPropertyChanged();
            }
        }

        public TemperatureControl(string hostIP, int port) : base(hostIP, port)
        {
            ResponseResult = string.Empty;
            ConnectionStatusChanged += (connnectionStatus) =>
            {
                ConnectionStatus = connnectionStatus;
                ConnectStatusChanged?.Invoke(connnectionStatus);
            };
        }
        public virtual async Task<bool> AllLightOffAsync(byte SlaveID)
        {
           var result = await SendWriteMultipleCoilsMsgFormat(SlaveID, 16, new byte[] { 0, 2 }, new byte[] { 0, 0 });
            return result;
        }
        public virtual async Task<bool> ReadDIsAsync(byte SlaveID)
        {
          var result = await  ReadCoilsCommand_SendMsgFormat(SlaveID, 0, 8);
            return result;
        }
        public virtual async Task<bool> AllLightOnAsync(byte SlaveID)
        {
            var result = await  SendWriteMultipleCoilsMsgFormat(SlaveID, 16, new byte[] { 0, 2 }, new byte[] { 255, 0 });
            return result;
        }
        public virtual async Task<bool> ReadPZ900PointsAsync(byte SlaveID)
        {
           var result = await  ReadHoldingRegister_SendMsgFormat(SlaveID, 0, 20);
            return result;
        }
        public override bool ReceivedMsg(string msg)
        {
            if (!string.IsNullOrEmpty(msg) || !string.IsNullOrWhiteSpace(msg))
            {
                if (!msg.Contains("Error"))
                {
                    var stringArray = msg.Split(';');
                    var returnFunctionCode = (FunctionCode)Enum.Parse(typeof(FunctionCode), stringArray[1], true);

                    switch (returnFunctionCode)
                    {
                        case FunctionCode.ReadCoils:
                            break;
                        case FunctionCode.ReadDiscreteInputs:
                            break;
                        case FunctionCode.ReadHoldingRegisters:
                            break;
                        case FunctionCode.ReadInputRegisters:
                            break;
                        case FunctionCode.WriteSingleCoil:
                            break;
                        case FunctionCode.WriteSingleRegister:
                            break;
                        case FunctionCode.WriteMultipleCoils:
                            break;
                        case FunctionCode.WriteMultipleRegisters:
                            break;
                    }

                    if (ResponseResult.Length > 5000)
                    {
                        ResponseResult = string.Empty;
                    }
                }
                idx++;
                ResponseResult += "No: temp " + idx + msg + "\n";
               // ResponseResult += "TemperatureControl;" + msg + "\n";
                ReceivedCallBackMsg(ResponseResult);
            }
            return true;
        }
        public abstract bool ReceivedCallBackMsg(string msg);
    }
}
