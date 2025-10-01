using Castle.Components.DictionaryAdapter;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.ModuleEntitys;
using StackExchange.Redis;
using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ProtoBuf;

namespace MsTestProject
{

    [ProtoContract]
    public class ByteArraysContainer
    {
        [ProtoMember(1)]
        public List<byte[]> ByteArrays { get; set; } = new List<byte[]>();
    }

    [TestClass]
    public sealed class RedisTest
    {

        private static string _singleRedisConn = "123.56.45.66:6380,password=Bqjx2024,defaultDatabase=1";

        private static string _multyRedisConn = "123.56.45.66:6380,password=Bqjx2024,defaultDatabase=1";


        private static string _mongoDbConn = "mongodb://123.56.45.66:27017";

        private long _runTimes = 10_0000;

        private static TangerineEncrypTtransmitor encrypTtransmitor = new TangerineEncrypTtransmitor();


        private static string _flowId = Guid.NewGuid().ToString().Replace('-', '_');

        //private static RedisClient _redis = new RedisClient(_singleRedisConn);

        private static ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(new ConfigurationOptions
        {
            EndPoints = { "123.56.45.66:6380" }, // Host + Port
            Password = "Bqjx2024",               // Explicit password
            SyncTimeout = 50000 // 5 seconds (default is 1s)
        });

        private static IDatabase db1 = redis.GetDatabase(10);
        private static IDatabase db2 = redis.GetDatabase(2);

        private static MongoClient _mongoClient = new MongoClient(_mongoDbConn);


       // static void Main(string[] args)
       // {
       //     // SingleDatabaseMethod();
       //     Stopwatch stopwatch1 = new Stopwatch();
       //     Stopwatch stopwatch2 = new Stopwatch();


       //     #region redisTest
       //     // 开始计时

       //     //var task1 = Task.Run(() =>
       //     //{
       //     //    stopwatch1.Start();
       //     //    for (int i = 0; i < 100; i++)
       //     //    {
       //     //        var valus = db1.SortedSetRangeByScoreWithScoresAsync(DateTime.Now.ToString("yyyyMMdd"), 0 + (i * 100), 0 + (i * 100) + 100).GetAwaiter().GetResult();
       //     //        foreach (var item in valus)
       //     //        {
       //     //            Console.WriteLine(item.Score + ":\r\n" + encrypTtransmitor.Decrypt(DecompressWithDeflate(item.Element)));
       //     //            Console.WriteLine(i);
       //     //        }
       //     //    }
       //     //    stopwatch1.Stop();
       //     //    Console.WriteLine($"StopWatch1:{stopwatch1.ElapsedMilliseconds}");
       //     //});


       //     //var task2 = Task.Run(() =>
       //     //{
       //     //    stopwatch2.Start();
       //     //    for (int i = 0; i < 100; i++)
       //     //    {
       //     //        var valus = db2.SortedSetRangeByScoreWithScoresAsync(DateTime.Now.ToString("yyyyMMdd"), 0 + (i * 100), 0 + (i * 100) + 100).GetAwaiter().GetResult();
       //     //        foreach (var item in valus)
       //     //        {
       //     //            Console.WriteLine(item.Score + ":\r\n" + item.Element);
       //     //            Console.WriteLine(i);
       //     //        }
       //     //    }
       //     //    stopwatch2.Stop();
       //     //    Console.WriteLine($"StopWatch2:{stopwatch2.ElapsedMilliseconds}");

       //     //});

       //     //Task.WhenAll(task1,task2).ContinueWith( a =>
       //     //{
       //     //    Console.WriteLine($"StopWatch1:{stopwatch1.ElapsedMilliseconds}");
       //     //    Console.WriteLine($"StopWatch2:{stopwatch2.ElapsedMilliseconds}");
       //     //});
       //     #endregion

       //     #region mongoDbTest

       //     try
       //     {
       //         var myDB = _mongoClient.GetDatabase("TestDbOne");
       //         var collection = myDB.GetCollection<FlowSingleByteData>("385de086_2aea_4b01_828c_d3c71fc37b97");

       //         var indexKeys = Builders<FlowSingleJsonData>.IndexKeys
       //              .Ascending(p => p.CreateTime);

