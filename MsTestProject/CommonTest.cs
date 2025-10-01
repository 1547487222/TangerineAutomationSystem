using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using QStandaedPlatform.Engine.Common.Common.ModuleEntitys;
using System;
using ProtoBuf;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using MongoDB.Driver;
using System.Diagnostics;
using Tangerine.ModuleRunDataTransferApi.Common;
using Tangerine.ModuleRunDataTransferApi.Models;
using System.Threading;
using Microsoft.Extensions.Options;
using SqlSugar;
using System.IO.Compression;
using System.Net.Http.Headers;
using QStandaedPlatform.Engine.Laboratory;
using QStandaedPlatform.Engine.Common.Common.SampleEntitys;

namespace MsTestProject
{
    [TestClass]
    public class CommonTest
    {
        private static HttpClient _httpClient;

        private RedisTest rt = new RedisTest();

        private int _httpTimeOut = 60 * 5;

        private string _mongoWebApiBaseAdress = "http://192.168.120.45:6100";

        private static string _mongoDbConn = "mongodb://admin:Bqjx2025@123.56.45.66:27018/?authSource=admin";

        private long _runTimes = 10_0000;

        private static MongoClient _mongoClient = new MongoClient(_mongoDbConn);


        [TestMethod]
        public void ConvertTest()
        {
            var ret = Convert.ToDateTime("2025-08-08 00:00:00");
            Console.WriteLine(ret);
        }


        [TestMethod]
        public async Task WebApiAddDataTest()
        {
            
            try
            {
                //_httpClient = new HttpClient { BaseAddress = new Uri(_mongoWebApiBaseAdress), Timeout = TimeSpan.FromSeconds(_httpTimeOut) };

                Stopwatch st1 = new();
                st1.Start();
                var options = new ParallelOptions
                {
                    MaxDegreeOfParallelism = 100, // 最大并发数
                    CancellationToken = new CancellationToken() // 可取消令牌
                };

              

                Parallel.For(0, 10, options, async j =>
                {
                    using (_httpClient = new HttpClient { BaseAddress = new Uri(_mongoWebApiBaseAdress), Timeout = TimeSpan.FromSeconds(_httpTimeOut) }) 
                    {
                        try
                        {
                            for (int i = 0; i < 10000; i++)
                            {
                                var testData = RedisTest.CreateSingleModelData(i);

                                var entity = new FlowRunDataEntity
                                {
                                    DatabaseName = "TestTwo",
                                    FlowId = $"a{j}",
                                    ModuleRunData = testData
                                };
                                var json = JsonConvert.SerializeObject(entity);
                                //var content = GetStreamContentFromStr(json);

                                var content = new StringContent(
                                     json,
                                     Encoding.UTF8,
                                     "application/json"
                                 );

                                // 2. 调用 API
                                var response =  _httpClient.PostAsync("api/DataTransferMongo/addModuleRunData", content).GetAwaiter().GetResult();


                                // 3. 验证响应
                                var code = response.IsSuccessStatusCode;

                                //var createdProduct = await response.Content.ReadFromJsonAsync<AddResponseModel>();
                            }

                        }
                        catch (Exception e)
                        {

                            throw ;
                        }
                    }

               });
                st1.Stop();
                var ms = st1.ElapsedMilliseconds;
                Console.WriteLine($"花费时间:{ms} ms");

            }
            catch (TaskCanceledException ex)
            {

            }
            catch (Exception ex)
            {

                throw;
            }

        }

        [TestMethod]
        public async Task WebApiGetDataTest()
        {
            try
            {
                _httpClient = new HttpClient { BaseAddress = new Uri(_mongoWebApiBaseAdress), Timeout = TimeSpan.FromSeconds(_httpTimeOut) };

                var entity = new MongoDataQueryDto
                {
                    DatabaseName = "ModuleRunData",
                    TableName = "06c5bbfb_3623_4a4e_8170_fa89658a3eda",
                    StartDate = DateOnly.FromDateTime(DateTime.Now.ToUniversalTime()),
                    EndDate = DateOnly.FromDateTime(DateTime.Now.ToUniversalTime()),
                };
                var jsonContent = new StringContent(
                           JsonConvert.SerializeObject(entity),
                           Encoding.UTF8,
                           "application/json"
                       );

                // 2. 调用 API
                var response = await _httpClient.PostAsync("api/DataTransferMongo/getModuleRunData", jsonContent);

                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync();

                var container = Serializer.Deserialize<ByteArraysContainer>(stream);
                var byteArray = container.ByteArrays;
                var tempList = new List<ModuleReportRunDataEntity>();
                foreach (var item in byteArray)
                {
                    var text = RedisTest.DecompressWithDeflate(item);
                    var tempEntity = JsonConvert.DeserializeObject<ModuleReportRunDataEntity>(text);
                    tempList.Add(tempEntity);
                }
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine("http响应超时");
            }
            catch (Exception ex)
            {

                throw;
            }
           


        }


