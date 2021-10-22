using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusTesting
{
    public class TestTest2 : TemperatureControl
    {
        private bool _testConnectionStatus;

        public bool TestConnectionStatus
        {
            get { return _testConnectionStatus; }
            set
            {
                _testConnectionStatus = value;
                NotifyPropertyChanged();
            }
        }
        public TestTest2(string hostIP, int port) : base(hostIP, port)
        {
            ConnectStatusChanged += (connnectionStatus) =>
            {
                TestConnectionStatus = connnectionStatus;
            };
        }

        public override bool ReceivedCallBackMsg(string msg)
        {
            return true;
        }
    }
}