       //         var indexOptions = new CreateIndexOptions
       //         {
       //             Unique = true,
       //             Name = $"{_flowId}_createTime" // 自定义索引名称
       //         };
       //         //collection.Indexes.CreateOne(new CreateIndexModel<FlowSingleJsonData>(indexKeys, indexOptions));

       //         //stopwatch1.Start();

       //         //for (int q = 0; q < 10000; q++)
       //         //{
       //         //    var ByteDatas = CreateSingleData();

       //         //    //var zip = CompressWithDeflate(ByteDatas);

       //         //    var singleData = new FlowSingleJsonData
       //         //    {
       //         //        ByteDatas = ByteDatas
       //         //    };

       //         //    collection.InsertOne(singleData);


       //         //}
       //         //stopwatch1.Stop();

       //         //Console.WriteLine($"MongoDbTest write ms:{stopwatch1.ElapsedMilliseconds}");

       //         stopwatch2.Start();
             
       //         var filter = Builders<FlowSingleByteData>.Filter.And(
       //    Builders<FlowSingleByteData>.Filter.Gte(x => x.CreateTime, Convert.ToDateTime("2019-08-06 00:00:00")),
       //    Builders<FlowSingleByteData>.Filter.Lte(x => x.CreateTime, Convert.ToDateTime("2038-08-06 23:59:59"))
       //);

       //         var tempList = new List<byte[]>();
       //         var datas = collection.Find(filter).SortBy(x => x.CreateTime).ToList();

                
       //         int i = 0;
               
       //         foreach (var item in datas)
       //         {

       //             tempList.Add(item.ByteDatas);
                   
       //         }


       //         var result = SerializeByteArrays(tempList);

       //         stopwatch2.Stop();
       //         Console.WriteLine($"MongoDbTest read ms:{stopwatch2.ElapsedMilliseconds};i:{i}");
       //         Console.WriteLine(result);


                

       //     }
       //     catch (Exception ex)
       //     {

       //         throw;
       //     }
       //     #endregion




       //     Console.ReadLine();
       // }

        // 2. 序列化方法
        public static byte[] SerializeByteArrays(List<byte[]> byteArrays)
        {
            var container = new ByteArraysContainer { ByteArrays = byteArrays };
            using var stream = new MemoryStream();
            Serializer.Serialize(stream, container);
            return stream.ToArray();
        }

        // 3. 反序列化方法
        protected static List<byte[]> DeserializeByteArrays(byte[] data)
        {
            using var stream = new MemoryStream(data);
            var container = Serializer.Deserialize<ByteArraysContainer>(stream);
            return container.ByteArrays;
        }


        [TestMethod]
        public static string SingleDatabaseMethod()
        {

            var list = new List<FlowSingleByteData>();
            for (int q = 0; q < 10000; q++)
            {
                var singleData = new FlowSingleByteData
                {
                    //ByteDatas = CreateSingleData()
                };
                //var text=  encrypTtransmitor.Encrypt(JsonConvert.SerializeObject(singleData));
                //  var zip = CompressWithDeflate(text);

                var text = JsonConvert.SerializeObject(singleData);
                DateTime today = DateTime.Today;
                db1.SortedSetIncrement(DateTime.Now.ToString("yyyyMMdd"), text, q);
            }


            return string.Empty;

        }

        public static byte[] CompressWithDeflate(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            using var memoryStream = new MemoryStream();
            using (var deflateStream = new DeflateStream(memoryStream, CompressionLevel.Optimal))
            {
                deflateStream.Write(bytes, 0, bytes.Length);
            }
            return memoryStream.ToArray();
        }

