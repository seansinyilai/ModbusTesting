using ModbusConnection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusTesting
{
    public abstract class TemperatureControl : ModbusMaster
    {
        AutoResetEvent mResetEvent = new AutoResetEvent(false);
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

        public TemperatureControl(string hostIP, int port) : base(hostIP, port, "TempControl")
        {
            ResponseResult = string.Empty;
            ConnectionStatusChanged += (connnectionStatus) =>
            {
                ConnectionStatus = connnectionStatus;
                ConnectStatusChanged?.Invoke(connnectionStatus);
            };
        }
        public virtual async Task<bool> SetPZ900BufferPointsAsync(byte SlaveID, List<TempRecipeStruct> listOfStruck)
        {
            ushort shiftingIdx = 0;
            bool result = false;
            ushort mPoints = 0;
            byte[] value1 = new byte[2];
            byte[] value2 = new byte[2];
            if (listOfStruck.GetType() != typeof(List<TempRecipeStruct>))
            {
                return false;
            }
            listOfStruck.ForEach(async tmpStruct =>
             {
                 mPoints = Convert.ToUInt16(12288 + shiftingIdx);
                 shiftingIdx += 3;
                 value1 = tmpStruct.Temperature.SplitShortToHighAndLowByte();
                 value2 = tmpStruct.TempTime.SplitShortToHighAndLowByte();
                 result = await SendWriteMultipleRegistersMsgFormat(SlaveID, mPoints, new byte[] { 0, 2 }, value1, value2);
                
             });
            var result2 = await SetPZ900ModeEnd(2, Convert.ToByte(listOfStruck.Count()));
            return result && result2;
        }
        public virtual async Task<bool> ReadPZ900BufferPointsAsync(byte SlaveID)
        {
            var result = await ReadHoldingRegister_SendMsgFormat(SlaveID, 12288, 30);
            return result ;
        }
        public virtual async Task<bool> ReadPZ900ModeEnd(byte SlaveID)
        {
            var result = await ReadHoldingRegister_SendMsgFormat(SlaveID, 20480, 1);
            return result;
        }
        public virtual async Task<bool> SetPZ900ModeEnd(byte SlaveID, byte data)
        {
            if (data.GetType() != typeof(byte))
            {
                return false;
            }
            byte[] tmp = new byte[2];
            string binartsaa = Convert.ToString(data, 2).PadLeft(8, '0');
            string highbit = binartsaa.Substring(0, 4);
            string lowbit = binartsaa.Substring(4, 4);
            tmp[0] = Convert.ToByte(Convert.ToInt32(highbit, 2));
            tmp[1] = Convert.ToByte(Convert.ToInt32(lowbit, 2));
            var result = await SendWriteSingleRegisterMsgFormat(SlaveID, 20480, tmp);
            return result;
        }
        public virtual async Task<bool> ReadPZ900PointsAsync(byte SlaveID)
        {
            var result = await ReadHoldingRegister_SendMsgFormat(SlaveID, 0, 20);
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
