
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
using System.Threading;
using System.Threading.Tasks;

namespace System.Foundation.Modules.Arms
{
    public class ArmService
    {
        private readonly SampleService _sampleService;
        public ArmService(SampleService sampleService)
        {
            _sampleService = sampleService;
        }
        public  async Task ArmTransport( QPosition startPos, QPosition endPos, ClawModel claw, TrayModArmData armData, Modular modular, CancellationToken cancellationToken)
        {
            var parameter = Extensions.GetParameter();
            foreach (var item in modular.ModuleFuncCodeParameter.FuncCodeParamterInfos)
            {
                parameter[item.ParameterAddress] = item.ParameterValueFactory.First().Value;
            }
            startPos.X += armData.X_Start_Offset;
            startPos.Y += armData.Y_Start_Offset;
            startPos.Z += armData.Z_Start_Offset;
            startPos.Z2 += armData.Z2_Start_Offset;
            startPos.Angle += armData.R_Start_Offset;
            endPos.X += armData.X_End_Offset;
            endPos.Y += armData.Y_End_Offset;
            endPos.Z += armData.Z_End_Offset;
            endPos.Z2 += armData.Z2_End_Offset;
            endPos.Angle += armData.R_End_Offset;
            parameter["D100"] = startPos.X;
            parameter["D102"] = startPos.Y;
            parameter["D104"] = startPos.Z;
            parameter["D106"] = startPos.Z2;
            parameter["D108"] = startPos.Angle;
            parameter["D110"] = endPos.X;
            parameter["D112"] = endPos.Y;
            parameter["D114"] = endPos.Z;
            parameter["D116"] = endPos.Z2;
            parameter["D118"] = endPos.Angle;

            if (claw != null && claw.FromOpenPos != 0)
            {
                parameter["D130"] = claw.FromOpenPos;
                parameter["D132"] = claw.FromAngle;
                parameter["D134"] = claw.ToOpenPos;
                parameter["D136"] = claw.ToAngle;
            }
            await modular.WriteParameterAsync([.. parameter.Values]);
            if (!modular.VerifyModuleActivityStatus())
            {
                await modular.ModuleExecuteAsync();
            }
            await modular.CheckModuleDoneAsync(cancellationToken);
        }
        public  async Task SampleArmTransport(Guid actionId,SampleTaskInfo sampleTaskInfo, QPosition startPos,QPosition endPos,ClawModel claw, TrayModArmData armData, Modular modular, CancellationToken cancellationToken)
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
            startPos.X += armData.X_Start_Offset;
            startPos.Y += armData.Y_Start_Offset;
            startPos.Z += armData.Z_Start_Offset;
            startPos.Z2 += armData.Z2_Start_Offset;
            startPos.Angle += armData.R_Start_Offset;
            endPos.X += armData.X_End_Offset;
            endPos.Y += armData.Y_End_Offset;
            endPos.Z += armData.Z_End_Offset;
            endPos.Z2 += armData.Z2_End_Offset;
            endPos.Angle += armData.R_End_Offset;
            parameter["D100"] = startPos.X;
            parameter["D102"] = startPos.Y;
            parameter["D104"] = startPos.Z;
            parameter["D106"] = startPos.Z2;
            parameter["D108"] = startPos.Angle;
            parameter["D110"] = endPos.X;
            parameter["D112"] = endPos.Y;
            parameter["D114"] = endPos.Z;
            parameter["D116"] = endPos.Z2;
            parameter["D118"] = endPos.Angle;
            if (claw != null && claw.FromOpenPos != 0)
            {
                parameter["D130"] = claw.FromOpenPos;
                parameter["D132"] = claw.FromAngle;
                parameter["D134"] = claw.ToOpenPos;
                parameter["D136"] = claw.ToAngle;
            }
            sampleTraceEntity.SetInputParameters(parameter);
            try
            {
                await modular.WriteParameterAsync([.. parameter.Values]);
                if (!modular.VerifyModuleActivityStatus())
                {
                    await modular.ModuleExecuteAsync();
                }
                await modular.CheckModuleDoneAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                sampleTraceEntity.SetAlertMessage(ex.ToString());
                throw;
            }
            finally
            {
                sampleTraceEntity.SetEndTime();
                sampleTraceEntity.TaskEbrDataEntities.Clear();
                _sampleService.SaveSampleTrace(sampleTraceEntity.SamplingId);
            }
        }

