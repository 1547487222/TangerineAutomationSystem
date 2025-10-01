using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Common.Common.Metadatas;
using QStandaedPlatform.Engine.Common.Common.SampleEntitys;
using QStandaedPlatform.Engine.Laboratory;
using QStandaedPlatform.Engine.Extensions;
using System;
using System.Collections.Generic;
using System.Foundation.Modules.Models;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Foundation.Modules.Arms.Pipettes
{
    public class PipetteService
    {
        private readonly
             SampleService _sampleService;
        public PipetteService(SampleService sampleService)
        {
            _sampleService = sampleService;
        }

        public  async Task PipetteAsync(Modular modular,QPosition temp,PipetteType pipetteType,ILogger logger)
        {
            var parameter = Extensions.GetParameter();
            foreach (var item in modular.ModuleFuncCodeParameter.FuncCodeParamterInfos)
            {
                parameter[item.ParameterAddress] = item.ParameterValueFactory.First().Value;
            }
            switch (pipetteType)
            {
                case PipetteType.PipettePick:
                    logger.LogInformation($"移液枪取枪头");
                    parameter["D100"] = temp.X;
                    parameter["D102"] = temp.Y;
                    parameter["D104"] = temp.Z;
                    parameter["D106"] = temp.Z2;
                    parameter["D108"] = temp.Angle;
                    break;
                case PipetteType.PipetteSuck:
                    logger.LogInformation($"移液枪吸液");
                    parameter["D100"] = temp.X;
                    parameter["D102"] = temp.Y;
                    parameter["D104"] = temp.Z;
                    parameter["D106"] = temp.Z2;
                    parameter["D108"] = temp.Angle;
                    break;
                case PipetteType.PipetteDispense:
                    logger.LogInformation($"移液枪排液");
                    parameter["D110"] = temp.X;
                    parameter["D112"] = temp.Y;
                    parameter["D114"] = temp.Z;
                    parameter["D116"] = temp.Z2;
                    parameter["D118"] = temp.Angle;
                    break;
                case PipetteType.PipetteRelease:
                    logger.LogInformation($"移液枪放枪头");
                    parameter["D110"] = temp.X;
                    parameter["D112"] = temp.Y;
                    parameter["D114"] = temp.Z;
                    parameter["D116"] = temp.Z2;
                    parameter["D118"] = temp.Angle;
                    break;
            }
            await modular.WriteParameterAsync(modular.ModuleInfo.ModuleParameterAddress, [.. parameter.Values]);
            if (!modular.VerifyModuleActivityStatus(modular.ModuleInfo.ModuleFuncStateCodeAddress))
            {
                await modular.ModuleExecuteAsync(modular.ModuleInfo.ModuleFuncCodeAddress, (short)modular.ModuleFuncCodeParameter.FuncCode);
            }
            await modular.CheckDoneAsync(modular.ModuleInfo.ModuleFuncStateCodeAddress, modular.ModuleInfo.ModuleFuncCodeAddress);
        }


        public  async Task SampleLiquorReliefAsync(Guid actionId,SampleTaskInfo sampleTaskInfo, Modular modular, QPosition src,QPosition dest, ILogger logger)
        {
            var sampleTraceEntity = _sampleService.GetSampleTrace(sampleTaskInfo.SamplingId);
            sampleTraceEntity.SetModuleInfo(actionId, modular.ModuleFuncCodeParameter);
            sampleTraceEntity.SetBasicInfo(sampleTaskInfo);
            sampleTraceEntity.SetStartTime();
            var parameter = Extensions.GetParameter();
            foreach (var item in modular.ModuleFuncCodeParameter.FuncCodeParamterInfos)
            {
                parameter[item.ParameterAddress] = item.ParameterValueFactory.First().Value;
            }
            logger.LogInformation($"移液枪移液");
            parameter["D100"] = src.X;
            parameter["D102"] = src.Y;
            parameter["D104"] = src.Z;
            parameter["D106"] = src.Z2;
            parameter["D108"] = src.Angle;
            parameter["D110"] = dest.X;
            parameter["D112"] = dest.Y;
            parameter["D114"] = dest.Z;
            parameter["D116"] = dest.Z2;
            parameter["D118"] = dest.Angle;
            sampleTraceEntity.SetInputParameters(parameter);
            try
            {
                await modular.WriteParameterAsync(modular.ModuleInfo.ModuleParameterAddress, [.. parameter.Values]);
                if (!modular.VerifyModuleActivityStatus(modular.ModuleInfo.ModuleFuncStateCodeAddress))
                {
                    await modular.ModuleExecuteAsync(modular.ModuleInfo.ModuleFuncCodeAddress, (short)modular.ModuleFuncCodeParameter.FuncCode);
                }
                await modular.CheckDoneAsync(modular.ModuleInfo.ModuleFuncStateCodeAddress, modular.ModuleInfo.ModuleFuncCodeAddress);
                if (modular.ModuleFuncCodeParameter.ChannelEbrInfos.Count > 0)
                {
                    var values = await modular.ReadEbrDatasAsync(modular.ModuleFuncCodeParameter.ChannelEbrInfos);
                    foreach (var item in values)
                    {
                        foreach (var (key, value, unit) in item.Value)
                        {
                            SampleTaskDataEntity sampleTaskDataEntity = new();
                            sampleTaskDataEntity.EbrValue = value.ToString();
                            sampleTaskDataEntity.EbrUnit = unit;
                            sampleTaskDataEntity.EbrKey = key;
                            sampleTaskDataEntity.EbrKeyDescription = key;
                            sampleTraceEntity.SetEbrData(sampleTaskDataEntity);
                        }
                    }
                }
                sampleTraceEntity.Status = SampleTraceStatus.Success;
            }
            
            catch (Exception ex)
            {
                sampleTraceEntity.AlertMessage = ex.Message;
                sampleTraceEntity.Status = SampleTraceStatus.Alert;
                _sampleService.SaveSampleTrace(sampleTraceEntity.SamplingId);
                throw;
            }
            finally
            {
                sampleTraceEntity.SetEndTime();
                _sampleService.SaveSampleTrace(sampleTraceEntity.SamplingId);
            }
        }

	    public async Task LiquorReliefAsync(Modular modular, QPosition src, QPosition dest, ILogger logger)
        {
            var parameter = Extensions.GetParameter();
            foreach (var item in modular.ModuleFuncCodeParameter.FuncCodeParamterInfos)
            {
                parameter[item.ParameterAddress] = item.ParameterValueFactory.First().Value;
            }
            logger.LogInformation($"移液枪移液");
            parameter["D100"] = src.X;
            parameter["D102"] = src.Y;
            parameter["D104"] = src.Z;
            parameter["D106"] = src.Z2;
            parameter["D108"] = src.Angle;
            parameter["D110"] = dest.X;
            parameter["D112"] = dest.Y;
            parameter["D114"] = dest.Z;
            parameter["D116"] = dest.Z2;
            parameter["D118"] = dest.Angle;
            try
            {
                await modular.WriteParameterAsync(modular.ModuleInfo.ModuleParameterAddress, [.. parameter.Values]);
                if (!modular.VerifyModuleActivityStatus(modular.ModuleInfo.ModuleFuncStateCodeAddress))
                {
                    await modular.ModuleExecuteAsync(modular.ModuleInfo.ModuleFuncCodeAddress, (short)modular.ModuleFuncCodeParameter.FuncCode);
                }
                await modular.CheckDoneAsync(modular.ModuleInfo.ModuleFuncStateCodeAddress, modular.ModuleInfo.ModuleFuncCodeAddress);
            }

            catch (Exception ex)
            {
                throw;
            }
            finally
            {
            }
        }
	    
	
    }
}