        [TestMethod]
        public async Task WebApiAddSampleTraceDataTest()
        {

            try
            {
                //_httpClient = new HttpClient { BaseAddress = new Uri(_mongoWebApiBaseAdress), Timeout = TimeSpan.FromSeconds(_httpTimeOut) };

                Stopwatch st1 = new();
                st1.Start();
                var options = new ParallelOptions
                {
                    MaxDegreeOfParallelism = 100, // 最大并发数
                    CancellationToken = new CancellationToken() // 可取消令牌
                };


                Parallel.For(0, 4, options, async j =>
                {
                    using (_httpClient = new HttpClient { BaseAddress = new Uri(_mongoWebApiBaseAdress), Timeout = TimeSpan.FromSeconds(_httpTimeOut) })
                    {
                        try
                        {
                            for (int i = 0; i < 1000; i++)
                            {
                                var testData = RedisTest.CreateSingleModelData(i);

                                var entity = new SampleTraceEntity
                                {
                                    ProcessflowId = j.ToString(),
                                    PlatformId = j % 4,
                                    PlatformName = $"platFormName:{j % 4}",
                                    PlatformTaskId = j % 4,
                                    SamplingTaskId = j % 4,
                                    SamplingId = i % 20,
                                    SampleSn = $"sampleSN:{i % 20}",
                                    SampleName = $"SampleName:{i % 20}",
                                    SampleType = "TestSample",
                                    SampleRemarks = $"SampleRemarks{i % 20}",
                                    ModuleName = $"moduleName:{i % 10}",
                                    ModuleActionId = $"moduleName:{i}",
                                    ModuleActionDescription = $"ModuleActionDescription:{i}",
                                    InputParametersJson = $"InputParametersJson:{i}",
                                    ModuleSerialNumber = i,
                                    TaskEbrDataEntities = new List<SampleTaskDataEntity> 
                                    { 
                                        new SampleTaskDataEntity{EbrKey = "速度", EbrValue = "100",EbrUnit ="rpm",EbrKeyDescription = "测试速度" },
                                        new SampleTaskDataEntity{EbrKey = "温度", EbrValue = "4",EbrUnit ="度",EbrKeyDescription = "测试温度"},
                                        new SampleTaskDataEntity{ EbrKey = "时间", EbrValue = "10",EbrUnit ="s",EbrKeyDescription = "测试时间"},
                                        new SampleTaskDataEntity{EbrKey = "加液量", EbrValue = "5",EbrUnit ="ml",EbrKeyDescription = "测试加液量" },
                                        new SampleTaskDataEntity{ EbrKey = "加盐量", EbrValue = "8",EbrUnit ="g",EbrKeyDescription = "测试加盐量"},
                                    }
                                };
                                var json = JsonConvert.SerializeObject(entity);
                                var content = GetStreamContentFromStr(json);

                                //var content = new StringContent(
                                //     json,
                                //     Encoding.UTF8,
                                //     "application/json"
                                // );

                                // 2. 调用 API
                                var response = _httpClient.PostAsync("api/DataTransferMongo/addSampleTraceData", content).GetAwaiter().GetResult();


                                // 3. 验证响应
                                var code = response.IsSuccessStatusCode;

                                //var createdProduct = await response.Content.ReadFromJsonAsync<AddResponseModel>();
                            }

                        }
                        catch (Exception e)
                        {

                            throw;
                        }
                    }

                });
                st1.Stop();
                var ms = st1.ElapsedMilliseconds;
                Console.WriteLine($"花费时间:{ms} ms");

            }
            catch (TaskCanceledException ex)
            {

            }
            catch (Exception ex)
            {

                throw;
            }

        }


        [TestMethod]
        public async Task WebApiGetSampleTraceDataTest()
        {
            try
            {
                Stopwatch st1 = new();
                st1.Start();

                _httpClient = new HttpClient { BaseAddress = new Uri(_mongoWebApiBaseAdress), Timeout = TimeSpan.FromSeconds(_httpTimeOut) };

                var entity = new SampleTraceQueryDto
                {
                    SamplingId = 6
                };
                var jsonContent = new StringContent(
                           JsonConvert.SerializeObject(entity),
                           Encoding.UTF8,
                           "application/json"
                       );

                // 2. 调用 API
                var response = await _httpClient.PostAsync("api/DataTransferMongo/getSampleTraceData", jsonContent);

                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync();

                var byteData = await StreamToByteArrayWithBufferAsync(stream);

                var strList = RedisTest.DecompressWithDeflate(byteData);

                var tempList = JsonConvert.DeserializeObject<List<SampleTraceEntity>>(strList);

                st1.Stop();
                var s1 =  st1.ElapsedMilliseconds;
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine("http响应超时");
            }
            catch (Exception ex)
            {

                throw;
            }



        }