        public  async Task<string> SampleArmWithScanTransport(Guid actionId,SampleTaskInfo sampleTaskInfo, QPosition startPos,QPosition scanPos, QPosition endPos, ClawModel claw, TrayModArmData armData, Modular modular, CancellationToken cancellationToken)
        {
            var canCode = string.Empty;
            var sampleTraceEntity = _sampleService.GetSampleTrace(sampleTaskInfo.SamplingId);
            sampleTraceEntity.SetModuleInfo(actionId, modular.ModuleFuncCodeParameter);
            sampleTraceEntity.SetBasicInfo(sampleTaskInfo);
            sampleTraceEntity.SetStartTime();
            var parameter = Extensions.GetParameter();
            foreach (var item in modular.ModuleFuncCodeParameter.FuncCodeParamterInfos)
            {
                parameter[item.ParameterAddress] = item.ParameterValueFactory.First().Value;
            }
            startPos.X += armData.X_Start_Offset;
            startPos.Y += armData.Y_Start_Offset;
            startPos.Z += armData.Z_Start_Offset;
            startPos.Z2 += armData.Z2_Start_Offset;
            startPos.Angle += armData.R_Start_Offset;
            endPos.X += armData.X_End_Offset;
            endPos.Y += armData.Y_End_Offset;
            endPos.Z += armData.Z_End_Offset;
            endPos.Z2 += armData.Z2_End_Offset;
            endPos.Angle += armData.R_End_Offset;
            parameter["D100"] = startPos.X;
            parameter["D102"] = startPos.Y;
            parameter["D104"] = startPos.Z;
            parameter["D106"] = startPos.Z2;
            parameter["D108"] = startPos.Angle;
            parameter["D110"] = endPos.X;
            parameter["D112"] = endPos.Y;
            parameter["D114"] = endPos.Z;
            parameter["D116"] = endPos.Z2;
            parameter["D118"] = endPos.Angle;
            parameter["D120"] = scanPos.X;
            parameter["D122"] = scanPos.Y;
            parameter["D124"] = scanPos.Z;
            parameter["D126"] = scanPos.Z2;
            parameter["D128"] = scanPos.Angle;
            if (claw != null && claw.FromOpenPos != 0)
            {
                parameter["D130"] = claw.FromOpenPos;
                parameter["D132"] = claw.FromAngle;
                parameter["D134"] = claw.ToOpenPos;
                parameter["D136"] = claw.ToAngle;
            }
            sampleTraceEntity.SetInputParameters(parameter);
            try
            {
                await modular.WriteParameterAsync([.. parameter.Values]);
                if (!modular.VerifyModuleActivityStatus())
                {
                    await modular.ModuleExecuteAsync();
                }
                await modular.CheckModuleDoneAsync(cancellationToken);
                var sn = await modular.ReadCodeAsync();
                sampleTraceEntity.SetSampleSn(sn);
                canCode = sn;
                sampleTraceEntity.TaskEbrDataEntities.Clear();
                var ebrDatas = await modular.ReadEbrDatasAsync();
                if (ebrDatas.Count != 0)
                {
                    foreach (var (key, value, unit) in ebrDatas.First().Value)
                    {
                        SampleTaskDataEntity sampleTaskDataEntity = new()
                        {
                            EbrValue = value.ToString(),
                            EbrKey = key,
                            EbrKeyDescription = key,
                            EbrUnit = unit,
                            RecordTime = DateTime.Now
                        };
                        sampleTraceEntity.SetEbrData(sampleTaskDataEntity);
                    }
                }
            }
            catch (Exception ex)
            {
                sampleTraceEntity.SetAlertMessage(ex.ToString());
                throw;
            }
            finally
            {
                sampleTraceEntity.SetEndTime();
                _sampleService.SaveSampleTrace(sampleTraceEntity.SamplingId);
            }
            return canCode;
        }
    }
}
