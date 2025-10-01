using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProtoBuf;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.ModuleEntitys;
using QStandaedPlatform.Engine.Common.Common.SampleEntitys;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
    
namespace QStandaedPlatform.Engine.Laboratory
{
    /// <summary>
    /// 对接王工的MangoWebAPI
    /// </summary>
    public class MangoStorage:IDisposable
    {

        private readonly HttpClient _client;
        private readonly
            CompressionService _compressionService = new();
        private readonly ILogger<MangoStorage>? _logger;
        public MangoStorage(HttpClient httpClient)
        {
            _client = httpClient;
            _logger = LoggerProviderManager.GetLoggerFactory().CreateLogger<MangoStorage>();
            _logger?.LogInformation("MangoStorage构造初始化，client：{client}", _client);
        }
        public async Task SaveModuleRunData(FlowRunDataEntity flowRunData)
        {
            var json = JsonConvert.SerializeObject(flowRunData);

            var jsonContent = new StringContent(
                 json,
                 Encoding.UTF8,
                 "application/json"
             );

            // 2. 调用 API
            var response = await _client.PostAsync("api/DataTransferMongo/addModuleRunData", jsonContent);


            // 3. 验证响应
            var code = response.IsSuccessStatusCode;

            if (!code)
                throw new Exception("api/DataTransferMongo/addModuleRunData  写入模块运行数据异常");
        }


        public async Task<List<ModuleReportRunDataEntity>> GetFlowRunData(FlowRunDataQueryDto queryDto)
        {
            var jsonContent = new StringContent(
                       JsonConvert.SerializeObject(queryDto),
                       Encoding.UTF8,
                       "application/json"
                   );

            // 2. 调用 API
            var response = await _client.PostAsync("api/DataTransferMongo/getMongoFile", jsonContent);

            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync();

            var container = Serializer.Deserialize<ByteArraysContainer>(stream);
            var byteArray = container.ByteArrays;
            var tempList = new List<ModuleReportRunDataEntity>();
            foreach (var item in byteArray)
            {
                var text = _compressionService.DecompressWithDeflate(item);
                var tempEntity = JsonConvert.DeserializeObject<ModuleReportRunDataEntity>(text);
                if (tempEntity != null)
                    tempList.Add(tempEntity);
            }
            return tempList;
        }


        public async Task SaveSampleTraceData(SampleTraceEntity entity)
        {
            var json = JsonConvert.SerializeObject(entity);
            var content = GetStreamContentFromStr(json);

            // 2. 调用 API
            var response = await _client.PostAsync("api/DataTransferMongo/addSampleTraceData", content);

            // 3. 验证响应
            var code = response.IsSuccessStatusCode;
            if (!code)
                throw new Exception("api/DataTransferMongo/addSampleTraceData  写入样品追踪数据异常");
        }

        public async Task<List<SampleTraceEntity>> GetSampleTraceDatas(SampleTraceQueryDto dto)
        {
           var list=new List<SampleTraceEntity>();
            try
            {
              
                var jsonContent = new StringContent(
                           JsonConvert.SerializeObject(dto),
                           Encoding.UTF8,
                           "application/json"
                       );
               
                var response = await _client.PostAsync("api/DataTransferMongo/getSampleTraceData", jsonContent);

                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync();

                var byteData = await StreamToByteArrayWithBufferAsync(stream);

                var strList = _compressionService.DecompressWithDeflate(byteData);

                var tempList = JsonConvert.DeserializeObject<List<SampleTraceEntity>>(strList);
                if (tempList != null)
                list.AddRange(tempList);
             
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine("http响应超时");
                return list;
            }
            catch (Exception ex)
            {

                throw;
            }
            return list;
        }


        public void Dispose()
        {
            _client.Dispose();
        }


        #region private method


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

            while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
            {
                await memoryStream.WriteAsync(buffer.AsMemory(0, bytesRead));
            }

            return memoryStream.ToArray();
        }
        #endregion
    }


    public class FlowRunDataEntity
    {
        /// <summary>
        /// 数据库名称，格式不带'-'符号
        /// </summary>
        public string DatabaseName { get; set; } = string.Empty;
        /// <summary>
        /// 数据表名称：一般为flowId,格式不带'-'符号
        /// </summary>
        public string FlowId { get; set; }= string.Empty;

        /// <summary>
        /// 模块运行数据
        /// </summary>

        public ModuleReportRunDataEntity ModuleRunData { get; set; }
    }

    public class FlowRunDataQueryDto
    {
        /// <summary>
        /// 数据库名称，格式不带'-'符号
        /// </summary>
        public string DatabaseName { get; set; } = string.Empty;
        /// <summary>
        /// 数据表名称：一般为flowId,格式不带'-'符号
        /// </summary>
        public string FlowId { get; set; } = string.Empty;
        /// <summary>
        /// 起始时间，如果为空，返回所有
        /// </summary>
        public DateOnly? StartDate { get; set; }
        /// <summary>
        /// 结束时间，如果为空，返回StartDate当天
        public DateOnly? EndDate { get; set; }
    }


    public class SampleTraceQueryDto
    {
        public long? SamplingId { get; set; }

        public long? GlobalSampleId { get; set; }

        public long? SamplingTaskId { get; set; }
        public string ProcessflowId { get; set; } = string.Empty;
        public string PlatformName { get; set; } = string.Empty;

        /// 起始时间，如果为空，返回所有
        /// </summary>
        public DateOnly? StartDate { get; set; }
        /// <summary>
        /// 结束时间，如果为空，返回StartDate当天
        public DateOnly? EndDate { get; set; }
    }

    [ProtoContract]
    public class ByteArraysContainer
    {
        [ProtoMember(1)]
        public List<byte[]> ByteArrays { get; set; } = [];
    }
}