        public static string DecompressWithDeflate(byte[] compressedBytes)
        {
            using var memoryStream = new MemoryStream(compressedBytes);
            using var outputStream = new MemoryStream();
            using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress))
            {
                deflateStream.CopyTo(outputStream);
            }
            return Encoding.UTF8.GetString(outputStream.ToArray());
        }



        public static string CreateSingleData()
        {
            var model = CreateSingleModelData();

              var json = JsonConvert.SerializeObject(model);

            return json;
        }

        public static ModuleReportRunDataEntity CreateSingleModelData(int index = 0)
        {
            ModuleReportRunDataEntity moduleReportRunDataEntity = new();
            moduleReportRunDataEntity.ModuleName = $"加液模块{index}";
            moduleReportRunDataEntity.ModuleActionDescription = "";
            moduleReportRunDataEntity.Alarms =
            [
               new ModuleReportAlarmEntity
                    {
                        AlarmCode="ALM_01",
                    }
            ];
            moduleReportRunDataEntity.ModuleParameter = "";
            moduleReportRunDataEntity.PlatFormName = "加液平台";
            moduleReportRunDataEntity.StartDay = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            moduleReportRunDataEntity.StartTime = DateTime.Now;
            moduleReportRunDataEntity.TaskDatas = [];
            for (int i = 0; i < 5; i++)
            {
                var taskData = new ModuleReportMonitorTaskDataOnceEntity();
                for (int j = 0; j < 10; j++)
                {
                    taskData.ModuleReportCollectTaskDataItemEntities.Add(new ModuleReportMonitorTaskDataItemEntity
                    {
                        MonitorName = $"采集项{j}",
                        MonitorTime = DateTime.Now,
                        MonitorValue = i.ToString()
                    });
                }
                moduleReportRunDataEntity.TaskDatas.Add(taskData);
            }

          

            return moduleReportRunDataEntity;
        }


        public string CreateMultyData()
        {
            var tempList = new List<ModuleReportRunDataEntity>();
            for (int count = 0; count < _runTimes; count++)
            {
                ModuleReportRunDataEntity moduleReportRunDataEntity = new();
                moduleReportRunDataEntity.ModuleName = "加液模块";
                moduleReportRunDataEntity.ModuleActionDescription = "";
                moduleReportRunDataEntity.Alarms =
                [
                   new ModuleReportAlarmEntity
                    {
                        AlarmCode="ALM_01",
                    }
                ];
                moduleReportRunDataEntity.ModuleParameter = "";
                moduleReportRunDataEntity.PlatFormName = "加液平台";
                moduleReportRunDataEntity.StartDay = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                moduleReportRunDataEntity.StartTime = DateTime.Now;
                moduleReportRunDataEntity.TaskDatas = [];
                for (int i = 0; i < 20; i++)
                {
                    var taskData = new ModuleReportMonitorTaskDataOnceEntity();
                    for (int j = 0; j < 5; j++)
                    {
                        taskData.ModuleReportCollectTaskDataItemEntities.Add(new ModuleReportMonitorTaskDataItemEntity
                        {
                            MonitorName = $"采集项{j}",
                            MonitorTime = DateTime.Now,
                            MonitorValue = i.ToString()
                        });
                    }
                    moduleReportRunDataEntity.TaskDatas.Add(taskData);
                }

                tempList.Add(moduleReportRunDataEntity);

            }
            var json = JsonConvert.SerializeObject(tempList);

            return json;
        }

    }

    public class FlowSingleByteData
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public DateOnly CreateDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        public DateTime CreateTime { get; set; } = DateTime.Now;

        public byte[] ModuleReportRunData { get; set; }

    }

    public class FlowSingleJsonData
    {
        [BsonId]
        public ObjectId _id { get; set; }
        public DateOnly CreateDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        public DateTime CreateTime { get; set; } = DateTime.Now;

        public string ModuleReportRunData { get; set; }

    }

    public class FlowDataOneDay
    {
        public string FlowId { get; set; } = Guid.NewGuid().ToString().Replace("-", "_");

        public string ModuleReportRunData { get; set; }

        public long Score { get; set; } = DateTime.UtcNow.Ticks;
    }

    public class TangerineEncrypTtransmitor
    {
        private readonly byte[] _iv;

        public TangerineEncrypTtransmitor()
        {
            _iv = new byte[16];
        }

        public string Key { get; set; } = "1234567890123456";
        public string Encrypt(string plainText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(Key);
                aesAlg.IV = _iv;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new())
                {
                    using (CryptoStream csEncrypt = new(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        public string Decrypt(string cipherText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(Key);
                aesAlg.IV = _iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }

}
