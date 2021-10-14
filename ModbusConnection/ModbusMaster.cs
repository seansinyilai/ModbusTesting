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
        bool asd = false;
        Task RunReadMessageThread;
        TcpClient MasterClient;
        NetworkStream _streamFromServer = default;
        bool closed = false;
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

                        //byte[] testbyte = new byte[1];
                        //MasterClient.Client.Send(testbyte, testbyte.Length, 0);
                        if (MasterClient.Available > 0)
                        {
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
                            string stringdata = BitConverter.ToString(datashow);//把數組轉換成16
                            //var c = _streamFromServer.Read(buff, 0, buff.Length);
                            //string temp = Encoding.ASCII.GetString(buff, 0, buff.Length).Trim((char)0);
                            ReceivedMsg(stringdata);
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
        /// <param name="Address">any</param>
        /// <param name="FunctionCode">what to do</param>
        /// <param name="StartAddress">buffer</param>
        /// <param name="data">data to send</param>
        public void SENDMsgFormat(int transactionID, int protocolID, int Address, FunctionCode FunctionCode, int StartAddress, byte[] data)
        {
            SENDRequest(new SendStruct()
            {
                transactionID = transactionID,
                protocolID = protocolID,
                Address = Address,
                FunctionCode = (int)FunctionCode,
                StartAddress = StartAddress,
                data = data
            });
        }
        public void SENDRequest(SendStruct obj)
        {
            var highAndLowBit = obj.transactionID.SplitIntToHighAndLowByte();
            var highAndLowBitProtocol = obj.protocolID.SplitIntToHighAndLowByte();
            var highAndLowBitAddress = new byte[] { Convert.ToByte(obj.Address) };
            var highAndLowBitFunction = new byte[] { Convert.ToByte(obj.FunctionCode) };
            var highAndLowBitStartAddress = obj.StartAddress.SplitIntToHighAndLowByte();
            var mLength = highAndLowBitAddress.Length + highAndLowBitFunction.Length + highAndLowBitStartAddress.Length + obj.data.Length;
            var dataCount = obj.data.Length.SplitIntToHighAndLowByte();
            var lengthTotal = (highAndLowBitAddress.Length + highAndLowBitFunction.Length + highAndLowBitStartAddress.Length + obj.data.Length+ dataCount.Length).SplitIntToHighAndLowByte();
          
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
                DataCount = dataCount,
                data = obj.data,
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
            headerList.Add(Tuple.Create(StaticVarSharedClass.Data, obj.data));
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

        public virtual bool ReceivedMsg(string msg)
        {
            return false;
        }
    }
}
