using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModbusConnection;

namespace ModbusTesting
{
    public class ConnecTemp : ModbusMaster
    {
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

        public ConnecTemp(string hostIP, int port) : base(hostIP, port)
        {
            ResponseResult = string.Empty;
            ConnectionStatusChanged += (connnectionStatus) =>
            {
                ConnectionStatus = connnectionStatus;
            };
        }

        public override bool ReceivedMsg(string msg)
        {            
            if (ResponseResult.Length > 32767)
            {
                ResponseResult = string.Empty;
            }
            ResponseResult += msg + "\n";
            return true;
        }
    }
}
