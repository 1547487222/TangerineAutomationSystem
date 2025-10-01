using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ProtoBuf;
using QStandaedPlatform.Engine.Common.Common.ModuleEntitys;
using System.IO.Compression;
using System.Text;
using Tangerine.ModuleRunDataTransferApi.Common;
using Tangerine.ModuleRunDataTransferApi.Models;
using QStandaedPlatform.Engine.Common.Common.SampleEntitys;
using QStandaedPlatform.Engine.Laboratory;

namespace Tangerine.ModuleRunDataTransferApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DataTransferMongoController : ControllerBase
    {
        private readonly MongoDbHelper _mongoHelper;


         private readonly ILogger<DataTransferMongoController> _logger;

        public DataTransferMongoController(MongoDbHelper mongoHelper, ILogger<DataTransferMongoController> logger)
        {
            _mongoHelper = mongoHelper;
            _logger = logger;
        }

        #region 模块监控数据
        [HttpPost("addModuleRunDataGzip")]
        [DisableRequestSizeLimit] // 允许大文件上传
        public async Task<IActionResult> AddModuleRunDataGZip()
        {
            if (!Request.Headers.ContentEncoding.Contains("gzip"))
            {
                return BadRequest("Content-Encoding must be gzip.");
            }

            try
            {
                using (var gzipStream = new GZipStream(Request.Body, CompressionMode.Decompress))
                using (var reader = new StreamReader(gzipStream, Encoding.UTF8))
                {
                    string json = await reader.ReadToEndAsync();
                    if (string.IsNullOrEmpty(json))
                    {
                        return BadRequest("添加数据为空");

                    }

                    var entity = Newtonsoft.Json.JsonConvert.DeserializeObject<MongoDataAddEntity<ModuleReportRunDataEntity>>(json);

                    var databaseName = GetDatabaseNameByIndex(0);
                    if (!string.IsNullOrEmpty(entity.DatabaseName))
                    {
                        databaseName = entity.DatabaseName;
                    }

                    var task = await AddCommonDataToMongo(databaseName, entity.TableName, entity.AddDataModel);

                    _logger.LogInformation($"添加数据成功,flowId:{entity.TableName},databaseName:{databaseName}");

                    return Ok(new
                    {
                        Success = task.result,
                        Message = task.msg
                    });
                    // 处理 json...
                    return Ok("Data processed.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }


        // POST: DataTransferMongoController/Create
        [HttpPost("addModuleRunData")]
        public async Task<ActionResult> AddModuleRunData([FromBody] FlowRunDataEntity entity)
        {
            var databaseName = GetDatabaseNameByIndex(0);
            if (!string.IsNullOrEmpty(entity.DatabaseName))
            {
                databaseName = entity.DatabaseName;
            }
            try
            {
                if (entity == null)
                {
                    _logger.LogDebug("添加的数据不能为null");
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "添加的数据不能为null"
                    });

                }

                var task = await AddCommonDataToMongo(databaseName, entity.FlowId, entity.ModuleRunData);

                _logger.LogInformation($"添加数据成功,flowId:{entity.FlowId},databaseName:{databaseName}");

                return Ok(new
                {
                    Success = task.result,
                    Message = task.msg
                });
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"添加数据异常,flowId:{entity.FlowId},databaseName:{databaseName}，Exception:{ex.ToString()}");

                return BadRequest(new
                {
                    Success = false,
                    Message = ex.ToString()
                });
            }
            finally
            {
                this.Request.Body.Dispose();
            }
        }


        [HttpPost("getMongoFile")]
        public async Task<IActionResult> GetModuleRunData([FromBody] MongoDataQueryDto queryDto)
        {
            var databaseName = GetDatabaseNameByIndex(0);
            try
            {

                queryDto.DatabaseName = databaseName;

                var ret = await GetDatasFromMongo(queryDto);
                if (!ret.result)
                {
                    _logger.LogDebug($"获取数据,flowId:{queryDto.TableName},databaseName:{databaseName}，Exception:{ret.msg}");
                    return StatusCode(503, ret.msg);
                }
                return ret.file;
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"获取数据,flowId:{queryDto.TableName},databaseName:{databaseName}，Exception:{ex.ToString()}");
                return StatusCode(503, $"获取数据失败:{ex.ToString()}");
            }
        }

        #endregion


        #region 样品追踪数据

        // POST: DataTransferMongoController/Create
        [HttpPost("addSampleTraceData")]
        [DisableRequestSizeLimit] // 允许大文件上传
        public async Task<ActionResult> AddSampleTraceData()
        {
            if (!Request.Headers.ContentEncoding.Contains("gzip"))
            {
                return BadRequest("Content-Encoding must be gzip.");
            }
            var databaseName = GetDatabaseNameByIndex(1);
            var tableName = $"{databaseName}_{DateTime.Now.ToString("yyyy_MM")}";

            var exprireDays = _mongoHelper.MongoConfig.ExpireDays.Split(';')[1] ?? "90";

            try
            {
                using (var gzipStream = new GZipStream(Request.Body, CompressionMode.Decompress))
                using (var reader = new StreamReader(gzipStream, Encoding.UTF8))
                {
                    string json = await reader.ReadToEndAsync();
                    if (string.IsNullOrEmpty(json))
                    {
                        return BadRequest("添加数据为空");

                    }

                    var entity = Newtonsoft.Json.JsonConvert.DeserializeObject<SampleTraceEntity>(json);
                    var keyModel = new SampleTraceKeyEntity
                    {
                        CreateDate = DateOnly.FromDateTime(DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local)),
                        CreateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local),
                        SamplingTaskId = entity.SamplingTaskId,
                        SamplingId = entity.SamplingId,
                        GlobalSampleId = entity.SamplingId,
                        ProcessflowId = entity.ProcessflowId,
                        PlatformName = entity.PlatformName,
                        IsAlarm = !string.IsNullOrEmpty(entity.AlertMessage)
                    };

                    var byteModel = new SampleTraceByteDataEntity
                    {
                        SampleType = entity.SampleType,
                        SampleSn = entity.SampleSn,
                        PlatformTaskId = entity.PlatformTaskId,
                        LabwareName = entity.LabwareName,
                        ModuleSerialNumber = entity.ModuleSerialNumber,
                        PlatformId = entity.PlatformId,
                        SampleName = entity.SampleName,
                        SampleRemarks = entity.SampleRemarks,
                        ModuleName = entity.ModuleName,
                        ModuleActionId = entity.ModuleActionId,
                        ModuleActionDescription = entity.ModuleActionDescription,
                        AlertMessage = entity.AlertMessage,
                        StartTime = entity.StartTime,
                        EndTime = entity.EndTime,
                        Status = entity.Status,
                        TaskEbrDataEntities = entity.TaskEbrDataEntities
                    };
                    var text = Newtonsoft.Json.JsonConvert.SerializeObject(byteModel);
                    var zip = CompressWithDeflate(text);
                    keyModel.ByteDatas = zip;

                    var collection = _mongoHelper.GetCollection<SampleTraceKeyEntity>(databaseName, tableName);

                    var timeIndexModel = new IndexModel<SampleTraceKeyEntity> 
                    {
                        indexKeys = Builders<SampleTraceKeyEntity>.IndexKeys
                           .Descending(p => p.CreateTime),
                        option = new CreateIndexOptions
                        {
                            Unique = true,
                            Background = true,
                            Name = $"{tableName}_CreateTime",// 自定义索引名称
                            ExpireAfter = TimeSpan.FromDays(Convert.ToDouble(exprireDays))
                        }
                    };

                    var unionIndexModel = new IndexModel<SampleTraceKeyEntity>
                    {
                        indexKeys = Builders<SampleTraceKeyEntity>.IndexKeys
                           .Ascending(p => p.SamplingTaskId)
                           .Ascending(p => p.GlobalSampleId)
                           .Ascending(p =>p.SamplingId)
                           .Ascending(p =>p.ProcessflowId)
                           .Ascending(p => p.IsAlarm),
                        option = new CreateIndexOptions
                        {
                            Background = true,
                            Name = $"{tableName}_SamplingTaskId_GlobalSampleId_SamplingId_ProcessflowId_IsAlarm"// 自定义索引名称
                        }
                    };

                    var createIndex = _mongoHelper.CreateIndexs(collection, timeIndexModel, unionIndexModel);

                    await collection.InsertOneAsync(keyModel);

                    _logger.LogInformation($"添加数据成功,tableName:{tableName},databaseName:{databaseName}");

                    return Ok(new
                    {
                        Success = true,
                        Message = "ok"
                    });
                  
                }
             
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"添加数据异常,tableName:{tableName},databaseName:{databaseName}，Exception:{ex.ToString()}");

                return BadRequest(new
                {
                    Success = false,
                    Message = ex.ToString()
                });
            }
            finally
            {
                this.Request.Body.Dispose();
            }
        }


        [HttpPost("getSampleTraceData")]
        public async Task<IActionResult> GetSampleTraceData([FromBody] SampleTraceQueryDto queryDto)
        {
            var databaseName = GetDatabaseNameByIndex(1);
            var tableName = $"{databaseName}_{DateTime.Now.ToString("yyyy_MM")}";
            try
            {

                var ret = await GetSampleTraceDatas(queryDto);
                if (!ret.result)
                {
                    _logger.LogDebug($"获取EBR数据,tableName:{tableName},databaseName:{databaseName}，Exception:{ret.msg}");
                    return StatusCode(503, ret.msg);
                }
                return ret.file;
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"获取EBR数据,tableName:{tableName},databaseName:{databaseName}，Exception:{ex.ToString()}");
                return StatusCode(503, $"获取EBR数据:{ex.ToString()}");
            }
        }

        #endregion


        #region private methods
        private async Task<(bool result, string msg)> AddCommonDataToMongo<T>(string DatabaseName,string tableName, T tData,string keyInfo = "") where T: class
        {
            try
            {
                _logger.LogInformation($"当前数据库:{DatabaseName}");

                var text = Newtonsoft.Json.JsonConvert.SerializeObject(tData);
                var zip = CompressWithDeflate(text);

                var data = new CommonkeyInfoEntity
                {
                    CreateDate = DateOnly.FromDateTime(DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local)),
                    CreateTime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local),
                    KeyInfo = string.IsNullOrEmpty(keyInfo) ? Guid.NewGuid().ToString() : keyInfo,
                    ByteDatas = zip
                };
              

                var collection = _mongoHelper.GetCollection<CommonkeyInfoEntity>(DatabaseName,tableName);

                var indexKeys = Builders<CommonkeyInfoEntity>.IndexKeys
                       .Ascending(p => p.CreateTime);

                var indexOptions = new CreateIndexOptions
                {
                    Unique = true,
                    Name = $"{tableName}_CreateTime"// 自定义索引名称
                };
                var createIndex = _mongoHelper.CreateIndex(collection, indexKeys, indexOptions);

                await collection.InsertOneAsync(data);

                await Task.Delay(0);
                return (true, string.Empty);
            }
            catch (Exception ex)
            {

                return (false, ex.ToString());
            }

        }


        private async Task<(bool result, string msg, FileContentResult? file)> GetDatasFromMongo(MongoDataQueryDto queryDto)
        {
            try
            {

                if (queryDto == null)
                {
                    _logger.LogWarning("查询Dto为空");
                    return (false, "查询Dto为空", null);
                }

                if (string.IsNullOrEmpty(queryDto.TableName))
                {
                    _logger.LogWarning("流程ID为空");
                    return (false, "流程ID为空", null);
                }

                var filter = Builders<CommonkeyInfoEntity>.Filter.Empty;
                if (queryDto.StartDate == null)
                {
                    filter = Builders<CommonkeyInfoEntity>.Filter.Empty;
                }
                else
                {
                    if (queryDto.EndDate == null)
                    {
                        filter = Builders<CommonkeyInfoEntity>.Filter.And(
                            Builders<CommonkeyInfoEntity>.Filter.Gte(x => x.CreateTime, Convert.ToDateTime($"{queryDto.StartDate.Value.ToString("yyyy-MM-dd")} 00:00:00").ToUniversalTime()),
                            Builders<CommonkeyInfoEntity>.Filter.Lte(x => x.CreateTime, Convert.ToDateTime($"{queryDto.StartDate.Value.ToString("yyyy-MM-dd")} 23:59:59").ToUniversalTime())
                        );

                    }
                    else
                    {
                        filter = Builders<CommonkeyInfoEntity>.Filter.And(
                          Builders<CommonkeyInfoEntity>.Filter.Gte(x => x.CreateTime, Convert.ToDateTime($"{queryDto.StartDate.Value.ToString("yyyy-MM-dd")} 00:00:00").ToUniversalTime()),
                          Builders<CommonkeyInfoEntity>.Filter.Lte(x => x.CreateTime, Convert.ToDateTime($"{queryDto.EndDate.Value.ToString("yyyy-MM-dd")} 23:59:59").ToUniversalTime())
                      );
                    }
                }
                if (!string.IsNullOrEmpty(queryDto.KeyInfo))
                {
                    filter = Builders<CommonkeyInfoEntity>.Filter.And(
                          Builders<CommonkeyInfoEntity>.Filter.Eq(x => x.KeyInfo, queryDto.KeyInfo
                      ));
                }

            ReRead: var collection = _mongoHelper.GetCollection<CommonkeyInfoEntity>(queryDto.DatabaseName, queryDto.TableName);

                var index = 0;
                var datas = await collection.Find(filter).SortBy(x => x.CreateTime).ToCursorAsync();

                var tempList = new List<byte[]>();

                while (await datas.MoveNextAsync())
                {
                    foreach (var item in datas.Current)
                    {
                        tempList.Add(item.ByteDatas);
                    }
                }
                if (tempList.Count == 0)
                {
                    index++;
                    if (index < 5)
                    {
                        goto ReRead;
                    }
                    else
                    {
                        return (false, $"读取MongoDB数据错误，读取到数据数量为0", null);
                    }

                }
                var result = SerializeByteArrays(tempList);
                var file = File(result, "application/octet-stream");
                _logger.LogInformation($"获取数据成功:flowId:{queryDto.TableName},databaseName:{queryDto.DatabaseName},startdate:{queryDto.StartDate},endDate:{queryDto.EndDate}");
                return (true, "ok", file);
            }
            catch (MongoException ex)
            {
                _logger.LogError($"获取数据失败:flowId:{queryDto.TableName},databaseName:{queryDto.DatabaseName},startdate:{queryDto.StartDate},endDate:{queryDto.EndDate}，MongoDB 错误,Exception:{ex.ToString()}");
                return (false, $"MongoDB 错误: {ex.Message}", null);
            }
            catch (Exception ex)
            {
                _logger.LogError($"获取数据失败:flowId:{queryDto.TableName},databaseName:{queryDto.DatabaseName},startdate:{queryDto.StartDate},endDate:{queryDto.EndDate}，Exception:{ex.ToString()}");
                return (false, ex.ToString(), null);
            }

        }


        private async Task<(bool result, string msg, FileContentResult? file)> GetSampleTraceDatas(SampleTraceQueryDto queryDto)
        {
            var databaseName = GetDatabaseNameByIndex(1);
            var tableName = $"{databaseName}_{DateTime.Now.ToString("yyyy_MM")}";
            try
            {
                #region 条件拼接
                if (queryDto == null)
                {
                    _logger.LogWarning("查询Dto为空");
                    return (false, "查询Dto为空", null);
                }
                if (queryDto.SamplingId == null && queryDto.SamplingTaskId == null && queryDto.GlobalSampleId == null)
                {
                    _logger.LogWarning("查询的样品ID和任务ID不能为空");
                    return (false, "查询的样品ID和任务ID不能为空", null);
                }

                var filter = Builders<SampleTraceKeyEntity>.Filter.Empty;

                if (queryDto.SamplingTaskId != null)
                {
                    filter = Builders<SampleTraceKeyEntity>.Filter.And(
                          Builders<SampleTraceKeyEntity>.Filter.Eq(x => x.SamplingTaskId, queryDto.SamplingTaskId
                      ));
                }
                if (queryDto.SamplingId != null)
                {
                    filter = Builders<SampleTraceKeyEntity>.Filter.And(
                          Builders<SampleTraceKeyEntity>.Filter.Eq(x => x.SamplingId, queryDto.SamplingId
                      ));
                }
                if (queryDto.GlobalSampleId != null)
                {
                    filter = Builders<SampleTraceKeyEntity>.Filter.And(
                          Builders<SampleTraceKeyEntity>.Filter.Eq(x => x.GlobalSampleId, queryDto.GlobalSampleId
                      ));
                }
                if (!string.IsNullOrEmpty(queryDto.ProcessflowId))
                {
                    filter = Builders<SampleTraceKeyEntity>.Filter.And(
                          Builders<SampleTraceKeyEntity>.Filter.Eq(x => x.ProcessflowId, queryDto.ProcessflowId
                      ));
                }
                if (!string.IsNullOrEmpty(queryDto.PlatformName))
                {
                    filter = Builders<SampleTraceKeyEntity>.Filter.And(
                          Builders<SampleTraceKeyEntity>.Filter.Eq(x => x.PlatformName, queryDto.PlatformName
                      ));
                }
                if (queryDto.StartDate != null)
                {
                    filter = Builders<SampleTraceKeyEntity>.Filter.And(
                            Builders<SampleTraceKeyEntity>.Filter.Gte(x => x.CreateTime, Convert.ToDateTime($"{queryDto.StartDate.Value.ToString("yyyy-MM-dd")} 00:00:00").ToUniversalTime()));
                }
                if (queryDto.EndDate != null)
                {
                    filter = Builders<SampleTraceKeyEntity>.Filter.And(
                            Builders<SampleTraceKeyEntity>.Filter.Lte(x => x.CreateTime, Convert.ToDateTime($"{queryDto.EndDate.Value.ToString("yyyy-MM-dd")} 23:59:59").ToUniversalTime()));
                }
            #endregion

            ReRead: var collection = _mongoHelper.GetCollection<SampleTraceKeyEntity>(databaseName, tableName);

                var index = 0;
                var datas = await collection.Find(filter)
                    .SortBy(x => x.ProcessflowId)
                    .SortBy(x => x.SamplingTaskId)
                    .SortBy(x => x.GlobalSampleId)
                    .SortBy(x => x.SamplingId)
                    .SortBy(x => x.CreateTime)
                    .ToCursorAsync();

                var tempList = new List<SampleTraceEntity>();

                while (await datas.MoveNextAsync())
                {
                    foreach (var item in datas.Current)
                    {
                        var byteModel = Newtonsoft.Json.JsonConvert.DeserializeObject<SampleTraceByteDataEntity>( DecompressWithDeflate(item.ByteDatas));

                        var tempModel = new SampleTraceEntity
                        {
                            ProcessflowId = item.ProcessflowId,
                            PlatformId = byteModel!.PlatformId,
                            PlatformName = item.PlatformName,
                            PlatformTaskId = byteModel.PlatformTaskId,
                            SamplingTaskId = item.SamplingTaskId,
                            SamplingId = item.SamplingId,
                            SampleSn = byteModel.SampleSn,
                            SampleName = byteModel.SampleName,
                            SampleType = byteModel.SampleType,
                            SampleRemarks = byteModel.SampleRemarks,
                            LabwareName = byteModel.LabwareName,
                            ModuleName = byteModel.ModuleName,

                            ModuleActionId = byteModel.ModuleActionId,
                            ModuleSerialNumber = byteModel.ModuleSerialNumber,
                            ModuleActionDescription = byteModel.ModuleActionDescription,
                            InputParametersJson = byteModel.InputParametersJson,
                            AlertMessage = byteModel.AlertMessage,
                            StartTime = byteModel.StartTime,
                            EndTime = byteModel.EndTime,
                            Status = byteModel.Status,
                            TaskEbrDataEntities = byteModel.TaskEbrDataEntities
                           
                        };
                        tempList.Add(tempModel);
                    }
                }
                if (tempList.Count == 0)
                {
                    index++;
                    if (index < 5)
                    {
                        goto ReRead;
                    }
                    else
                    {
                        return (false, $"读取MongoDB数据错误，读取到数据数量为0", null);
                    }

                }
                tempList = tempList.OrderBy(s => s.PlatformName).ToList();



                var text = Newtonsoft.Json.JsonConvert.SerializeObject(tempList);

                var result = CompressWithDeflate(text);


                
                  var file = File(result, "application/octet-stream");
                _logger.LogInformation($"获取数据成功:tableName:{tableName},databaseName:{databaseName},startdate:{queryDto.StartDate},endDate:{queryDto.EndDate}");
                return (true, "ok", file);
            }
            catch (MongoException ex)
            {
                _logger.LogError($"获取数据失败:tableName:{tableName},databaseName:{databaseName},startdate:{queryDto.StartDate},endDate:{queryDto.EndDate}，MongoDB 错误,Exception:{ex.ToString()}");
                return (false, $"MongoDB 错误: {ex.Message}", null);
            }
            catch (Exception ex)
            {
                _logger.LogError($"获取数据失败:tableName:{tableName},databaseName:{databaseName},startdate:{queryDto.StartDate},endDate:{queryDto.EndDate}，Exception:{ex.ToString()}");
                return (false, ex.ToString(), null);
            }

        }

        /// <summary>
        /// 字符串压缩
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private byte[] CompressWithDeflate(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            using var memoryStream = new MemoryStream();
            using (var deflateStream = new DeflateStream(memoryStream, CompressionLevel.Optimal))
            {
                deflateStream.Write(bytes, 0, bytes.Length);
            }
            return memoryStream.ToArray();
        }
       /// <summary>
       /// 解压成字符串
       /// </summary>
       /// <param name="compressedBytes"></param>
       /// <returns></returns>
        private string DecompressWithDeflate(byte[] compressedBytes)
        {
            using var memoryStream = new MemoryStream(compressedBytes);
            using var outputStream = new MemoryStream();
            using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress))
            {
                deflateStream.CopyTo(outputStream);
            }
            return Encoding.UTF8.GetString(outputStream.ToArray());
        }

        // 2. protobuf序列化方法
        private byte[] SerializeByteArrays(List<byte[]> byteArrays)
        {
            var container = new ByteArraysContainer { ByteArrays = byteArrays };
            using var stream = new MemoryStream();
            Serializer.Serialize(stream, container);
            return stream.ToArray();
        }

        // 3. protobuf反序列化方法
        private List<byte[]> DeserializeByteArrays(byte[] data)
        {
            using var stream = new MemoryStream(data);
            var container = Serializer.Deserialize<ByteArraysContainer>(stream);
            return container.ByteArrays;
        }

        private string GetDatabaseNameByIndex(int index)
        {
            var nameArray = _mongoHelper.DatabaseName.Split(';') ;

            if (index >= nameArray.Length)
            {
                index = nameArray.Length - 1;
            }

            return nameArray[index];
        }
        #endregion

    }

    [ProtoContract]
    public class ByteArraysContainer
    {
        [ProtoMember(1)]
        public List<byte[]> ByteArrays { get; set; } = new List<byte[]>();
    }
}
