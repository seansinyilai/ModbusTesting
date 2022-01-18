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
            ConnectionEstablishment = new TestTest("127.0.0.1", 502);
            //ConnectionEstablishment = new TestTest("192.168.3.22", 502);
            ConnectionEstablishment2 = new TestTest2("192.168.0.110", 502);
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
            //await ConnectionEstablishment.SendWriteMultipleRegistersMsgFormat(1, 48,
            //                                                                        new byte[] { 0, 3 },
            //                                                                        new byte[] { 2, 2 },
            //                                                                        new byte[] { 3, 3 },
            //                                                                        new byte[] { 4, 4 });       
            //await ConnectionEstablishment.SendWriteMultipleRegistersMsgFormat(1, 48,
            //                                                                        new byte[] { 0, 1 },
            //                                                                        new byte[] { 0, 221 });
            byte[] array = new byte[64];
            for (int i = 0; i < 64; i++)
            {
                array[i] = Convert.ToByte(i + 1);
            }
            await ConnectionEstablishment.SendWriteMultipleRegistersMsgFormat(1, 0, array);


            //SpinWait.SpinUntil(() => false, 2000);
            //await ConnectionEstablishment.SendWriteMultipleRegistersMsgFormat(1, 48, new byte[] { 221 });
            //SpinWait.SpinUntil(() => false, 2000);
            //await ConnectionEstablishment.SendWriteMultipleRegistersMsgFormat(1, 48, new byte[] { 222 });
            //SpinWait.SpinUntil(() => false, 2000);
            //await ConnectionEstablishment.SendWriteMultipleRegistersMsgFormat(1, 48, new byte[] { 223 });

        }

        private async Task SendWriteMultipleCoils()
        {
            //  var result = await ConnectionEstablishment.SendWriteMultipleCoilsMsgFormat(1, 19, new byte[] { 0, 21}, new byte[] { 173, 90 }, new byte[] { 22,0 });

            string binaryStr = string.Empty;
            byte[] tmptwo = new byte[2];
            List<byte[]> asdf = new List<byte[]>();
            int tmploop = 0;
            int startbit = 0;
            int amountofstr = 16;//    101011010101101010110
           // bool[] tmp = new bool[] { true, false, true, false, true, true, false, true, false, true, false, true, true, false, true, false, true, false, true, true, false };
             bool[] tmp = new bool[] { true, false, true, false, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, true, false, true, true, true, true, false, true, true, true, true, true, true, false, true, };
            Console.WriteLine(tmp.Length);
            // bool[] tmp = new bool[2000];

            //for (int i = 0; i < tmp.Length; i++)
            //{
            //    if (!(i%2).Equals(0))
            //    {
            //        tmp[i] = false;
            //    }
            //    else
            //    {
            //        tmp[i] = true;
            //    }
            //}
            var result = await ConnectionEstablishment.SendWriteMultipleCoilsMsgFormat(1, 19, tmp);

            //var tmpLength = tmp.Length;
            //var mlength = tmpLength.SplitIntToHighAndLowByte();

            //for (int i = 0; i < tmpLength; i++)
            //{
            //    binaryStr += Convert.ToString(Convert.ToInt32(tmp[i]));
            //}
            //var enterTimes = binaryStr.Length / 16;
            //var ableUnableDivided = (binaryStr.Length % 16).Equals(0);
            //if (!ableUnableDivided)
            //{
            //    enterTimes += 1;
            //}
            //var leftVal = binaryStr.Length % 16;
            //byte[] byteArray = new byte[2];
            //for (int i = 0; i < enterTimes; i++)
            //{
            //    if ((enterTimes - 1).Equals(i)&&!leftVal.Equals(0)) amountofstr = leftVal;
            //    startbit = tmploop;
            //    string tmpEmp = string.Empty;
            //    var reversedBinary = binaryStr.Substring(startbit, amountofstr).ToCharArray();
            //    Array.Reverse(reversedBinary);
            //    reversedBinary.ToList().ForEach(x =>
            //    {
            //        tmpEmp += x.ToString();
            //    });
            //    byteArray = Convert.ToInt32(tmpEmp, 2).SplitIntToReverseHighAndLowByte();
            //    asdf.Add(byteArray);
            //    tmploop += 16;
            //}
            //var result = await ConnectionEstablishment.SendWriteMultipleCoilsMsgFormat(1, 19, mlength, asdf.ToArray());

            #region 暫時

            #endregion
            ///  bool[] array = new bool[14];   106 213 21
            //await ConnectionEstablishment.SendWriteMultipleCoilsMsgFormat(1, 20, new byte[] { 0, 40 }, new byte[] { 205, 01 }, new byte[] { 125, 20 });
            //await ConnectionEstablishment.SendWriteMultipleCoilsMsgFormat(1, 20, new byte[] { 0, 56 }, new byte[] { 205, 01 }, new byte[] { 125, 20 }, new byte[] { 20, 50 });
            //await ConnectionEstablishment.SendWriteMultipleCoilsMsgFormat(1, 20, new byte[] { 0, 80 }, new byte[] { 50, 0 }, new byte[] { 255, 0 }, new byte[] { 255, 0 }, new byte[] { 255, 0 }, new byte[] { 255, 0 });

            //var asdf = await ConnectionEstablishment.SendWriteMultipleCoilsMsgFormat(1, 8, new byte[] { 0, 3 }, new byte[] { 5, 0 });
            //var asdf = await ConnectionEstablishment.SendWriteMultipleCoilsMsgFormat(1, 16, new byte[] { 0, 7 }, new byte[] { 119, 0 });
            //var asdf = await ConnectionEstablishment.SendWriteMultipleCoilsMsgFormat(1, 8, new byte[] { 0, 16 }, new byte[] { 1, 149 });
            //var asdf = await ConnectionEstablishment.SendWriteMultipleCoilsMsgFormat(1, 8, new byte[] { 0, 20 }, new byte[] { 170, 170 });
            ///   for (int i = 0; i < 14; i++)
            ///    {
            //       array[i] = true;
            //if ((i % 2).Equals(0))
            //{
            //    array[i] = false;
            //}
            //else
            //{
            //    array[i] = true;
            //}
            ///   }
            ///   var asdf = await ConnectionEstablishment.SendWriteMultipleCoilsMsgFormat(1, 8, new bool[] { true, true, false, false, true, false, true, true, true });
            //new bool[] { true, true, false, false, true, false, true, false, true }
        }

        private async Task SendWriteSingleRegister()
        {
            await ConnectionEstablishment.SendWriteSingleRegisterMsgFormat(1, 1, 750);
            //await ConnectionEstablishment.SendWriteSingleRegisterMsgFormat(2, 12289, new byte[] { 0, 60 });
            // await ConnectionEstablishment.ReadHoldingRegister_SendMsgFormat(2, 12289, 1);
        }

        private async Task SendWriteSingleCoil()
        {
            //await ConnectionEstablishment.SendWriteSingleCoilMsgFormat(1, 17, new byte[] { 0, 0 });
            await ConnectionEstablishment.SendWriteSingleCoilMsgFormat(1, 17, 1);
            SpinWait.SpinUntil(() => false, 5000);
            await ConnectionEstablishment.SendWriteSingleCoilMsgFormat(1, 17, 0);

        }

        private async Task ReadHoldingRegisters()
        {
            //ConnectionEstablishment.ReadHoldingRegister_SendMsgFormat(1, 10001, 3);
            //ConnectionEstablishment.ReadHoldingRegister_SendMsgFormat(1, 108, 3);
            //ConnectionEstablishment.ReadHoldingRegister_SendMsgFormat(1, 0, 3);
            // ConnectionEstablishment.ReadHoldingRegister_SendMsgFormat(2, 48, 3);
            //       var a = await ConnectionEstablishment2.ReadPZ900ModeEnd(2);
            await ConnectionEstablishment.ReadHoldingRegister_SendMsgFormat(1, 0, 64);
            //await Task.Run(async () =>
            //{
            //    while (true)
            //    {
            //        var asd = await ConnectionEstablishment.ReadHoldingRegister_SendMsgFormat(1, 0, 20);

            //    }
            //});
        }

        private async Task ReadDiscreteInputs()
        {
            //ConnectionEstablishment.ReadDiscreteInputs_SendMsgFormat(1, 197, 22);
            int idx = 0;
            int iii = 0;
            //var a = await ConnectionEstablishment2.ReadPZ900ModeEnd(2);
            //////List<TempRecipeStruct> lsTmep = new List<TempRecipeStruct>();
            //////lsTmep.Add(new TempRecipeStruct() {Temperature = 70,TempTime=7 });
            //////lsTmep.Add(new TempRecipeStruct() {Temperature = 333,TempTime=3});
            //////var b = await ConnectionEstablishment2.SetPZ900BufferPointsAsync(2, lsTmep);
            //////var cb = await ConnectionEstablishment2.ReadPZ900BufferPointsAsync(2);
            ////////var b = await ConnectionEstablishment2.SetPZ900ModeEnd(2,6);
            //////var c = await ConnectionEstablishment2.ReadPZ900ModeEnd(2);
            ///
            #region TempClosed
            //await Task.Run(async () =>
            //{
            //    while (true)
            //    {
            //        var a = await ConnectionEstablishment2.ReadPZ900PointsAsync(2);
            //        //  var a = await ConnectionEstablishment2.ReadPZ900AllPointsAsync(2);
            //        if (a)
            //        {
            //            idx++;
            //            Console.WriteLine("輸入===> temp " + idx);
            //            List<TempRecipeStruct> lsTmep = new List<TempRecipeStruct>();
            //            lsTmep.Add(new TempRecipeStruct() { Temperature = 70, TempTime = 7 });
            //            lsTmep.Add(new TempRecipeStruct() { Temperature = 333, TempTime = 3 });
            //            var b = await ConnectionEstablishment2.SetPZ900BufferPointsAsync(2, lsTmep);
            //            //  var b = await ConnectionEstablishment2.AllLightOnAsync(1);
            //            if (b)
            //            {
            //                idx++;
            //                Console.WriteLine("輸入===> temp " + idx);
            //                //var c = await ConnectionEstablishment2.ReadDIsAsync(1);
            //                //var c = await ConnectionEstablishment2.ReadAllLightsAsync(1);
            //                //if (c)
            //                //{
            //                //    var d = await ConnectionEstablishment2.AllLightOffAsync(1);
            //                //    idx++;
            //                //    Console.WriteLine("輸入===> temp " + idx);
            //                //}
            //            }
            //        }
            //        //idx++;
            //        //Console.WriteLine("輸入===> temp " + idx);
            //        iii++;
            //    }
            //});
            #endregion

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
            //   var fgh = await ConnectionEstablishment.WriteGreenLightOffAsync(1);

            //   var b = await ConnectionEstablishment.ReadAllLightsAsync(1);


            //var asdasdfasd = await ConnectionEstablishment.WriteRedLightOnAsync(1);

            //SpinWait.SpinUntil(() => false, 10000);

            //var sdafrrtysadf = await ConnectionEstablishment.WriteRedLightOffAsync(1);

            //var asdasd = await ConnectionEstablishment.WriteYellowLightOnAsync(1);

            //SpinWait.SpinUntil(() => false, 10000);

            //var sdafsadf = await ConnectionEstablishment.WriteYellowLightOffAsync(1);

            //////////SpinWait.SpinUntil(() => false, 200);
            //var asdasdfdfgasd = await ConnectionEstablishment.WriteGreenLightOnAsync(1);

            //SpinWait.SpinUntil(() => false, 10000);

            //var sdafrrtysdfgsadf = await ConnectionEstablishment.WriteGreenLightOffAsync(1);

            //var asdfsadqweqwe = await ConnectionEstablishment.WriteBuzzerOnAsync(1);

            //SpinWait.SpinUntil(() => false, 10000);

            //var asdfsadqweqwe2 = await ConnectionEstablishment.WriteBuzzerOffAsync(1);

            ////////SpinWait.SpinUntil(() => false, 200);   



            #region tmpeclosed



            int idx = 0;
            int iiii = 0;
            await Task.Run(async () =>
            {
                while (true)
                {
                    //SpinWait.SpinUntil(() => false, 200);

                    //var ksadhfk = await ConnectionEstablishment.AllLightOnAsync(1);

                    //SpinWait.SpinUntil(() => false, 200);
                    //var ksasdfsadadhfk = await ConnectionEstablishment.AllLightOffAsync(1);

                    //var asdasd = await ConnectionEstablishment.WriteYellowLightOnAsync(1);

                    //SpinWait.SpinUntil(() => false, 200);

                    //var sdafsadf = await ConnectionEstablishment.WriteYellowLightOffAsync(1);

                    //SpinWait.SpinUntil(() => false, 200);


                    //var asdasdfasd = await ConnectionEstablishment.WriteRedLightOnAsync(1);

                    //SpinWait.SpinUntil(() => false, 200);

                    //var sdafrrtysadf = await ConnectionEstablishment.WriteRedLightOffAsync(1);

                    //SpinWait.SpinUntil(() => false, 200);   
                    //var asdasdfdfgasd = await ConnectionEstablishment.WriteGreenLightOnAsync(1);

                    //SpinWait.SpinUntil(() => false, 200);

                    //var sdafrrtysdfgsadf = await ConnectionEstablishment.WriteGreenLightOffAsync(1);

                    SpinWait.SpinUntil(() => false, 200);
                    //#region TempClose
                    //var a = await ConnectionEstablishment.AllLightOffAsync(1);
                    //if (a)
                    //{

                    //    idx++;
                    //    Console.WriteLine("輸入===> IO " + idx);
                    //    var b = await ConnectionEstablishment.ReadDIsAsync(1);
                    //    var b = await ConnectionEstablishment.ReadAllLightsAsync(1);
                    //    if (b)
                    //    {

                    //        idx++;
                    //        Console.WriteLine("輸入===> IO " + idx);
                    //        var c = await ConnectionEstablishment.AllLightOnAsync(1);
                    //        if (c)
                    //        {
                    //            idx++;
                    //            Console.WriteLine("輸入===> IO " + idx);
                    //            var d = await ConnectionEstablishment.ReadAllLightsAsync(1);
                    //        }
                    //    }

                    //}
                    //idx++;
                    //Console.WriteLine("輸入===> IO " + idx);
                    //iiii++;
                    //#endregion

                    if (iiii == 1)
                    {

                    }
                }
            });
            #endregion
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
