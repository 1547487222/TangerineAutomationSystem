
using QStandaedPlatform.Engine.Mqtt;
using System.Drawing;
using System.Net.Sockets;
using System.Text;

namespace QStandaedPlatform.Engine
{
    public class MyProgram
    {
        static async Task Main()
        {

            //modbus 数据帧 ADU=MBAP+PDU+功能码+数据域，MBAP 7byte ,功能码 1byte 数据域由功能确定。
            //MBAP 为报文头，长度7字节，组成如下
            //事务处理标识 2byte 协议标识 2byte 长度 2byte 单元标识符 1byte
            //事务处理标识： 可以理解为报文的序列号，一般每次通信之后就要加1以区别不同的通信数据报文
            //协议标识：00 00表示ModbusTCP协议
            //长度：表示接下来的数据长度，单位为字节
            // 单元标识符： 可以理解为设备地址

            //  Console.WriteLine("hello world");
            //  ModbusClient modbusClient = new("192.168.10.88",502);
            //var values= await modbusClient.ReadCoilsAsync(0,200);
            //  Console.WriteLine();
            //  foreach (var item in values)
            //  {
            //      Console.Write(item);
            //  }
            //ModbusTcp modbusTcp = new(new DeviceLink 
            //{
            //     //Ip= "192.168.10.88",
            //     Ip= "192.168.10.88",
            //     Port =502,
            //});
            //modbusTcp.Connect();
            //modbusTcp.Init();
            //var bit = modbusTcp.ReadMultiCoils(new RegisterVariable { Address = 2, DataLength = 09 });
            //Console.WriteLine(bit);
            //ModbusTcpNet modbusTcpNet = new("127.0.0.1")
            //{
            //    ReceiveTimeOut = 2000
            //};
            ////var a = modbusTcpNet.WriteMultiCoils("M2", [true, true, true, true]);
            //IncrementCount<ushort> incrementCount = new(0);

            //Task.Run(() => 
            //{
            //    while (true)
            //    {
            //        var b = modbusTcpNet.ReadMultiCoils("M100",100).Content;
            //        Thread.Sleep(1);
            //    }
            //});
            //Parallel.For(0, 70000, i =>
            //{
            //    var a = i;
            //    var b = modbusTcpNet.ReadSingleCoil("M2").Content;
            //    Console.WriteLine(incrementCount.GetNextValue()+" "+$"[{a}]" + ":" + b);
            //     modbusTcpNet.WriteMultiCoils("M2", [true, true, true, true]);
            //});
            // var A = modbusTcpNet.WriteMultiRegister("D200",
            // [
            //   -1,-2,-3,-4,-5,-6,-7,-8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30,
            //]);
            //var b = modbusTcpNet.ReadHoldingRegister("D200",1);
            //string result1 = "";

            //var bytes = new List<byte>();
            //foreach (var word in b.Content)
            //{
            //    var temp = BitConverter.GetBytes(word);
            //    if (BitConverter.IsLittleEndian)
            //        temp = temp.Reverse().ToArray();
            //    bytes.AddRange(temp);
            //}
            //result1 = Encoding.ASCII.GetString(bytes.ToArray());
            //Console.WriteLine(result1);
            //var sn=Encoding.ASCII.GetBytes(result1);
            //var ddd = BitConverter.ToInt16([sn[1],sn[0]]);
            //Console.ReadLine();

            BqjxMqttClient bqjxMqttClient = new();
            //await bqjxMqttClient.ConnectAsync("",111);


        }
    }
}

