using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QStandaedPlatform.Engine.Components;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common
{

    /// <summary>
    /// 批处理器
    /// </summary>
    public class BatchProcessor
    {
        private readonly HashSet<int> processedIndexes;
        private Func<int, Dictionary<string, object>, Task>? processStep;
        private Func<int, int, Dictionary<string, object>, Task>? processStep2;
        private Func<int, Task<bool>>? validationStep;
        private Action<int, Exception>? errorHandler;
   
        public BatchProcessor()
        {
            processedIndexes = [];
        }

        public Dictionary<string,object> Context { get; set; } = [];

        public BatchProcessor SetProcessStep(Func<int, Dictionary<string, object>, Task> processStep)
        {
            this.processStep = processStep;
            return this;
        }

        public BatchProcessor SetProcessStep(Func<int,int, Dictionary<string, object>, Task> processStep)
        {
            this.processStep2 = processStep;
            return this;
        }


        public BatchProcessor SetValidationStep(Func<int, Task<bool>> validationStep)
        {
            this.validationStep = validationStep;
            return this;
        }

        public BatchProcessor SetErrorHandler(Action<int, Exception> errorHandler)
        {
            this.errorHandler = errorHandler;
            return this;
        }
        public async Task<bool> ProcessBatchAsync(int batchSize, Dictionary<string, object> contexts, CancellationToken cancellationToken = default)
        {

            foreach (var context in contexts)
            {
                Context[context.Key] = context.Value;
            }
            for (int i = 0; i < batchSize; i++)
            {
                if (processedIndexes.Contains(i))
                    continue;

                try
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (validationStep != null && !await validationStep(i))
                    {
                        processedIndexes.Add(i);
                        continue;
                    }
                    await ProcessStepWithRetry(i);
                    processedIndexes.Add(i);
                }
                catch (Exception ex)
                {
                    HandleError(i, ex);
                    throw;
                }

            }
            Reset();
            return true;
        }

        private async Task ProcessStepWithRetry(int i)
        {
            try
            {
                if (processStep != null)
                {
                    await processStep(i, Context);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void HandleError(int i, Exception ex)
        {
            errorHandler?.Invoke(i, ex);
        }

        public void Reset()
        {
            processedIndexes.Clear();
        }

        public IReadOnlyCollection<int> GetProcessedIndexes()
        {
            return processedIndexes;
        }
    }

}
