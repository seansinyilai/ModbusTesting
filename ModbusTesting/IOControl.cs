using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModbusConnection;

namespace ModbusTesting
{
    public abstract class IOControl : ModbusMaster
    {
        public event Action<bool> ConnectStatusChanged;
        private int idx = 0;
        private string _response;

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

        public IOControl(string hostIP, int port) : base(hostIP, port,"IOControl")
        {
            ResponseResult = string.Empty;
            ConnectionStatusChanged += (connnectionStatus) =>
            {
                ConnectionStatus = connnectionStatus;
                ConnectStatusChanged?.Invoke(connnectionStatus);
            };
        }

        public virtual async Task<bool> ReadDIsAsync(byte SlaveID)
        {
            var result = await ReadCoilsCommand_SendMsgFormat(SlaveID, 0, 8);
            return result;
        }
        public virtual async Task<bool> AllLightOnAsync(byte SlaveID)
        {
            var result = await SendWriteMultipleCoilsMsgFormat(SlaveID, 19, new byte[] { 0, 2 }, new byte[] { 255, 0 });
            return result;
        }
        public virtual async Task<bool> WriteGreenLightOnAsync(byte SlaveID)
        {
            var result = await SendWriteSingleCoilMsgFormat(SlaveID, 16, new byte[] { 255, 0 });
            return result;
        }
        public virtual async Task<bool> WriteYellowLightOnAsync(byte SlaveID)
        {
            var result = await SendWriteSingleCoilMsgFormat(SlaveID, 17, new byte[] { 255, 0 });
            return result;
        }
        public virtual async Task<bool> WriteRedLightOnAsync(byte SlaveID)
        {
            var result = await SendWriteSingleCoilMsgFormat(SlaveID, 18, new byte[] { 255, 0 });
            return result;
        }
        public virtual async Task<bool> WriteBuzzerOnAsync(byte SlaveID)
        {
            var result = await SendWriteSingleCoilMsgFormat(SlaveID, 19, new byte[] { 255, 0 });
            return result;
        }
        public virtual async Task<bool> WriteGreenLightOffAsync(byte SlaveID)
        {
            var result = await SendWriteSingleCoilMsgFormat(SlaveID, 16, new byte[] { 0, 0 });
            return result;
        }
        public virtual async Task<bool> WriteYellowLightOffAsync(byte SlaveID)
        {
            var result = await SendWriteSingleCoilMsgFormat(SlaveID, 17, new byte[] { 0, 0 });
            return result;
        }
        public virtual async Task<bool> WriteRedLightOffAsync(byte SlaveID)
        {
            var result = await SendWriteSingleCoilMsgFormat(SlaveID, 18, new byte[] { 0, 0 });
            return result;
        }
        public virtual async Task<bool> WriteBuzzerOffAsync(byte SlaveID)
        {
            var result = await SendWriteSingleCoilMsgFormat(SlaveID, 19, new byte[] { 0, 0 });
            return result;
        }
        public virtual async Task<bool> AllLightOffAsync(byte SlaveID)
        {
            var result = await SendWriteMultipleCoilsMsgFormat(SlaveID, 19, new byte[] { 0, 2 }, new byte[] { 0, 0 });
            return result;
        }
        public virtual async Task<bool> ReadAllLightsAsync(byte SlaveID)
        {
            var result = await ReadCoilsCommand_SendMsgFormat(SlaveID, 16, 8);
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
                ResponseResult += "No: IO " + idx + msg + "\n";
                // ResponseResult += "IOControl;" + msg + "\n";
                ReceivedCallBackMsg(ResponseResult);
            }
            return true;
        }
        public abstract bool ReceivedCallBackMsg(string msg);
    }
}
