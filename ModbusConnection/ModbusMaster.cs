using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusConnection
{
    public class ModbusMaster
    {

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
                        byte[] testbyte = new byte[1];
                        MasterClient.Client.Send(testbyte, testbyte.Length, 0);
                        if (MasterClient.Available > 0)
                        {
                            DateTime RecvTime = DateTime.Now;
                            _streamFromServer = MasterClient.GetStream();
                            byte[] buff = new byte[MasterClient.ReceiveBufferSize];
                            _streamFromServer.Read(buff, 0, buff.Length);
                            string temp = Encoding.ASCII.GetString(buff, 0, buff.Length).Trim((char)0);
                            ReceivedMsg(temp);
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
        public void SENDMsgFormat(int transactionID, int protocolID, int Address, MBAPHeader FunctionCode, int StartAddress, byte[] data)
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
            var lengthTotal = (highAndLowBitAddress.Length + highAndLowBitFunction.Length + highAndLowBitStartAddress.Length + obj.data.Length).SplitIntToHighAndLowByte();
            var dataCount = obj.data.Length.SplitIntToHighAndLowByte();
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
