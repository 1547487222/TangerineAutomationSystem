

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using SqlSugar;
using Tangerine.ModuleRunDataTransferApi.Common;
using Serilog;
using Tangerine.ModuleRunDataTransferApi.Models;

namespace Tangerine.ModuleRunDataTransferApi
{
    public class Program
    {

        public static IConfigurationRoot configuration { get; set; }

        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);


            // 方法1：配置Kestrel全局限制
            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.Limits.MaxRequestBodySize = 1024 * 1024 * 1024; // 设置为100MB
            });

            configuration = new ConfigurationBuilder()
      .SetBasePath(AppContext.BaseDirectory) // 设置配置文件的基目录
      .AddJsonFile("./appsettings.json", optional: true, reloadOnChange: true)
       .Build();

            builder.Configuration.AddConfiguration(configuration);

            builder.Services.Configure<AppSettingConfigModel>(builder.Configuration.GetSection("GlobalSets"));


            // 从配置文件读取Serilog配置
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();
           // Log.Information("Starting web application");

            // 使用Serilog代替默认日志系统
            builder.Host.UseSerilog();

            // 添加 Windows 服务支持
            builder.Host.UseWindowsService(options =>
            {
                //options.ServiceName = "DataToMongoWebAPIService"; // 自定义服务名称
            });


            ConfigServices(builder.Services);

            var app = builder.Build();

            Configure(app);

            app.Run();
        }

        private static void ConfigServices(IServiceCollection services)
        {
            services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });

                //// 设置示例模型的时间格式为本地时间
                //c.MapType<DateTime>(() => new OpenApiSchema
                //{
                //    Type = "string",
                //    Example = new OpenApiString(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                //});
            });


            #region 跨域配置
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()    // 允许任何来源
                          .AllowAnyMethod()    // 允许任何 HTTP 方法（GET/POST/PUT/DELETE等）
                          .AllowAnyHeader();  // 允许任何请求头
                });
            });
            #endregion




            #region 读取配置文件

           

            services.AddScoped<MongoClient>(provider => 
            {
                var options = provider.GetRequiredService<IOptions<AppSettingConfigModel>>().Value;
                return new MongoClient(options.NoSqlSet.MongoConfigSet.Standalone);
            });

            services.AddScoped<MongoDbHelper>(r =>
            {
                var options = r.GetRequiredService<IOptions<AppSettingConfigModel>>().Value;
                var mongoClient = r.GetRequiredService<MongoClient>();
                return new MongoDbHelper(mongoClient, options.NoSqlSet.MongoConfigSet);
            });

            #endregion

        }

        public static void Configure(WebApplication app)
        {
          
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => 
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tangerine api v1");
                  
                }); 
            }
            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSerilogRequestLogging(options =>
            {
                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                    diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                };
            });


        }
    }
}
