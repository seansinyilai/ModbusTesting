using ModbusConnection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ModbusTesting
{
    public class MainTest : ObservableObject
    {
        public RelayCommand ReadCoilsSEND { get; set; }
        public RelayCommand ReadDiscreteInputsSEND { get; set; }
        public RelayCommand ReadHoldingRegistersSEND { get; set; }
        public RelayCommand SendWriteSingleCoilSEND { get; set; }
        public RelayCommand SendWriteSingleRegisterSEND { get; set; }
        public RelayCommand SendWriteMultipleCoilsSEND { get; set; }
        public RelayCommand SendWriteMultipleRegistersSEND { get; set; }
        //public IOControl ConnectionEstablishment { get; set; }
        public TestTest ConnectionEstablishment { get; set; }
        public TestTest2 ConnectionEstablishment2 { get; set; }

        private bool flag = false;
        public MainTest()
        {
         
            //ConnectionEstablishment = new IOControl("192.168.0.112", 502);           
            ConnectionEstablishment = new TestTest("192.168.0.112", 502);
            ConnectionEstablishment2 = new TestTest2("192.168.0.112", 502);
            ////ConnectionEstablishment = new IOControl("127.0.0.1", 502);
            ReadCoilsSEND = new RelayCommand(async e => { await ReadCoils(); });
            ReadDiscreteInputsSEND = new RelayCommand(async e => { await ReadDiscreteInputs(); });
            //ReadDiscreteInputsSEND = new RelayCommand(ReadDiscreteInputs, param => true);
            ReadHoldingRegistersSEND = new RelayCommand(async e => { await ReadHoldingRegisters(); });
            SendWriteSingleCoilSEND = new RelayCommand(async e => { await SendWriteSingleCoil(); });
            SendWriteSingleRegisterSEND = new RelayCommand(async e => { await SendWriteSingleRegister(); });
            SendWriteMultipleCoilsSEND = new RelayCommand(async e => { await SendWriteMultipleCoils(); });
            SendWriteMultipleRegistersSEND = new RelayCommand(async e => { await SendWriteMultipleRegisters(); });
            //ReadHoldingRegistersSEND = new RelayCommand(ReadHoldingRegisters, param => true);
            //SendWriteSingleCoilSEND = new RelayCommand(SendWriteSingleCoil, param => true);
            //SendWriteSingleRegisterSEND = new RelayCommand(SendWriteSingleRegister, param => true);
            //SendWriteMultipleCoilsSEND = new RelayCommand(SendWriteMultipleCoils, param => true);
            //SendWriteMultipleRegistersSEND = new RelayCommand(SendWriteMultipleRegisters, param => true);
        }

        private async Task SendWriteMultipleRegisters()
        {
            //ConnectionEstablishment.SendWriteMultipleRegistersMsgFormat(1, 2, new byte[] { 0, 3 }, new byte[] { 0, 10 }, new byte[] { 1, 2 }, new byte[] { 1, 2 });
            await ConnectionEstablishment.SendWriteMultipleRegistersMsgFormat(2, 48, new byte[] { 0, 3 }, new byte[] { 0, 1 }, new byte[] { 0, 1 }, new byte[] { 0, 1 });

        }

        private async Task SendWriteMultipleCoils()
        {
            //ConnectionEstablishment.SendWriteMultipleCoilsMsgFormat(1, 20, new byte[] { 0, 40 }, new byte[] { 205, 01 }, new byte[] { 125, 20 });
            var asdf = await ConnectionEstablishment.SendWriteMultipleCoilsMsgFormat(1, 16, new byte[] { 0, 16 }, new byte[] { 0, 0 });
        }

        private async Task SendWriteSingleRegister()
        {
            await ConnectionEstablishment.SendWriteSingleRegisterMsgFormat(2, 1, new byte[] { 0, 3 });
        }

        private async Task SendWriteSingleCoil()
        {
            await ConnectionEstablishment.SendWriteSingleCoilMsgFormat(1, 17, new byte[] { 0, 0 });
        }

        private async Task ReadHoldingRegisters()
        {
            //ConnectionEstablishment.ReadHoldingRegister_SendMsgFormat(1, 10001, 3);
            //ConnectionEstablishment.ReadHoldingRegister_SendMsgFormat(1, 108, 3);
            //ConnectionEstablishment.ReadHoldingRegister_SendMsgFormat(1, 0, 3);
            // ConnectionEstablishment.ReadHoldingRegister_SendMsgFormat(2, 48, 3);
            await Task.Run(async () =>
            {
                while (true)
                {
                    await ConnectionEstablishment.ReadHoldingRegister_SendMsgFormat(2, 0, 20);

                }
            });
        }

        private async Task ReadDiscreteInputs()
        {
            //ConnectionEstablishment.ReadDiscreteInputs_SendMsgFormat(1, 197, 22);
            int idx = 0;

            //////var a = await ConnectionEstablishment2.ReadPZ900ModeEnd(2);
            //////List<TempRecipeStruct> lsTmep = new List<TempRecipeStruct>();
            //////lsTmep.Add(new TempRecipeStruct() {Temperature = 70,TempTime=7 });
            //////lsTmep.Add(new TempRecipeStruct() {Temperature = 333,TempTime=3});
            //////var b = await ConnectionEstablishment2.SetPZ900BufferPointsAsync(2, lsTmep);
            //////var cb = await ConnectionEstablishment2.ReadPZ900BufferPointsAsync(2);
            ////////var b = await ConnectionEstablishment2.SetPZ900ModeEnd(2,6);
            //////var c = await ConnectionEstablishment2.ReadPZ900ModeEnd(2);
            ///

            await Task.Run(async () =>
            {
                while (true)
                {
                    var a = await ConnectionEstablishment2.ReadPZ900PointsAsync(1);
                    if (a)
                    {
                        Console.WriteLine("輸入===> temp " + idx);
                        idx++;
                        var b = await ConnectionEstablishment2.AllLightOnAsync(1);
                        if (b)
                        {
                            Console.WriteLine("輸入===> temp " + idx);
                            idx++;
                            var c = await ConnectionEstablishment2.ReadDIsAsync(1);
                            if (c)
                            {
                                var d = await ConnectionEstablishment2.AllLightOffAsync(1);
                            }
                        }

                    }

                    Console.WriteLine("輸入===> temp " + idx);
                    idx++;
                }
            });
            //new Thread(() =>
            //{
            //    int idx = 0;
            //    while (true)
            //    {
            //        //ConnectionEstablishment2.ReadPZ900Points(2);
            //        //Console.WriteLine("輸入===> Z900Points " + idx);
            //        //ConnectionEstablishment2.AllLightOff(1);
            //        //ConnectionEstablishment2.ReadDIs(1);
            //        //ConnectionEstablishment2.AllLightOn(1);
            //        //idx++;
            //    }

            //}).Start();

            //if (!flag)
            //{
            //    ConnectionEstablishment.AllLightOn(1);
            //    flag = true;
            //}
            //else
            //{
            //    ConnectionEstablishment.AllLightOff(1);
            //    flag = false;
            //}
        }

        private async Task ReadCoils()
        {
            int idx = 0;
            await Task.Run(async () =>
            {
                while (true)
                {

                    var a = await ConnectionEstablishment.AllLightOffAsync(1);
                    if (a)
                    {
                        Console.WriteLine("輸入===> IO " + idx);
                        idx++;
                        //var b = await ConnectionEstablishment.ReadDIsAsync(1);
                        var b = await ConnectionEstablishment.ReadAllLightsAsync(1);
                        if (b)
                        {
                            Console.WriteLine("輸入===> IO " + idx);
                            idx++;
                            var c = await ConnectionEstablishment.AllLightOnAsync(1);
                            if (c)
                            {
                                var d = await ConnectionEstablishment.ReadAllLightsAsync(1);
                                Console.WriteLine("輸入===> IO " + idx);
                                idx++;
                            }
                        }

                    }

                    Console.WriteLine("輸入===> IO " + idx);
                    idx++;
                }
            });

            //  var result= await ConnectionEstablishment.ReadDIsAsync(1);
            //await Task.Run(() =>
            //  {
            //       //    //ConnectionEstablishment.AllLightOff(1);
            //       //    //ConnectionEstablishment.ReadDIs(1);
            //       //    //  ConnectionEstablishment.AllLightOn(1);
            //       //    //   ConnectionEstablishment.AllLightOff(1);
            //       //    //int idx = 0;

            //  }).Start();
            //ConnectionEstablishment.ReadDIs();
            //if (!flag)
            //{
            //    ConnectionEstablishment.AllLightOn();
            //    flag = true;
            //}
            //else
            //{
            //    ConnectionEstablishment.AllLightOff();
            //    flag = false;
            //}

            //ConnectionEstablishment.WriteGreenLight();
            //ConnectionEstablishment.WriteYellowLight();
            //ConnectionEstablishment.WriteRedLight();
            //ConnectionEstablishment.WriteBuzzerLight();
            //ConnectionEstablishment.ReadCoilsCommand_SendMsgFormat(1, 16, 9);
        }


    }
}