        [TestMethod]
        public void TestAsync()
        {
            var bList = new List<bool> { true, false, true, true };

            var b = true;

            foreach (var item in bList)
            {
                if (!item)
                {
                    b = item;
                }
            }

            Console.WriteLine($"b:{b}");

            var bb = true;
            bList.ForEach( s =>
            {
                if (!s)
                {
                    bb = s;
                }
            });
            Console.WriteLine($"bb:{bb}");

        

        }



        [TestMethod]
        public async Task AddMongoDataTest()
        {
            Stopwatch stopwatch1 = new Stopwatch();
            Stopwatch stopwatch2 = new Stopwatch();

            var databaseNameList = new List<string>() {"TestBaseOne", "TestBaseTwo", "TestBaseThree" };

            try
            {
                var cancellationToken = new CancellationToken(); // 可取消令牌

                stopwatch1.Start();

                var options = new ParallelOptions
                {
                    MaxDegreeOfParallelism = 100, // 最大并发数
                    CancellationToken = cancellationToken // 可取消令牌
                };
               
                Parallel.For(0, 100, options, i =>
                {
                    using (var mongoClient = new MongoClient(_mongoDbConn))
                    {
                        for (int j = 0; j < 1000; j++)
                            {
                                var databaseName = "testTwo";
                                var tableName = $"t{i}";
                                var data = RedisTest.CreateSingleModelData((j * i) + j);
                                var ret = AddDataToMongo(mongoClient, databaseName, tableName, data);
                            }
                    }
                });

                //using (var mongoClient = new MongoClient(_mongoDbConn))
                //{
                //    for (int j = 0; j < 10000; j++)
                //    {
                //        var databaseName = "testFour";
                //        var tableName = $"t1";
                //        var data = RedisTest.CreateSingleModelData(j);
                //        var ret = AddDataToMongo(mongoClient, databaseName, tableName, data);
                //    }
                //}
                stopwatch1.Stop();
                var ms = stopwatch1.ElapsedMilliseconds;

                Console.WriteLine($"话费时间：{ms} ms");
            }
            catch (Exception ex)
            {

                throw;
            }
            
        }

        private async Task<(bool result, string msg)> AddDataToMongo<T>(MongoClient mongoClient,string DatabaseName, string tableName, T tData) where T : class
        {
            try
            {
               
                    var mongoHelper = new MongoDbHelper(mongoClient, DatabaseName);

                    var text = Newtonsoft.Json.JsonConvert.SerializeObject(tData);
                    var zip = RedisTest.CompressWithDeflate(text);

                    var data = new MongoDataByteBaseEntity
                    {
                        CreateDate = DateOnly.FromDateTime(DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local)),
                        CreateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local),
                        ByteDatas = zip
                    };


                    var collection = mongoHelper.GetCollection<MongoDataByteBaseEntity>(DatabaseName, tableName);

                    var indexKeys = Builders<MongoDataByteBaseEntity>.IndexKeys
                           .Ascending(p => p.CreateTime);

                    var indexOptions = new CreateIndexOptions
                    {
                        Unique = true,
                        Name = $"{tableName}_CreateTime"// 自定义索引名称
                    };
                    var createIndex = mongoHelper.CreateIndex(collection, indexKeys, indexOptions);

                    await collection.InsertOneAsync(data);
                

                await Task.Delay(0);
                return (true, string.Empty);
            }
            catch (Exception ex)
            {

                return (false, ex.ToString());
            }

        }


        public class AddResponseModel
        {
            public bool Success { get; set; }

            public string Message { get; set; }

        
        }
        public class GetResponseModel : AddResponseModel
        {
            public FileContentResult? fileContent { get; set; }
        }


        private StreamContent GetStreamContentFromStr(string json)
        {
            var compressedStream = new MemoryStream();

            // 压缩 JSON
            using (var gzipStream = new GZipStream(compressedStream, CompressionLevel.Fastest, leaveOpen: true))
            using (var writer = new StreamWriter(gzipStream, Encoding.UTF8))
            {
                 writer.Write(json);
            }

            compressedStream.Position = 0; // 重置流位置

            // 发送请求
            var content = new StreamContent(compressedStream);
            content.Headers.ContentEncoding.Add("gzip");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return content;
        }


        private static async Task<byte[]> StreamToByteArrayWithBufferAsync(Stream stream, int bufferSize = 81920)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            using var memoryStream = new MemoryStream();

            var buffer = new byte[bufferSize];
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await memoryStream.WriteAsync(buffer, 0, bytesRead);
            }

            return memoryStream.ToArray();
        }


    }
}
