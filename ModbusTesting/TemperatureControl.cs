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
        /// <summary>
        /// 給予一個list的溫度以及時間參數
        /// 依照參數設定Recipe
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <param name="listOfStruck"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 讀取 Recipe1 所有區段點位
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        public virtual async Task<bool> ReadPZ900BufferPointsAsync(byte SlaveID)
        {
            var result = await ReadHoldingRegister_SendMsgFormat(SlaveID, 12288, 48);
            return result ;
        }
        /// <summary>
        /// 讀取段數設定點位
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        public virtual async Task<bool> ReadPZ900ModeEnd(byte SlaveID)
        {
            var result = await ReadHoldingRegister_SendMsgFormat(SlaveID, 20480, 1);
            return result;
        }
        /// <summary>
        /// 設定段數
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <param name="data"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 讀取溫控器前20點位
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        public virtual async Task<bool> ReadPZ900PointsAsync(byte SlaveID)
        {
            var result = await ReadHoldingRegister_SendMsgFormat(SlaveID, 0, 20);
            return result;
        }
        public virtual async Task<bool> ReadPZ900AllPointsAsync(byte SlaveID)
        {
            var result = await ReadHoldingRegister_SendMsgFormat(SlaveID, 100, 100);
            return result;
        }
        /// <summary>
        /// Test用
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        public virtual async Task<bool> AllLightOffAsync(byte SlaveID)
        {
            var result = await SendWriteMultipleCoilsMsgFormat(SlaveID, 16, new byte[] { 0, 2 }, new byte[] { 0, 0 });
            return result;
            // return false;
        }

        /// <summary>
        /// Test用
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        public virtual async Task<bool> ReadDIsAsync(byte SlaveID)
        {
            //  var result = await ReadCoilsCommand_SendMsgFormat(SlaveID, 0, 8);
            return false;
        }
        /// <summary>
        /// Test用
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        public virtual async Task<bool> ReadAllLightsAsync(byte SlaveID)
        {
            var result = await ReadCoilsCommand_SendMsgFormat(SlaveID, 16, 8);
            return result;
            //return false;
        }
        /// <summary>
        /// Test用
        /// </summary>
        /// <param name="SlaveID"></param>
        /// <returns></returns>
        public virtual async Task<bool> AllLightOnAsync(byte SlaveID)
        {
            var result = await SendWriteMultipleCoilsMsgFormat(SlaveID, 16, new byte[] { 0, 2 }, new byte[] { 255, 0 });
            return result;
            //return false;
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
                //ResponseResult += "No: temp " + idx + msg + "\n";
                ResponseResult += "TemperatureControl;" + msg + "\n";
                //ResponseResult = string.Empty;
                ReceivedCallBackMsg(ResponseResult);
            }
            return true;
        }
        public abstract bool ReceivedCallBackMsg(string msg);
    }
}
