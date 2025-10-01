using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Laboratory;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Extensions
{
    public static class Extensions
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> collection)
        {
            return new ObservableCollection<T>(collection);
        }


        private static readonly string[] _keys = Enumerable.Range(0, 50)
         .Select(i => $"D{100 + i * 2}")
         .ToArray();
        public static Dictionary<string, float> GetParameter()
        {
            return _keys.ToDictionary(k => k, _ => 0f);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="onException"></param>
        public static void TryCatch(this Action action, Action<Exception> onException)
        {
            ArgumentNullException.ThrowIfNull(action);
            ArgumentNullException.ThrowIfNull(onException);

            try
            {
                action();
            }
            catch (Exception ex)
            {
                onException(ex);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <param name="onException"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T? TryCatch<T>(this Func<T> func, Action<Exception> onException, T? defaultValue = default)
        {
            ArgumentNullException.ThrowIfNull(func);
            ArgumentNullException.ThrowIfNull(onException);

            try
            {
                return func();
            }
            catch (Exception ex)
            {
                onException(ex);
                return defaultValue;
            }
        }

        /// <summary>
        /// 记录方法执行时间
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="actionName"></param>
        /// <param name="action"></param>
        public static void RecordActionConsumeTime(this ILogger logger, string actionName, Action action)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(action);
            var startTime = DateTime.Now;
            action();
            logger.LogInformation($"{actionName} Elapsed Time: {DateTime.Now - startTime}");
        }
        public static T RecordActionConsumeTimeAsync<T>(this ILogger logger, string actionName, Func<T> action)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(action);
            var startTime = DateTime.Now;
            var t = action();
            logger.LogInformation($"{actionName} Elapsed Time: {DateTime.Now - startTime}");
            return t;
        }
        public static async Task RecordActionConsumeTimeAsync(this ILogger logger, string actionName, Func<Task> action)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(action);
            var startTime = DateTime.Now;
            await action();
            logger.LogInformation($"{actionName} Elapsed Time: {DateTime.Now - startTime}");
        }
        public static async Task<T> RecordActionConsumeTimeAsync<T>(this ILogger logger, string actionName, Func<Task<T>> action)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(action);
            var startTime = DateTime.Now;
            var t = await action();
            logger.LogInformation($"{actionName} Elapsed Time: {DateTime.Now - startTime}");
            return t;
        }


        public static IServiceCollection AddMangoStorage(this IServiceCollection services)
        {
            services.AddDispose(typeof(ReentrantLockService<>));
            services.AddSingleton<DistributedLockManager>();
            services.AddSingleton<TemporaryTimingWheel>();
            services.AddSingleton<SampleService>();
            services.AddHttpClient();
            services.AddHttpClient<MangoStorage>(client =>
            {
                client.BaseAddress = new Uri("http://192.168.120.45:6100");
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.Timeout = TimeSpan.FromSeconds(60 * 5);
            });
            return services;
        }

        public static IServiceCollection AddDispose(this IServiceCollection services, Type serviceType)
        {
            services.AddSingleton(serviceType);
            return services;
        }
    }
}
