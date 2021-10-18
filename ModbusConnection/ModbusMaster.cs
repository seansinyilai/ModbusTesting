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
    public class ModbusMaster
    {
        DispatcherTimer tikTok;
        Task RunReadMessageThread;
        TcpClient MasterClient;
        NetworkStream _streamFromServer = default;
        bool closed = false;
        int autoIncrement = 0;
        private bool _ToConnect;

        public bool ToConnect
        {
            get { return _ToConnect; }
            set { _ToConnect = value; }
        }
        private string _hostIP;

        public string HostIP
        {
            get { return _hostIP; }
            set { _hostIP = value; }
        }
        private int _port;

        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }

        public ModbusMaster(string hostIP, int port)
        {
            HostIP = hostIP;
            Port = port;
            RunReadMessageThread = new Task(ReadMessage);
            RunReadMessageThread.Start();
            tikTok = new DispatcherTimer();
            tikTok.Tick += new EventHandler(timeCycle);
            tikTok.Interval = new TimeSpan(0, 0, 0, 10);
            tikTok.Start();

        }

        private void timeCycle(object sender, EventArgs e)
        {
            byte[] data = new byte[] { 0x00, 0x0f, 0x00, 0x00, 0x00, 0x06, 0x01, 0x04, 0x00, 0x00, 0x00, 0x01 };
            //    MasterClient.Client.Send(data);
        }

        private void TcpToConnect(string hostIP, int port)
        {
            MasterClient = new TcpClient();
            MasterClient.ReceiveTimeout = 1000;
            MasterClient.SendTimeout = 1000;
            MasterClient.BeginConnect(IPAddress.Parse(hostIP),
                                        port, new AsyncCallback(ConnectCallBack), null);
        }

        private void ReadMessage()
        {
            while (true)
            {
                try
                {
                    SpinWait.SpinUntil(() => false, 100);
                    if (!ToConnect && !closed)
                    {
                        closed = true;
                        TcpToConnect(HostIP, Port);
                    }
                    if (MasterClient.Connected)
                    {
                        if (MasterClient.Available > 0)
                        {   // var c = _streamFromServer.Read(buff, 0, buff.Length);
                            // string temp = Encoding.ASCII.GetString(buff, 0, buff.Length).Trim((char)0);
                            DateTime RecvTime = DateTime.Now;
                            _streamFromServer = MasterClient.GetStream();
                            byte[] buff = new byte[MasterClient.ReceiveBufferSize];
                            MasterClient.Client.Receive(buff);
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
                                string response = string.Empty;
                                switch (functionCode)
                                {
                                    case FunctionCode.ReadCoils:
                                        {
                                            int byteCount = int.Parse(splitString[8], System.Globalization.NumberStyles.HexNumber);
                                            int outputStatusSection1 = int.Parse(splitString[9], System.Globalization.NumberStyles.HexNumber);
                                            int outputStatusSection2 = int.Parse(splitString[10], System.Globalization.NumberStyles.HexNumber);
                                            int outputStatusSection3 = int.Parse(splitString[11], System.Globalization.NumberStyles.HexNumber);

                                            var byteCountbits = Convert.ToString(byteCount, 2).PadLeft(8, '0');
                                            var quantityCount = Convert.ToInt32(byteCountbits, 2);
                                            var outputStatusSection1bit = Convert.ToString(outputStatusSection1, 2).PadLeft(8, '0');
                                            var outputStatusSection2bit = Convert.ToString(outputStatusSection2, 2).PadLeft(8, '0');
                                            var outputStatusSection3bit = Convert.ToString(outputStatusSection3, 2).PadLeft(8, '0');
                                            outputStatusSection1bit = outputStatusSection1bit.ReverseString();
                                            outputStatusSection2bit = outputStatusSection2bit.ReverseString();
                                            outputStatusSection3bit = outputStatusSection3bit.ReverseString();

                                            response = string.Format("Func:{0}; Bits:{1};Quantity:{2}", functionCode.ToString(), "\n" + outputStatusSection1bit + "\n" + outputStatusSection2bit + "\n" + outputStatusSection3bit + "\n", quantityCount);
                                            Console.WriteLine(response);
                                        }
                                        break;
                                    case FunctionCode.ReadDiscreteInputs:
                                        {
                                            int byteCount = int.Parse(splitString[8], System.Globalization.NumberStyles.HexNumber);
                                            int outputStatusSection1 = int.Parse(splitString[9], System.Globalization.NumberStyles.HexNumber);
                                            int outputStatusSection2 = int.Parse(splitString[10], System.Globalization.NumberStyles.HexNumber);
                                            int outputStatusSection3 = int.Parse(splitString[11], System.Globalization.NumberStyles.HexNumber);

                                            var byteCountbits = Convert.ToString(byteCount, 2).PadLeft(8, '0');
                                            var quantityCount = Convert.ToInt32(byteCountbits, 2);
                                            var outputStatusSection1bit = Convert.ToString(outputStatusSection1, 2).PadLeft(8, '0');
                                            var outputStatusSection2bit = Convert.ToString(outputStatusSection2, 2).PadLeft(8, '0');
                                            var outputStatusSection3bit = Convert.ToString(outputStatusSection3, 2).PadLeft(8, '0');
                                            outputStatusSection1bit = outputStatusSection1bit.ReverseString();
                                            outputStatusSection2bit = outputStatusSection2bit.ReverseString();
                                            outputStatusSection3bit = outputStatusSection3bit.ReverseString();

                                            response = string.Format("Func:{0}; Bits:{1};Quantity:{2}", functionCode.ToString(), "\n" + outputStatusSection1bit + "\n" + outputStatusSection2bit + "\n" + outputStatusSection3bit + "\n", quantityCount);
                                            Console.WriteLine(response);
                                        }
                                        break;
                                    case FunctionCode.ReadHoldingRegisters:
                                        {
                                            int byteCount = int.Parse(splitString[8], System.Globalization.NumberStyles.HexNumber);
                                            int startAddressLow = int.Parse(splitString[9], System.Globalization.NumberStyles.HexNumber);
                                            int quantityOutputHigh = int.Parse(splitString[10], System.Globalization.NumberStyles.HexNumber);

                                            // 21060028
                                            // 慶頂 
                                            // PLC 模組 記憶卡CF 2G 1個
                                            // C 語言模組  0919543899
                                        }

                                        break;
                                    case FunctionCode.ReadInputRegisters:
                                        break;
                                    case FunctionCode.WriteSingleCoil:
                                        break;
                                    case FunctionCode.WriteSingleRegister:
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
                                            response = string.Format("Func:{0}; StartAddr:{1};Quantity:{2}", functionCode.ToString(), startAddOutput, quantityOutput);
                                        }
                                        break;
                                    case FunctionCode.WriteMultipleRegisters:
                                        break;
                                    default:
                                        break;
                                }
                                ReceivedMsg(response);
                            }
                            else
                            {
                                //ReadCoilsError;IllegalDataValue

                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    MasterClient.Client.Close();
                    TcpToConnect(HostIP, Port);
                }
            }
        }
        /// <param name="transactionID">autoIncrement</param>
        /// <param name="protocolID">0 modbus</param>
        /// <param name="SlaveID">any</param>
        /// <param name="FunctionCode">what to do</param>
        /// <param name="StartAddress">buffer</param>
        /// <param name="data">data to send</param>
        public void SendWriteSingleCoilMsgFormat(int SlaveID, int StartAddress, byte[] data)
        {
            autoIncrement++;
            SENDRequest(new SendStruct()
            {
                transactionID = autoIncrement,
                protocolID = 0,
                Address = SlaveID,
                FunctionCode = (int)FunctionCode.WriteSingleCoil,
                StartAddress = StartAddress,
                data = data,                                  ///陣列長度
                dataLength = data.Length,
            });
        }
        /// <param name="transactionID">autoIncrement</param>
        /// <param name="protocolID">0 modbus</param>
        /// <param name="SlaveID">any</param>
        /// <param name="FunctionCode">what to do</param>
        /// <param name="StartAddress">buffer</param>
        /// <param name="data">data to send</param>
        public void SendWriteSingleRegisterMsgFormat(int SlaveID, int StartAddress, byte[] data)
        {
            autoIncrement++;
            SENDRequest(new SendStruct()
            {
                transactionID = autoIncrement,
                protocolID = 0,
                Address = SlaveID,
                FunctionCode = (int)FunctionCode.WriteSingleRegister,
                StartAddress = StartAddress,
                data = data,                                  ///陣列長度
                dataLength = data.Length,
            });
        }
        /// <param name="transactionID">autoIncrement</param>
        /// <param name="protocolID">0 modbus</param>
        /// <param name="SlaveID">any</param>
        /// <param name="FunctionCode">what to do</param>
        /// <param name="StartAddress">buffer</param>
        /// <param name="data">data to send</param>
        public void SendWriteMultipleCoilsMsgFormat(int SlaveID, int StartAddress, byte[] outPutQuantityHighLowBit, params byte[][] multiOutputData)
        {
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
            if (quantityOutput < (multiOutputData.Count() * 2 * 8))
            {
                return;
            }
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
            SENDRequest(new SendStruct()
            {
                transactionID = autoIncrement,
                protocolID = 0,
                Address = SlaveID,
                FunctionCode = (int)FunctionCode.WriteMultipleCoils,
                StartAddress = StartAddress,
                data = data,                                  ///陣列長度
                dataLength = data.Length,
            });
        }

        /// <param name="transactionID">autoIncrement</param>
        /// <param name="protocolID">0 modbus</param>
        /// <param name="SlaveID">any</param>
        /// <param name="FunctionCode">what to do</param>
        /// <param name="StartAddress">buffer</param>
        /// <param name="data">data to send</param>
        public void SendWriteMultipleRegistersMsgFormat(int SlaveID, int StartAddress, byte[] quantityHighLowBit, params byte[][] multipleData)
        {
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
            SENDRequest(new SendStruct()
            {
                transactionID = autoIncrement,
                protocolID = 0,
                Address = SlaveID,
                FunctionCode = (int)FunctionCode.WriteMultipleRegisters,
                StartAddress = StartAddress,
                data = data,                                  ///陣列長度
                dataLength = data.Length,
            });
        }

        /// <param name="transactionID">autoIncrement</param>
        /// <param name="protocolID">0 modbus</param>
        /// <param name="SlaveID">any</param>
        /// <param name="FunctionCode">what to do</param>
        /// <param name="StartAddress">buffer</param>
        /// <param name="data">data to send</param>
        public void ReadCoilsCommand_SendMsgFormat(int SlaveID, int StartAddress, int numberOfDataToRead)
        {
            autoIncrement++;
            var gotByteData = numberOfDataToRead.SplitIntToHighAndLowByte();
            SENDRequest(new SendStruct()
            {
                transactionID = autoIncrement,
                protocolID = 0,
                Address = SlaveID,
                FunctionCode = (int)FunctionCode.ReadCoils,
                StartAddress = StartAddress,
                data = gotByteData,  /// 直接抓取實際數量
                dataLength = gotByteData.Length.SplitIntToHighAndLowByte().Length,
            });
        }
        /// <param name="transactionID">autoIncrement</param>
        /// <param name="protocolID">0 modbus</param>
        /// <param name="SlaveID">any</param>
        /// <param name="FunctionCode">what to do</param>
        /// <param name="StartAddress">buffer</param>
        /// <param name="data">data to send</param>
        public void ReadDiscreteInputs_SendMsgFormat(int SlaveID, int StartAddress, int numberOfDataToRead)
        {
            autoIncrement++;
            var gotByteData = numberOfDataToRead.SplitIntToHighAndLowByte();
            SENDRequest(new SendStruct()
            {
                transactionID = autoIncrement,
                protocolID = 0,
                Address = SlaveID,
                FunctionCode = (int)FunctionCode.ReadDiscreteInputs,
                StartAddress = StartAddress,
                data = gotByteData,  /// 直接抓取實際數量
                dataLength = gotByteData.Length.SplitIntToHighAndLowByte().Length,
            });
        }
        /// <param name="transactionID">autoIncrement</param>
        /// <param name="protocolID">0 modbus</param>
        /// <param name="SlaveID">any</param>
        /// <param name="FunctionCode">what to do</param>
        /// <param name="StartAddress">buffer</param>
        /// <param name="data">data to send</param>
        public void ReadHoldingRegister_SendMsgFormat(int SlaveID, int StartAddress, int numberOfDataToRead)
        {
            autoIncrement++;
            var gotByteData = numberOfDataToRead.SplitIntToHighAndLowByte();
            SENDRequest(new SendStruct()
            {
                transactionID = autoIncrement,
                protocolID = 0,
                Address = SlaveID,
                FunctionCode = (int)FunctionCode.ReadHoldingRegisters,
                StartAddress = StartAddress,
                data = gotByteData,  /// 直接抓取實際數量
                dataLength = gotByteData.Length.SplitIntToHighAndLowByte().Length,
            });
        }
        public void SENDRequest(SendStruct obj)
        {
            byte[] lengthTotal = null;
            var highAndLowBit = obj.transactionID.SplitIntToHighAndLowByte();
            var highAndLowBitProtocol = obj.protocolID.SplitIntToHighAndLowByte();
            var highAndLowBitAddress = new byte[] { Convert.ToByte(obj.Address) };
            var highAndLowBitFunction = new byte[] { Convert.ToByte(obj.FunctionCode) };
            var highAndLowBitStartAddress = obj.StartAddress.SplitIntToHighAndLowByte();
            lengthTotal = (highAndLowBitAddress.Length + highAndLowBitFunction.Length + highAndLowBitStartAddress.Length + obj.dataLength).SplitIntToHighAndLowByte();//+ dataCount.Length obj.data.Length

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
                //DataCount = dataCount,
                // data = obj.data,
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
            _streamFromServer.Write(dataOutStream, 0, dataOutStream.Length);
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
            // headerList.Add(Tuple.Create(StaticVarSharedClass.Data, obj.data));
            return headerList;
        }
        private void ConnectCallBack(IAsyncResult ar)
        {
            ToConnect = ar.AsyncWaitHandle.WaitOne(100, true);
            if (MasterClient.Connected && ToConnect)
            {
                ToConnect = true;
            }
            else
            {
                _streamFromServer?.Close();
                closed = false;
                ToConnect = false;
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
