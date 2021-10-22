using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModbusTesting
{
    public class TestTest : IOControl
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
        public TestTest(string hostIP, int port) : base(hostIP, port)
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
