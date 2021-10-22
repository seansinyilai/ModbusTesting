using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ModbusConnection
{
    public class ModbusMaster : ObservableObject
    {

        AutoResetEvent autoReset = new AutoResetEvent(false);
        TaskFactory taskFactory = new TaskFactory(new StaTaskScheduler(1));
        public event Action<bool> ConnectionStatusChanged;
        private BlockQueue<SendStruct> SendQueue;
        private BlockQueue<string> DealQueue;
        DispatcherTimer tikTok;
        TcpClient MasterClient;
        NetworkStream _streamFromServer = default;
        bool closed = false;
        bool _ToConnect;
        bool toSendFlag = false;
        ushort autoIncrement = 0;
        List<string> valueList;
        List<string> bitsList;
        List<string> discreteBitList;
        public bool ToConnect
        {
            get { return _ToConnect; }
            set
            {
                _ToConnect = value;
                NotifyPropertyChanged();
            }
        }
        private string _hostIP;

        public string HostIP
        {
            get { return _hostIP; }
            set
            {
                _hostIP = value;
                NotifyPropertyChanged();
            }
        }
        private int _port;

        public int Port
        {
            get { return _port; }
            set
            {
                _port = value;
                NotifyPropertyChanged();
            }
        }
        private string response;

        public string Response
        {
            get { return response; }
            set
            {
                response = value;
                NotifyPropertyChanged();
            }
        }

        public ModbusMaster(string hostIP, int port)
        {
            HostIP = hostIP;
            Port = port;
            SendQueue = new BlockQueue<SendStruct>();
            DealQueue = new BlockQueue<string>();
            Task.Factory.StartNew(SendMessage, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(ReadMessage, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(DealMessage, TaskCreationOptions.LongRunning);
            tikTok = new DispatcherTimer();
            tikTok.Tick += new EventHandler(timeCycle);
            tikTok.Interval = new TimeSpan(0, 0, 0, 10);
            tikTok.Start();

        }

        private void timeCycle(object sender, EventArgs e)
        {
            try
            {
                ushort temp = 1;
                var gotByteData = temp.SplitShortToHighAndLowByte();
                SendQueue.EnQueue(new SendStruct()
                {
                    TransactionID = 15,
                    ProtocolID = 0,
                    Address = 1,
                    FunctionCode = (byte)FunctionCode.ReadInputRegisters,
                    StartAddress = 0,
                    data = gotByteData,  /// 直接抓取實際數量
                    dataLength = ((ushort)gotByteData.Length).SplitShortToHighAndLowByte().Length,
                });
            }
            catch (Exception)
            {
                MasterClient.Client.Close();
                TcpToConnect(HostIP, Port);
            }
        }

        private void TcpToConnect(string hostIP, int port)
        {
            MasterClient = new TcpClient();
            MasterClient.ReceiveTimeout = 1000;
            MasterClient.SendTimeout = 1000;
            MasterClient.BeginConnect(IPAddress.Parse(hostIP),
                                        port, new AsyncCallback(ConnectCallBack), null);
        }
        private void SendMessage()
        {
            while (true)
            {
                // SpinWait.SpinUntil(() => false, 100);
                if (!toSendFlag)
                {
                    try
                    {
                        toSendFlag = true;
                        SendStruct GotMessage = SendQueue.DeQueue();
                        if (GotMessage != null) SENDRequest(GotMessage);
                    }
                    catch (Exception)
                    {

                        throw;
                    }

                }
            }
        }
        private void DealMessage()
        {
            while (true)
            {
                //     SpinWait.SpinUntil(() => false, 100);
                string GotMessage = DealQueue.DeQueue();
                if (GotMessage != null) ReceivedMsg(GotMessage);
            }
        }
        private void ReadMessage()
        {
            while (true)
            {
                if (!ToConnect && !closed)
                {
                    closed = true;
                    TcpToConnect(HostIP, Port);
                }
                if (MasterClient.Connected)
                {
                    if (MasterClient.Available > 0)
                    {
                        DateTime RecvTime = DateTime.Now;
                        _streamFromServer = MasterClient.GetStream();
                        byte[] buff = new byte[MasterClient.ReceiveBufferSize];
                        //MasterClient.Client.Receive(buff);
                        _streamFromServer.Read(buff, 0, buff.Length);
                        int length = buff[5];
                        byte[] datashow = new byte[length + 6];//定義所要顯示的接收的數據的長度  
                        for (int i = 0; i <= length + 5; i++)//將要顯示的數據存放到數組datashow中  
                        {
                            datashow[i] = buff[i];
                        }
                        byte[] myObjArray = new byte[datashow.Length];
                        Array.Copy(datashow, myObjArray, datashow.Length);
                        string stringdata = BitConverter.ToString(datashow);//把數組轉換成16
                        var ErrorResult = CheckingErrorCode(myObjArray[7], myObjArray[8]);
                        if (string.IsNullOrEmpty(ErrorResult) || string.IsNullOrWhiteSpace(ErrorResult))
                        {
                            //00-01-00-00-00-06-01-01-03-CD-6B-05
                            //00-02-00-00-00-06-01-02-03-AC-DB-35
                            //00-02-00-00-00-09-01-03-06-02-2B-00-00-00-64
                            //00-01-00-00-00-06-01-05-00-AD-FF-00
                            //00-01-00-00-00-06-01-06-00-01-00-03
                            //00-02-00-00-00-06-01-0F-00-14-00-0A
                            //00-02-00-00-00-06-01-10-00-02-00-02
                            //00-02-00-00-00-06-01-0F-00-14-00-28

                            var splitString = stringdata.Split('-');
                            int convertedFunctionCode = int.Parse(splitString[7], System.Globalization.NumberStyles.HexNumber);
                            var functionCode = (FunctionCode)convertedFunctionCode;
                            Response = string.Empty;
                            switch (functionCode)
                            {
                                case FunctionCode.ReadCoils:
                                    {
                                        int byteCount = int.Parse(splitString[8], System.Globalization.NumberStyles.HexNumber);
                                        var byteCountbits = Convert.ToString(byteCount, 2).PadLeft(8, '0');
                                        var quantityCount = Convert.ToInt32(byteCountbits, 2);
                                        if (bitsList != null) bitsList.Clear();
                                        bitsList = new List<string>();
                                        string temp = string.Empty;
                                        for (int i = 0; i < quantityCount; i++)
                                        {
                                            int outputStatusSection = int.Parse(splitString[9 + i], System.Globalization.NumberStyles.HexNumber);
                                            var outputStatusSectionBit = Convert.ToString(outputStatusSection, 2).PadLeft(8, '0').ReverseString();
                                            for (int j = 0; j < outputStatusSectionBit.Length; j++)
                                            {
                                                bitsList.Add(outputStatusSectionBit[j].ToString());
                                            };
                                        }
                                        bitsList.ForEach(bit =>
                                        {
                                            temp += bit;
                                        });
                                        Response = string.Format("Func:;{0};Bits:{1};Quantity:{2}", functionCode.ToString(), temp, quantityCount);
                                        Console.WriteLine(Response);
                                    }
                                    break;
                                case FunctionCode.ReadDiscreteInputs:
                                    {
                                        int byteCount = int.Parse(splitString[8], System.Globalization.NumberStyles.HexNumber);
                                        var byteCountbits = Convert.ToString(byteCount, 2).PadLeft(8, '0');
                                        var quantityCount = Convert.ToInt32(byteCountbits, 2);
                                        if (discreteBitList != null) discreteBitList.Clear();
                                        discreteBitList = new List<string>();
                                        string temp = string.Empty;
                                        for (int i = 0; i < quantityCount; i++)
                                        {
                                            int outputStatusSection = int.Parse(splitString[9 + i], System.Globalization.NumberStyles.HexNumber);
                                            var outputStatusSectionBit = Convert.ToString(outputStatusSection, 2).PadLeft(8, '0').ReverseString();
                                            for (int j = 0; j < outputStatusSectionBit.Length; j++)
                                            {
                                                discreteBitList.Add(outputStatusSectionBit[j].ToString());
                                            };
                                        }
                                        discreteBitList.ForEach(bit =>
                                        {
                                            temp += bit;
                                        });
                                        Response = string.Format("Func:;{0};Bits:{1};Quantity:{2}", functionCode.ToString(), temp, quantityCount);
                                        Console.WriteLine(Response);
                                    }
                                    break;
                                case FunctionCode.ReadHoldingRegisters:
                                    {
                                        int byteCount = int.Parse(splitString[8], System.Globalization.NumberStyles.HexNumber);
                                        if (valueList != null) valueList.Clear();
                                        valueList = new List<string>();
                                        int idx = 0;
                                        string listOfValues = string.Empty;
                                        for (int i = 0; i < byteCount / 2; i++)
                                        {
                                            int registerHigh = int.Parse(splitString[9 + i + idx], System.Globalization.NumberStyles.HexNumber);
                                            int registerLow = int.Parse(splitString[10 + i + idx], System.Globalization.NumberStyles.HexNumber);
                                            var registerHighBits = Convert.ToString(registerHigh, 2).PadLeft(8, '0');
                                            var registerLowBits = Convert.ToString(registerLow, 2).PadLeft(8, '0');
                                            var combinedBits = registerHighBits + registerLowBits;
                                            var value = Convert.ToInt32(combinedBits, 2).ToString();
                                            valueList.Add(value);
                                            idx++;
                                        }
                                        for (int i = 0; i < valueList.Count; i++)
                                        {
                                            listOfValues += valueList[i] + ";";
                                        }
                                        Response = string.Format("Func:;{0};Count:{1};listOfValues:{2}", functionCode.ToString(), byteCount.ToString(), listOfValues);
                                        Console.WriteLine(Response);
                                    }
                                    break;
                                case FunctionCode.WriteSingleCoil:
                                    {
                                        int outputAddressHigh = int.Parse(splitString[8], System.Globalization.NumberStyles.HexNumber);
                                        int outputAddressLow = int.Parse(splitString[9], System.Globalization.NumberStyles.HexNumber);
                                        int outputValueHigh = int.Parse(splitString[10], System.Globalization.NumberStyles.HexNumber);
                                        int outputValueLow = int.Parse(splitString[11], System.Globalization.NumberStyles.HexNumber);

                                        var outputValueHighbit = Convert.ToString(outputValueHigh, 2).PadLeft(8, '0');
                                        var outputValueLowbit = Convert.ToString(outputValueLow, 2).PadLeft(8, '0');
                                        var outputAddrHighbit = Convert.ToString(outputAddressHigh, 2).PadLeft(8, '0');
                                        var outputAddLowbit = Convert.ToString(outputAddressLow, 2).PadLeft(8, '0');

                                        var combinedOutputValueHigh = outputValueHighbit + outputValueLowbit;
                                        var combinedOutputAddr = outputAddrHighbit + outputAddLowbit;
                                        //var OutputValue = Convert.ToInt32(combinedOutputValueHigh, 2);
                                        var outputAddrOutput = Convert.ToInt32(combinedOutputAddr, 2);
                                        Response = string.Format("Func:;{0};OutputAddr:{1};OutputValue:{2}", functionCode.ToString(), outputAddrOutput, combinedOutputValueHigh);
                                        Console.WriteLine(Response);
                                    }
                                    break;
                                case FunctionCode.WriteSingleRegister:
                                    {
                                        int registerAddressHigh = int.Parse(splitString[8], System.Globalization.NumberStyles.HexNumber);
                                        int registerAddressLow = int.Parse(splitString[9], System.Globalization.NumberStyles.HexNumber);
                                        int registerValueHigh = int.Parse(splitString[10], System.Globalization.NumberStyles.HexNumber);
                                        int registerValueLow = int.Parse(splitString[11], System.Globalization.NumberStyles.HexNumber);

                                        var registerValueHighbit = Convert.ToString(registerValueHigh, 2).PadLeft(8, '0');
                                        var registerValueLowbit = Convert.ToString(registerValueLow, 2).PadLeft(8, '0');
                                        var registerAddrHighbit = Convert.ToString(registerAddressHigh, 2).PadLeft(8, '0');
                                        var registerAddLowbit = Convert.ToString(registerAddressLow, 2).PadLeft(8, '0');

                                        var combinedregisterValueHigh = registerValueHighbit + registerValueLowbit;
                                        var combinedregisterAddr = registerAddrHighbit + registerAddLowbit;
                                        // var registerValue = Convert.ToInt32(combinedregisterValueHigh, 2);
                                        var registerAddrregister = Convert.ToInt32(combinedregisterAddr, 2);
                                        Response = string.Format("Func:;{0};RegisterAddr:{1};RegisterValue:{2}", functionCode.ToString(), registerAddrregister, combinedregisterValueHigh);
                                        Console.WriteLine(Response);
                                    }
                                    break;
                                case FunctionCode.WriteMultipleCoils:
                                    {
                                        //00-02-00-00-00-06-01-0F-00-14-00-28
                                        int startAddressHigh = int.Parse(splitString[8], System.Globalization.NumberStyles.HexNumber);
                                        int startAddressLow = int.Parse(splitString[9], System.Globalization.NumberStyles.HexNumber);
                                        int quantityOutputHigh = int.Parse(splitString[10], System.Globalization.NumberStyles.HexNumber);
                                        int quantityOutputLow = int.Parse(splitString[11], System.Globalization.NumberStyles.HexNumber);

                                        var quantityHighbit = Convert.ToString(quantityOutputHigh, 2).PadLeft(8, '0');
                                        var quantityLowbit = Convert.ToString(quantityOutputLow, 2).PadLeft(8, '0');
                                        var startAddHighbit = Convert.ToString(startAddressHigh, 2).PadLeft(8, '0');
                                        var startAddLowbit = Convert.ToString(startAddressLow, 2).PadLeft(8, '0');

                                        var combinedQuantityHigh = quantityHighbit + quantityLowbit;
                                        var combinedStartAdd = startAddHighbit + startAddLowbit;
                                        var quantityOutput = Convert.ToInt32(combinedQuantityHigh, 2);
                                        var startAddOutput = Convert.ToInt32(combinedStartAdd, 2);
                                        Response = string.Format("Func:;{0};StartAddr:{1};Quantity:{2}", functionCode.ToString(), startAddOutput, quantityOutput);
                                        Console.WriteLine(Response);
                                    }
                                    break;
                                case FunctionCode.WriteMultipleRegisters:
                                    {
                                        int startAddressHigh = int.Parse(splitString[8], System.Globalization.NumberStyles.HexNumber);
                                        int startAddressLow = int.Parse(splitString[9], System.Globalization.NumberStyles.HexNumber);
                                        int quantityOfRegisterHigh = int.Parse(splitString[10], System.Globalization.NumberStyles.HexNumber);
                                        int quantityOfRegisterLow = int.Parse(splitString[11], System.Globalization.NumberStyles.HexNumber);

                                        var quantityHighbit = Convert.ToString(quantityOfRegisterHigh, 2).PadLeft(8, '0');
                                        var quantityLowbit = Convert.ToString(quantityOfRegisterLow, 2).PadLeft(8, '0');
                                        var startAddHighbit = Convert.ToString(startAddressHigh, 2).PadLeft(8, '0');
                                        var startAddLowbit = Convert.ToString(startAddressLow, 2).PadLeft(8, '0');

                                        var combinedQuantity = quantityHighbit + quantityLowbit;
                                        var combinedStartAdd = startAddHighbit + startAddLowbit;

                                        var quantityOutput = Convert.ToInt32(combinedQuantity, 2);
                                        var startAddOutput = Convert.ToInt32(combinedStartAdd, 2);
                                        Response = string.Format("Func:;{0};StartAddr:{1};Quantity:{2}", functionCode.ToString(), startAddOutput, quantityOutput);
                                        Console.WriteLine(Response);
                                    }
                                    break;
                                default:
                                    break;
                            }
                            toSendFlag = false;
                            if (!string.IsNullOrEmpty(Response) || !string.IsNullOrWhiteSpace(Response))
                            {
                                DealQueue.EnQueue(Response);
                                autoReset.Set();
                            }
                        }
                        else
                        {//ReadCoilsError;IllegalDataValue

                            DealQueue.EnQueue(ErrorResult);
                            toSendFlag = false;
                            if (!string.IsNullOrEmpty(Response) || !string.IsNullOrWhiteSpace(Response))
                            {
                                autoReset.Set();
                            }
                        }
                    }
                }
            }
        }
        /// <param name="transactionID">autoIncrement</param>
        /// <param name="protocolID">0 modbus</param>
        /// <param name="SlaveID">any</param>
        /// <param name="FunctionCode">what to do</param>
        /// <param name="StartAddress">buffer</param>
        /// <param name="data">data to send</param>
        public async Task<bool> SendWriteSingleCoilMsgFormat(byte SlaveID, ushort StartAddress, byte[] data)
        {
            await taskFactory.StartNew(() =>
            {

                autoIncrement++;
                SendQueue.EnQueue(new SendStruct()
                {
                    TransactionID = autoIncrement,
                    ProtocolID = 0,
                    Address = SlaveID,
                    FunctionCode = (byte)FunctionCode.WriteSingleCoil,
                    StartAddress = StartAddress,
                    data = data,                                  ///陣列長度
                    dataLength = data.Length,
                });
                autoReset.WaitOne();
            });
            return true;
            //string result;
            //int mLength = data.Length;
            //if (mLength != 2)
            //{
            //    result = "Length of data is invalid !!";
            //    return string.Format("Info: {0} Length:{1}", result, mLength.ToString());
            //}

            //result = "OK";
            //return result;
        }
        /// <param name="transactionID">autoIncrement</param>
        /// <param name="protocolID">0 modbus</param>
        /// <param name="SlaveID">any</param>
        /// <param name="FunctionCode">what to do</param>
        /// <param name="StartAddress">buffer</param>
        /// <param name="data">data to send</param>
        public async Task<bool> SendWriteSingleRegisterMsgFormat(byte SlaveID, ushort StartAddress, byte[] data)
        {
            await taskFactory.StartNew(() =>
            {
                autoIncrement++;
                SendQueue.EnQueue(new SendStruct()
                {
                    TransactionID = autoIncrement,
                    ProtocolID = 0,
                    Address = SlaveID,
                    FunctionCode = (byte)FunctionCode.WriteSingleRegister,
                    StartAddress = StartAddress,
                    data = data,                                  ///陣列長度
                    dataLength = data.Length,
                });
                autoReset.WaitOne();
            });
            return true;
            //    string result;
            //int mLength = data.Length;
            //if (mLength != 2)
            //{
            //    result = "Length of data is invalid !!";
            //    return string.Format("Info: {0} Length:{1}", result, mLength.ToString());
            //}

            //result = "OK";
            //return result;
        }
        /// <param name="transactionID">autoIncrement</param>
        /// <param name="protocolID">0 modbus</param>
        /// <param name="SlaveID">any</param>
        /// <param name="FunctionCode">what to do</param>
        /// <param name="StartAddress">buffer</param>
        /// <param name="data">data to send</param>
        public async Task<bool> SendWriteMultipleCoilsMsgFormat(byte SlaveID, ushort StartAddress, byte[] outPutQuantityHighLowBit, params byte[][] multiOutputData)
        {
            await taskFactory.StartNew(() =>
            {
                string result;
                int mLength = outPutQuantityHighLowBit.Length;
                if (mLength != 2)
                {
                    result = "Length of data is invalid !!";
                    // return string.Format("Info: {0} Length:{1}", result, mLength.ToString());
                }
                for (int y = 0; y < multiOutputData.Count(); y++)
                {
                    int mmLength = multiOutputData[y].Length;
                    if (multiOutputData[y].Length != 2)
                    {
                        result = "Length of data is invalid !!";
                        //   return string.Format("Info: {0} Length:{1}", result, mmLength.ToString());
                    }
                }
                autoIncrement++;
                var highbit = Convert.ToString(outPutQuantityHighLowBit[0], 2).PadLeft(8, '0');
                var lowbit = Convert.ToString(outPutQuantityHighLowBit[1], 2).PadLeft(8, '0');
                var combined = highbit + lowbit;
                var quantityOutput = Convert.ToInt32(combined, 2);
                var N = quantityOutput / 8;
                var remainder = quantityOutput % 8;
                if (remainder != 0)
                {
                    N += 1;
                }
                //if (quantityOutput < (multiOutputData.Count() * 2 * 8))  /// 8 是8個bits
                //{
                //    result = "Length of data is invalid !!";
                //    return string.Format("Info: {0} Length:{1}", result, mLength.ToString());
                //}
                byte[] data = new byte[3 + N + 1];
                int i = 0;
                for (; i < outPutQuantityHighLowBit.Length; i++)
                {
                    data[i] = outPutQuantityHighLowBit[i];
                }
                data[i] = Convert.ToByte(N);
                i++;
                for (int y = 0; y < multiOutputData.Count(); y++)
                {
                    for (int x = 0; x < multiOutputData[y].Length; x++)
                    {
                        data[i] = multiOutputData[y][x];
                        i++;
                    }
                }
                SendQueue.EnQueue(new SendStruct()
                {
                    TransactionID = autoIncrement,
                    ProtocolID = 0,
                    Address = SlaveID,
                    FunctionCode = (byte)FunctionCode.WriteMultipleCoils,
                    StartAddress = StartAddress,
                    data = data,                                  ///陣列長度
                    dataLength = data.Length,
                });
                autoReset.WaitOne();

            });
            return true;
            //result = "OK";
            //return result;
        }

        /// <param name="transactionID">autoIncrement</param>
        /// <param name="protocolID">0 modbus</param>
        /// <param name="SlaveID">any</param>
        /// <param name="FunctionCode">what to do</param>
        /// <param name="StartAddress">buffer</param>
        /// <param name="data">data to send</param>
        public async Task<bool> SendWriteMultipleRegistersMsgFormat(byte SlaveID, ushort StartAddress, byte[] quantityHighLowBit, params byte[][] multipleData)
        {
            await taskFactory.StartNew(() =>
            {
                string result;
                int mLength = quantityHighLowBit.Length;
                if (mLength != 2)
                {
                    result = "Length of data is invalid !!";
                    //  return string.Format("Info: {0} Length:{1}", result, mLength.ToString());
                }
                for (int y = 0; y < multipleData.Count(); y++)
                {
                    int mmLength = multipleData[y].Length;
                    if (multipleData[y].Length != 2)
                    {
                        result = "Length of data is invalid !!";
                        //    return string.Format("Info: {0} Length:{1}", result, mmLength.ToString());
                    }
                }
                autoIncrement++;
                var highbit = Convert.ToString(quantityHighLowBit[0], 2).PadLeft(8, '0');
                var lowbit = Convert.ToString(quantityHighLowBit[1], 2).PadLeft(8, '0');
                var combined = highbit + lowbit;
                var final = Convert.ToInt32(combined, 2);
                byte[] data = new byte[(2 * multipleData.Count()) + 3];
                int i = 0;
                for (; i < quantityHighLowBit.Length; i++)
                {
                    data[i] = quantityHighLowBit[i];
                }
                data[i] = Convert.ToByte(2 * final);
                i++;
                for (int y = 0; y < multipleData.Count(); y++)
                {
                    for (int x = 0; x < multipleData[y].Length; x++)
                    {
                        data[i] = multipleData[y][x];
                        i++;
                    }
                }

                SendQueue.EnQueue(new SendStruct()
                {
                    TransactionID = autoIncrement,
                    ProtocolID = 0,
                    Address = SlaveID,
                    FunctionCode = (byte)FunctionCode.WriteMultipleRegisters,
                    StartAddress = StartAddress,
                    data = data,                                  ///陣列長度
                    dataLength = data.Length,
                });
                autoReset.WaitOne();
            });
            return true;
            //result = "OK";
            //return result;
        }

        /// <param name="transactionID">autoIncrement</param>
        /// <param name="protocolID">0 modbus</param>
        /// <param name="SlaveID">any</param>
        /// <param name="FunctionCode">what to do</param>
        /// <param name="StartAddress">buffer</param>
        /// <param name="data">data to send</param>
        public async Task<bool> ReadCoilsCommand_SendMsgFormat(byte SlaveID, ushort StartAddress, ushort numberOfDataToRead)
        {
            await taskFactory.StartNew(() =>
            {
                autoIncrement++;
                var gotByteData = numberOfDataToRead.SplitShortToHighAndLowByte();
                SendQueue.EnQueue(new SendStruct()
                {
                    TransactionID = autoIncrement,
                    ProtocolID = 0,
                    Address = SlaveID,
                    FunctionCode = (int)FunctionCode.ReadCoils,
                    StartAddress = StartAddress,
                    data = gotByteData,  /// 直接抓取實際數量
                    dataLength = ((ushort)gotByteData.Length).SplitShortToHighAndLowByte().Length,
                });
                autoReset.WaitOne();
            });
            return true;
            //  return "OK";
        }
        /// <param name="transactionID">autoIncrement</param>
        /// <param name="protocolID">0 modbus</param>
        /// <param name="SlaveID">any</param>
        /// <param name="FunctionCode">what to do</param>
        /// <param name="StartAddress">buffer</param>
        /// <param name="data">data to send</param>
        public async Task<bool> ReadDiscreteInputs_SendMsgFormat(byte SlaveID, ushort StartAddress, ushort numberOfDataToRead)
        {
            await taskFactory.StartNew(() =>
            {
                autoIncrement++;
                var gotByteData = numberOfDataToRead.SplitShortToHighAndLowByte();
                SendQueue.EnQueue(new SendStruct()
                {
                    TransactionID = autoIncrement,
                    ProtocolID = 0,
                    Address = SlaveID,
                    FunctionCode = (int)FunctionCode.ReadDiscreteInputs,
                    StartAddress = StartAddress,
                    data = gotByteData,  /// 直接抓取實際數量
                    dataLength = ((ushort)gotByteData.Length).SplitShortToHighAndLowByte().Length,
                });
                autoReset.WaitOne();
            });
            return true;
        }
        /// <param name="transactionID">autoIncrement</param>
        /// <param name="protocolID">0 modbus</param>
        /// <param name="SlaveID">any</param>
        /// <param name="FunctionCode">what to do</param>
        /// <param name="StartAddress">buffer</param>
        /// <param name="data">data to send</param>
        public async Task<bool> ReadHoldingRegister_SendMsgFormat(byte SlaveID, ushort StartAddress, ushort numberOfDataToRead)
        {
            await taskFactory.StartNew(() =>
            {
                autoIncrement++;
                var gotByteData = numberOfDataToRead.SplitShortToHighAndLowByte();
                SendQueue.EnQueue(new SendStruct()
                {
                    TransactionID = autoIncrement,
                    ProtocolID = 0,
                    Address = SlaveID,
                    FunctionCode = (int)FunctionCode.ReadHoldingRegisters,
                    StartAddress = StartAddress,
                    data = gotByteData,  /// 直接抓取實際數量
                    dataLength = ((ushort)gotByteData.Length).SplitShortToHighAndLowByte().Length,
                });
                autoReset.WaitOne();
            });
            return true;
        }
        public void SENDRequest(SendStruct obj)
        {
            byte[] lengthTotal = null;
            var highAndLowBit = obj.TransactionID.SplitShortToHighAndLowByte();
            var highAndLowBitProtocol = obj.ProtocolID.SplitShortToHighAndLowByte();
            var highAndLowBitAddress = new byte[] { Convert.ToByte(obj.Address) };
            var highAndLowBitFunction = new byte[] { Convert.ToByte(obj.FunctionCode) };
            var highAndLowBitStartAddress = obj.StartAddress.SplitShortToHighAndLowByte();
            lengthTotal = ((ushort)(highAndLowBitAddress.Length + highAndLowBitFunction.Length + highAndLowBitStartAddress.Length + obj.dataLength)).SplitShortToHighAndLowByte();//+ dataCount.Length obj.data.Length

            List<byte[]> tmp = new List<byte[]>();
            List<byte> tmpByteArray = new List<byte>();
            var result = Header_PDU_Data(new CommandStruct()
            {
                transactionID = highAndLowBit,
                protocolID = highAndLowBitProtocol,
                length = lengthTotal,
                Address = highAndLowBitAddress,
                FunctionCode = highAndLowBitFunction,
                StartAddress = highAndLowBitStartAddress,
                DataCount = obj.data,
            });

            result.ForEach(x =>
            {
                tmp.Add(x.Item2);
            });

            tmp.ForEach(x =>
            {
                for (int j = 0; j < x.Length; j++)
                {
                    tmpByteArray.Add(x[j]);
                }
            });

            _streamFromServer = MasterClient.GetStream();
            byte[] dataOutStream = tmpByteArray.ToArray();
            _streamFromServer.WriteAsync(dataOutStream, 0, dataOutStream.Length);
            _streamFromServer.Flush();
        }
        private List<Tuple<string, byte[]>> Header_PDU_Data(CommandStruct obj)
        {
            List<Tuple<string, byte[]>> headerList = new List<Tuple<string, byte[]>>();
            headerList.Add(Tuple.Create(StaticVarSharedClass.TransactionID, obj.transactionID));
            headerList.Add(Tuple.Create(StaticVarSharedClass.ProtocalID, obj.protocolID));
            headerList.Add(Tuple.Create(StaticVarSharedClass.LengthFromAddressStartToDataEnd, obj.length));
            headerList.Add(Tuple.Create(StaticVarSharedClass.Address, obj.Address));
            headerList.Add(Tuple.Create(StaticVarSharedClass.FunctionCode, obj.FunctionCode));
            headerList.Add(Tuple.Create(StaticVarSharedClass.StartRegisterAdd, obj.StartAddress));
            headerList.Add(Tuple.Create(StaticVarSharedClass.DataCount, obj.DataCount));
            return headerList;
        }
        private void ConnectCallBack(IAsyncResult ar)
        {
            ToConnect = ar.AsyncWaitHandle.WaitOne(100, true);
            if (MasterClient.Connected && ToConnect)
            {
                ToConnect = true;
                ConnectionStatusChanged?.Invoke(ToConnect);
            }
            else
            {
                _streamFromServer?.Close();
                closed = false;
                ToConnect = false;
                ConnectionStatusChanged?.Invoke(ToConnect);
            }
        }
        private string CheckingErrorCode(byte er, byte ex)
        {
            bool ecFlag = false;
            string exString = string.Empty;
            switch ((ErrorCode)er)
            {
                case ErrorCode.ReadCoilsError:
                    {
                        ecFlag = true;
                        var result = CheckingException(ex);
                        if (!string.IsNullOrEmpty(result) && !string.IsNullOrWhiteSpace(result))
                            exString = result;
                        else
                            exString = string.Empty;
                    }
                    break;
                case ErrorCode.ReadDiscreteInputsError:
                    {
                        ecFlag = true;
                        var result = CheckingException(ex);
                        if (!string.IsNullOrEmpty(result) && !string.IsNullOrWhiteSpace(result))
                            exString = result;
                        else
                            exString = string.Empty;
                    }
                    break;
                case ErrorCode.ReadHoldingRegistersError:
                    {
                        ecFlag = true;
                        var result = CheckingException(ex);
                        if (!string.IsNullOrEmpty(result) && !string.IsNullOrWhiteSpace(result))
                            exString = result;
                        else
                            exString = string.Empty;
                    }
                    break;
                case ErrorCode.ReadInputRegistersError:
                    {
                        ecFlag = true;
                        var result = CheckingException(ex);
                        if (!string.IsNullOrEmpty(result) && !string.IsNullOrWhiteSpace(result))
                            exString = result;
                        else
                            exString = string.Empty;
                    }
                    break;
                case ErrorCode.WriteSingleCoilError:
                    {
                        ecFlag = true;
                        var result = CheckingException(ex);
                        if (!string.IsNullOrEmpty(result) && !string.IsNullOrWhiteSpace(result))
                            exString = result;
                        else
                            exString = string.Empty;
                    }
                    break;
                case ErrorCode.WriteSingleRegisterError:
                    {
                        ecFlag = true;
                        var result = CheckingException(ex);
                        if (!string.IsNullOrEmpty(result) && !string.IsNullOrWhiteSpace(result))
                            exString = result;
                        else
                            exString = string.Empty;
                    }
                    break;
                case ErrorCode.WriteMultipleCoilsError:
                    {
                        ecFlag = true;
                        var result = CheckingException(ex);
                        if (!string.IsNullOrEmpty(result) && !string.IsNullOrWhiteSpace(result))
                            exString = result;
                        else
                            exString = string.Empty;
                    }
                    break;
                case ErrorCode.WriteMultipleRegistersError:
                    {
                        ecFlag = true;
                        var result = CheckingException(ex);
                        if (!string.IsNullOrEmpty(result) && !string.IsNullOrWhiteSpace(result))
                            exString = result;
                        else
                            exString = string.Empty;
                    }
                    break;
                default:
                    break;
            }
            if (ecFlag)
            {
                return string.Format("{0};{1}", ((ErrorCode)er).ToString(), exString);
            }
            else
            {
                return string.Empty;
            }
        }
        private string CheckingException(byte ex)
        {
            bool exFlag = false;
            switch ((ExceptionCode)ex)
            {
                case ExceptionCode.IllegalFunction:
                    {
                        exFlag = true;
                    }
                    break;
                case ExceptionCode.IllegalDataAddress:
                    {
                        exFlag = true;
                    }
                    break;
                case ExceptionCode.IllegalDataValue:
                    {
                        exFlag = true;
                    }
                    break;
                case ExceptionCode.IllegalServerDeviceFailure:
                    {
                        exFlag = true;
                    }
                    break;
                case ExceptionCode.Acknowledge:
                    {
                        exFlag = true;
                    }
                    break;
                case ExceptionCode.ServerDeviceBusy:
                    {
                        exFlag = true;
                    }
                    break;
                case ExceptionCode.MemoryParityError:
                    {
                        exFlag = true;
                    }
                    break;
                case ExceptionCode.GateWayPathUnavailable:
                    {
                        exFlag = true;
                    }
                    break;
                case ExceptionCode.GateWayTargetDeviceFailedToRespond:
                    {
                        exFlag = true;
                    }
                    break;
                default:
                    break;
            }
            if (exFlag)
            {
                return ((ExceptionCode)ex).ToString();
            }
            else
            {
                return string.Empty;
            }
        }
        public virtual bool ReceivedMsg(string msg)
        {
            return false;
        }
    }
}
