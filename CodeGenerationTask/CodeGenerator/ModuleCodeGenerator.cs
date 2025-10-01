using CodeGenerationTask.BqjxModuleDefs;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CodeGenerationTask.CodeGenerator
{
    public static class ModuleCodeGenerator
    {
        /// <summary>
        /// 生成模块基类代码
        /// </summary>
        /// <param name="bqjxModuleDef"></param>
        /// <returns></returns>
        public static string GenerateModuleBaseCode(BqjxModuleDef bqjxModuleDef)
        {
            StringBuilder sb = new();

            //命名空间
            sb.AppendLine("using BQJX.Core.Common.Common.JsonAccess;");
            sb.AppendLine("using BQJX.Core.Common.Interface;");
            sb.AppendLine("using BQJX.Modules.Common;");
            sb.AppendLine("using BQJX.Core.Common.Logger;");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Text;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using BQJX.Core.Common.Attributes;");
            sb.AppendLine("using BQJX.Modules.Mod.Base;");
            sb.AppendLine();
            sb.AppendLine("namespace " + bqjxModuleDef.NamespaceName);
            sb.AppendLine("{");

            // 类头部注释
            sb.AppendLine("/// <summary>");
            sb.AppendLine("/// " + bqjxModuleDef.BqjxModuleClassDef.ModuleClassDescription);
            sb.AppendLine("/// </summary>");
            sb.AppendLine("    [Module]");
            // 类声明
            sb.AppendLine($"public abstract class {bqjxModuleDef.BqjxModuleClassDef.ModuleClassName} : {bqjxModuleDef.BqjxModuleClassDef.ModuleBaseClassName}");
            sb.AppendLine("{");

            // 构造函数
            //sb.AppendLine($"    public {bqjxModuleClassDef.ModuleClassName}(IModuleEntity moduleEntity) : base()");
            //sb.AppendLine("    {");
            //sb.AppendLine("        this.ModuleName = moduleEntity.ModuleName;");
            //sb.AppendLine("        this.PlatFormName = moduleEntity.PlatFormName;");
            //sb.AppendLine("        _plc = moduleEntity.PLC;");
            //sb.AppendLine("        _autoCtlInfo = new AutoControlInfo();");
            //sb.AppendLine($"        _logger = MyLoggerFactory.GetLogger(typeof({bqjxModuleClassDef.ModuleClassName}));");
            //sb.AppendLine("    }");
            sb.AppendLine("        protected ModuleControlInfo _moduleControlInfo;");
            sb.AppendLine("        protected Dictionary<string,float> _parameters = new Dictionary<string, float>();");
            sb.AppendLine();

            //构造函数
            sb.AppendLine($"        protected {bqjxModuleDef.BqjxModuleClassDef.ModuleClassName}()");
            sb.AppendLine("        {");
            var orderParamDefs = bqjxModuleDef.BqjxModuleClassDef.BqjxModuleMethodDefs.Where(p => !p.IsIndependent).SelectMany(p => p.BqjxModuleMethodParamDefs).OrderBy(p => p.Index);
            var tempKeys = new HashSet<string>();
            foreach (var paramDef in orderParamDefs)
            {
                var key = ToCamelCase(paramDef.ModuleMethodParamName);
                if (!tempKeys.Contains(key))
                {
                    sb.AppendLine($"_parameters[\"{key}\"]=0.0f;");
                    tempKeys.Add(key);
                }
            }
            sb.AppendLine("        }");
            // Execute 方法
            sb.AppendLine("    public override object Execute(Dictionary<string, object> parameters, string methodName, IGlobalStatus gs)");
            sb.AppendLine("    {");
            sb.AppendLine("        switch (methodName)");
            sb.AppendLine("        {");
            foreach (var item in bqjxModuleDef.BqjxModuleClassDef.BqjxModuleMethodDefs)
            {
                sb.AppendLine($"            case nameof({item.ModuleMethodName}):");
                sb.AppendLine("{");
                if (item.BqjxModuleMethodParamDefs.Count > 0)
                {
                    foreach (var paramDef in item.BqjxModuleMethodParamDefs)
                    {
                        sb.AppendLine($"            var {ToCamelCase(paramDef.ModuleMethodParamName)} =({paramDef.ModuleMethodParamType})parameters[\"{ToCamelCase(paramDef.ModuleMethodParamName)}\"]; ");
                    }
                }
                if (item.BqjxModuleMethodParamDefs.Count > 0)
                {
                    sb.AppendLine($"                return {item.ModuleMethodName}({string.Join(",", item.BqjxModuleMethodParamDefs.Select(p => ToCamelCase(p.ModuleMethodParamName)))},gs);");
                }
                else
                {
                    sb.AppendLine($"                return {item.ModuleMethodName}(gs);");
                }
                sb.AppendLine("}");
            }
            sb.AppendLine("            default:");
            sb.AppendLine("                _logger?.Error($\"method is not exist name :{methodName}\");");
            sb.AppendLine("                return 0;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine();

            //  方法
            foreach (var item in bqjxModuleDef.BqjxModuleClassDef.BqjxModuleMethodDefs)
            {
                sb.AppendLine("/// <summary>");
                sb.AppendLine("/// " + item.ModuleMethodDescription + string.Join(",", item.BqjxModuleMethodParamDefs.Select(p => $"[{p.ModuleMethodParamName} {p.ModuleMethodParamType}]")));
                sb.AppendLine("/// </summary>");
                foreach (var methodParamDef in item.BqjxModuleMethodParamDefs)
                {
                    sb.AppendLine($"        /// <param name=\"{methodParamDef.ModuleMethodParamName}\">{methodParamDef.ModuleMethodParamDescription}</param>");
                }
                sb.AppendLine("        /// <param name=\"gs\"></param>");
                sb.AppendLine("        /// <returns></returns>");
                sb.AppendLine("        [ModuleAction]");
                if (item.BqjxModuleMethodParamDefs.Count > 0)
                {
                    sb.AppendLine($"    public int {item.ModuleMethodName}({string.Join(",", item.BqjxModuleMethodParamDefs.Select(p => $"{p.ModuleMethodParamType} {ToCamelCase(p.ModuleMethodParamName)}"))},IGlobalStatus gs)");
                }
                else
                    sb.AppendLine($"    public int {item.ModuleMethodName}(IGlobalStatus gs)");
                sb.AppendLine("    {");
                sb.AppendLine("#if VIRTUAL");
                sb.AppendLine($"        _logger?.Info($\"{item.ModuleMethodName} \");");
                sb.AppendLine("        Thread.Sleep(5000);");
                sb.AppendLine("        return Task.FromResult(true);");
                sb.AppendLine("#endif");
                sb.AppendLine("           try");
                sb.AppendLine("           {");

                sb.AppendLine($"               var cmd = {item.FuncCode.ToString()};");
                sb.AppendLine("                var controlInfo = _moduleControlInfo.CommandInfoDic.FirstOrDefault(c => c.Key == cmd).Value; ");
                if (item.BqjxModuleMethodParamDefs.Count > 0)
                {
                    if (item.IsIndependent)
                    {
                        if (item.BqjxModuleMethodParamDefs.Count == 1)
                        {
                            var paramDef = item.BqjxModuleMethodParamDefs[0];
                            if (paramDef.ModuleMethodParamType == "float[]")
                            {
                                sb.AppendLine($"        var result = ExecuteCommand<float>({ToCamelCase(paramDef.ModuleMethodParamName)},cmd,controlInfo,gs);");
                            }
                            else
                            {
                                sb.AppendLine($"        var result = ExecuteCommand<float>(new float[]{{{ToCamelCase(paramDef.ModuleMethodParamName)}}},cmd,controlInfo,gs);");
                            }
                        }
                        else
                        {

                            sb.AppendLine($"        var result = ExecuteCommand<float>(new float[]{{{string.Join(",", item.BqjxModuleMethodParamDefs.Select(p => ToCamelCase(p.ModuleMethodParamName)))}}},cmd,controlInfo,gs);");
                        }
                    }
                    else
                    {
                        if (item.BqjxModuleMethodParamDefs.Count == 1)
                        {
                            var paramDef = item.BqjxModuleMethodParamDefs[0];
                            if (paramDef.ModuleMethodParamType == "float[]")
                            {
                                sb.AppendLine($"        var result = ExecuteCommand<float>({ToCamelCase(paramDef.ModuleMethodParamName)},cmd,controlInfo,gs);");
                            }
                            else
                            {
                                foreach (var paramDefItem in item.BqjxModuleMethodParamDefs)
                                {
                                    sb.AppendLine($"_parameters[\"{ToCamelCase(paramDefItem.ModuleMethodParamName)}\"]={ToCamelCase(paramDefItem.ModuleMethodParamName)};");
                                }
                                sb.AppendLine($"        var result = ExecuteCommand<float>(_parameters.Values.ToArray(),cmd,controlInfo,gs);");
                            }
                        }
                        else
                        {
                            foreach (var paramDefItem in item.BqjxModuleMethodParamDefs)
                            {
                                sb.AppendLine($"_parameters[\"{ToCamelCase(paramDefItem.ModuleMethodParamName)}\"]={ToCamelCase(paramDefItem.ModuleMethodParamName)};");
                            }
                            sb.AppendLine($"        var result = ExecuteCommand<float>(_parameters.Values.ToArray(),cmd,controlInfo,gs);");
                        }
                    }
                    
                }
                else
                {
                    sb.AppendLine($"        var result = ExecuteCommand<float>(null,cmd,controlInfo, gs);");
                }
                sb.AppendLine("        if (result != 99999)");
                sb.AppendLine("        {");
                sb.AppendLine($"            _logger?.Warn($\"unable to {item.ModuleMethodName} code:{{result}}\");");
                sb.AppendLine("        }");
                sb.AppendLine("        return result;");
                sb.AppendLine("            }");
                sb.AppendLine("            catch (Exception ex)");
                sb.AppendLine("            {");
                sb.AppendLine("                _logger?.Error(ex.ToString());");
                sb.AppendLine("                return (int)CommandExecuteErrCode.InnerErr;");
                sb.AppendLine("            }");
                sb.AppendLine("    }");
            }
            sb.AppendLine();
            sb.AppendLine("  }");
            sb.AppendLine("}");
            return FormatCode(sb.ToString());
        }

        public static string GenerateModuleTestCode(BqjxModuleDef oWnerbqjxModuleDef, BqjxModuleDef bqjxModuleDef)
        {
            StringBuilder sb = new();
            /*
             using BQJX.Core.Common.Common;
             using BQJX.Core.Common.Interface;
             using BQJX.Core.Common.Logger;
             using BQJX.Modules.Mod.Tube2ml;
             using BQJX.Modules.Models;
             using System;
             using System.Collections.Generic;
             using System.Linq;
             using System.Text;
             using System.Threading.Tasks;
             */
            //命名空间
            sb.AppendLine("using BQJX.Core.Common.Common;");
            sb.AppendLine("using BQJX.Core.Common.Interface;");
            sb.AppendLine("using BQJX.Core.Common.Logger;");
            sb.AppendLine("using BQJX.Communication.Inovance;");
            sb.AppendLine("using BQJX.Communication.Modbus;");
            sb.AppendLine($"using {bqjxModuleDef.NamespaceName};");
            sb.AppendLine("using BQJX.Modules.Models;");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Text;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine();
            sb.AppendLine("namespace "+ "ModuleUnitTest." + bqjxModuleDef.NamespaceName.Split('.').Last());
            sb.AppendLine("{");
            // 类头部注释
            sb.AppendLine("/// <summary>");
            sb.AppendLine("/// " + bqjxModuleDef.BqjxModuleClassDef.ModuleClassDescription+" 测试类");
            sb.AppendLine("/// </summary>");
            //类定义
            sb.AppendLine($"    public class ModuleUnit_{bqjxModuleDef.BqjxModuleClassDef.ModuleClassName}");
            sb.AppendLine("    {");
            sb.AppendLine("        private  readonly ILogger? _logger;");
            sb.AppendLine("        private readonly IGlobalStatus _globalStatus;");
            sb.AppendLine("    private readonly AutoResetEvent _pauseEvent = new AutoResetEvent(false);");
            //构造函数
            sb.AppendLine($"        public ModuleUnit_{bqjxModuleDef.BqjxModuleClassDef.ModuleClassName}()");
            sb.AppendLine("        {");
            sb.AppendLine("            Console.WriteLine(\"Hello, World!\");");
            sb.AppendLine("            MyLoggerFactory.Configure(AppContext.BaseDirectory + \"/log4net.config\");");
            sb.AppendLine("            _logger = MyLoggerFactory.GetLogger(typeof(Program));");
            sb.AppendLine("            _globalStatus = new GlobalStatus();");
            sb.AppendLine("            _globalStatus.StartProgram();");
            sb.AppendLine("        }");

            // Pause()方法定义
            sb.AppendLine("   static void Pause()");
            sb.AppendLine("        {");
            sb.AppendLine("            Console.WriteLine(\"按任意键返回菜单...\");");
            sb.AppendLine("            Console.ReadKey();");
            sb.AppendLine("        }");

            sb.AppendLine("        public async Task StartTestModuleAsync()");
            sb.AppendLine("        {");

            //
            sb.AppendLine("var moduleEntity = new ModuleEntity");
            sb.AppendLine("{");
            sb.AppendLine("    IpAddress = \"192.168.10.1\",");
            sb.AppendLine("};");
            sb.AppendLine("while (true)");
            sb.AppendLine("{");
            sb.AppendLine("    Console.WriteLine($\"请确认模块IP地址：{moduleEntity.IpAddress}, 是否正确？(【Enter键】继续, 【Space键】重新设置)：\");");
            sb.AppendLine("    ConsoleKeyInfo keyInfo = Console.ReadKey(true); // true 参数使得读取按键时不显示在控制台上");
            sb.AppendLine("    if (keyInfo.Key == ConsoleKey.Enter)");
            sb.AppendLine("    {");
            sb.AppendLine("        break;");
            sb.AppendLine("    }");
            sb.AppendLine("    else if (keyInfo.Key == ConsoleKey.Spacebar)");
            sb.AppendLine("    {");
            sb.AppendLine("        Console.Clear();");
            sb.AppendLine("        Console.WriteLine(\"请输入模块IP地址：\");");
            sb.AppendLine("        string newIp = Console.ReadLine();");
            sb.AppendLine("        moduleEntity.IpAddress = newIp;");
            sb.AppendLine("        continue;");
            sb.AppendLine("    }");
            sb.AppendLine("    else");
            sb.AppendLine("    {");
            sb.AppendLine("        Console.WriteLine(\"无效的按键，请按Enter键继续或按Space键重新设置IP地址。\");");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            sb.AppendLine("moduleEntity.PLC = new H5U(new ModbusTcp(moduleEntity.IpAddress, 502, null));");

            var className_small = bqjxModuleDef.BqjxModuleClassDef.ModuleClassName.ToLower();
            sb.AppendLine($"            {bqjxModuleDef.BqjxModuleClassDef.ModuleClassName} {className_small} = new {bqjxModuleDef.BqjxModuleClassDef.ModuleClassName}(moduleEntity);");
            sb.AppendLine("  bool exitProgram = false;");
            sb.AppendLine("            while (!exitProgram)");
            sb.AppendLine("            {");
            sb.AppendLine("                try");
            sb.AppendLine("                {");
            sb.AppendLine("                    Console.Clear();");
            sb.AppendLine($"                    Console.WriteLine($\"======={bqjxModuleDef.BqjxModuleClassDef.ModuleClassDescription} 测试 =======\");");

            sb.AppendLine("      Console.WriteLine(\"1. 模块回原  \");");
            sb.AppendLine("      Console.WriteLine(\"2. 切换自动\");");
            sb.AppendLine("      Console.WriteLine(\"3. 切换手动\");");
            sb.AppendLine("      Console.WriteLine(\"4. 模块启动  \");");
            sb.AppendLine("      Console.WriteLine(\"5. 获取模块状态  \");");
            sb.AppendLine("      Console.WriteLine(\"6. 获取模块报警信息  \");");
            sb.AppendLine("      Console.WriteLine(\"7. 模块停止  \");");
            sb.AppendLine("      Console.WriteLine(\"8. 模块暂停  \");");
            sb.AppendLine("      Console.WriteLine(\"9. 模块继续  \");");
            sb.AppendLine("      Console.WriteLine(\"A. 模块报警复位  \");");
            sb.AppendLine("      Console.WriteLine(\"B. 模块执行  \");");
            sb.AppendLine("      Console.WriteLine(\"C. 退出程序\");");
            sb.AppendLine("      Console.WriteLine(\"D. 模块执行状态清除\");");
            sb.AppendLine("      Console.Write(\"请选择操作（输入数字后回车）：\");");
            sb.AppendLine("      string? choice = Console.ReadLine();");
            sb.AppendLine("                    switch (choice)");
            sb.AppendLine("                    {");
            sb.AppendLine("                        case \"1\":");
            sb.AppendLine($"                            var result = await {className_small}.HomeAsync(_globalStatus);");
            sb.AppendLine("   Console.WriteLine(\"回原操作结果：\" + result);");
            sb.AppendLine("    Pause(); break;");
            sb.AppendLine("                        case \"2\":");
            sb.AppendLine($"                            var result2 = {className_small}.ManualAuto(true);");
            sb.AppendLine("         Console.WriteLine(\"切换自动操作结果：\" + result2);");
            sb.AppendLine("    Pause(); break;");
            sb.AppendLine("                        case \"3\":");
            sb.AppendLine($"                            var result3 = {className_small}.ManualAuto(false);");
            sb.AppendLine("         Console.WriteLine(\"切换手动操作结果：\" + result3);");
            sb.AppendLine("    Pause(); break;");
            sb.AppendLine("                        case \"4\":");
            sb.AppendLine($"                            var result4 = {className_small}.Start();");
            sb.AppendLine("         Console.WriteLine(\"启动操作结果：\" + result4);");
            sb.AppendLine("    Pause(); break;");
            sb.AppendLine("                        case \"5\":");
            sb.AppendLine($"               var result5 = await {className_small}.GetMachineStatusAsync();");
            sb.AppendLine("               Console.WriteLine(\"获取模块状态结果：\" + result5);");
            sb.AppendLine("    Pause(); break;");
            sb.AppendLine("                        case \"6\":");
            sb.AppendLine($"             var result6 = await {className_small}.GetErrInfoAsync();");
            sb.AppendLine("                  Console.WriteLine(\"获取模块报警信息结果：\" + result6);");
            sb.AppendLine("    Pause(); break;");
            sb.AppendLine("                        case \"7\":");
            sb.AppendLine($"                var result7 = {className_small}.Stop();");
            sb.AppendLine("               Console.WriteLine(\"停止操作结果：\" + result7);");
            sb.AppendLine("    Pause(); break;");
            sb.AppendLine("                        case \"8\":");
            sb.AppendLine($"          var result8 = {className_small}.Pause();");
            sb.AppendLine("     Console.WriteLine(\"暂停操作结果：\" + result8);");
            sb.AppendLine("   Pause(); break;");
            sb.AppendLine("                        case \"9\":");
            sb.AppendLine($"         var result9 = {className_small}.Continue();");
            sb.AppendLine("     Console.WriteLine(\"继续操作结果：\" + result9);");
            sb.AppendLine("   Pause(); break;");
            sb.AppendLine("                        case \"A\":");
            sb.AppendLine($"        var resultA = {className_small}.Reset();");
            sb.AppendLine("     Console.WriteLine(\"复位操作结果：\" + resultA);");
            sb.AppendLine("   Pause(); break;");
            sb.AppendLine("                        case \"B\":");
            sb.AppendLine($"                MatchModuleEx({className_small}, _globalStatus);");
            sb.AppendLine("        _pauseEvent.WaitOne();");
            sb.AppendLine("   Pause(); break;");
            sb.AppendLine("                        case \"C\":");
            sb.AppendLine("                            exitProgram = true;");
            sb.AppendLine("break;");
            sb.AppendLine("                        case \"D\":");
            sb.AppendLine($"              var resultD = {className_small}.ClearControlAndRunState();");
            sb.AppendLine("              Console.WriteLine(\"清除控制状态和运行状态结果：\" + resultD);");
            sb.AppendLine("          Pause(); break;");
            sb.AppendLine("                        default:");
            sb.AppendLine("                            Console.WriteLine(\"无效选项，请重试！\");");
            sb.AppendLine("   Pause(); break;");
            sb.AppendLine("                    }");
            sb.AppendLine("                }");
            sb.AppendLine("                catch (Exception ex)");
            sb.AppendLine("                {");
            sb.AppendLine("                    Console.WriteLine(ex);");
            sb.AppendLine("                    Pause();");
            sb.AppendLine("                }");
            sb.AppendLine("            }");
            sb.AppendLine("        }");
            sb.AppendLine();

            foreach (var methodDef in oWnerbqjxModuleDef.BqjxModuleClassDef.BqjxModuleMethodDefs)
            {
                if (methodDef.BqjxModuleMethodParamDefs.Count > 0)
                {
                    if (methodDef.BqjxModuleMethodParamDefs.Count == 1)
                    {
                        if (methodDef.BqjxModuleMethodParamDefs.First().ModuleMethodParamType == "float[]")
                        {
                            sb.AppendLine("        /// <summary>");
                            sb.AppendLine($"        /// {methodDef.ModuleMethodDescription}的参数 参数类型为float数组，请注意填写");
                            sb.AppendLine("        /// </summary>");
                            sb.AppendLine($" public List<float> {methodDef.ModuleMethodName}Prameters {{ get; set; }} = [];");
                        }
                        else
                        {
                            sb.AppendLine("        /// <summary>");
                            sb.AppendLine($"        /// {methodDef.ModuleMethodDescription}的参数 集合长度{methodDef.BqjxModuleMethodParamDefs.Count} {string.Join(",", methodDef.BqjxModuleMethodParamDefs.Select(p => $"[{p.ModuleMethodParamName} {p.ModuleMethodParamType}]"))}");
                            sb.AppendLine("        /// </summary>");
                            sb.AppendLine($" public List<object> {methodDef.ModuleMethodName}Prameters {{ get; set; }} = [];");
                        }
                    }
                    else
                    {
                        sb.AppendLine("        /// <summary>");
                        sb.AppendLine($"        /// {methodDef.ModuleMethodDescription}的参数 集合长度{methodDef.BqjxModuleMethodParamDefs.Count} {string.Join(",", methodDef.BqjxModuleMethodParamDefs.Select(p => $"[{p.ModuleMethodParamName} {p.ModuleMethodParamType}]"))}");
                        sb.AppendLine("        /// </summary>");
                        sb.AppendLine($" public List<object> {methodDef.ModuleMethodName}Prameters {{ get; set; }} = [];");
                    }
                }
            }

            sb.AppendLine();
            sb.AppendLine($"        private void MatchModuleEx({bqjxModuleDef.BqjxModuleClassDef.ModuleClassName} {bqjxModuleDef.BqjxModuleClassDef.ModuleClassName.ToLower()}, IGlobalStatus globalStatus)");
            sb.AppendLine("        {");
            sb.AppendLine("            Task.Run(() => ");
            sb.AppendLine("            {");
            sb.AppendLine("");
            //遍历函数
            if (oWnerbqjxModuleDef.BqjxModuleClassDef.BqjxModuleMethodDefs.Count > 0)
            {
                sb.AppendLine("                    while (true)");
                sb.AppendLine("                    {");
                sb.AppendLine("  Console.Clear();");
                sb.AppendLine($"                        Console.WriteLine(\"{bqjxModuleDef.BqjxModuleClassDef.ModuleClassDescription}执行选项：\");");
                var methodDefs = oWnerbqjxModuleDef.BqjxModuleClassDef.BqjxModuleMethodDefs;
                for (int i = 0; i < oWnerbqjxModuleDef.BqjxModuleClassDef.BqjxModuleMethodDefs.Count; i++)
                {
                    var methodDef = methodDefs[i];
                    sb.AppendLine($"                Console.WriteLine(\"{i + 1}. 执行{methodDef.ModuleMethodDescription}操作\");");
                }
                sb.AppendLine($"                     Console.WriteLine(\"{methodDefs.Count + 1}. 返回主菜单\");");

                sb.AppendLine("                   var option = Console.ReadLine();");
                sb.AppendLine("              switch (option)");
                sb.AppendLine("                        {");

                for (int i = 0; i < oWnerbqjxModuleDef.BqjxModuleClassDef.BqjxModuleMethodDefs.Count; i++)
                {
                    var methodDef = methodDefs[i];
                    sb.AppendLine($"                            case \"{i + 1}\":");
                    sb.AppendLine("{");
                    sb.AppendLine("       _pauseEvent.Set();");
                    sb.AppendLine("             var dict=new Dictionary<string, object>();");
                    if (methodDef.BqjxModuleMethodParamDefs.Count > 0)
                    {
                        if (methodDef.BqjxModuleMethodParamDefs.Count == 1)
                        {
                            if (methodDef.BqjxModuleMethodParamDefs.First().ModuleMethodParamType == "float[]")
                            {
                                var paramDef = methodDef.BqjxModuleMethodParamDefs[0];
                                sb.AppendLine($"dict[\"{paramDef.ModuleMethodParamName}\"]={methodDef.ModuleMethodName}Prameters;");
                            }
                            else
                            {
                                for (int j = 0; j < methodDef.BqjxModuleMethodParamDefs.Count; j++)
                                {
                                    var paramDef = methodDef.BqjxModuleMethodParamDefs[j];
                                    sb.AppendLine($"dict[\"{paramDef.ModuleMethodParamName}\"]={methodDef.ModuleMethodName}Prameters[{j}];");
                                }
                            }
                        }
                        else
                        {
                            for (int j = 0; j < methodDef.BqjxModuleMethodParamDefs.Count; j++)
                            {
                                var paramDef = methodDef.BqjxModuleMethodParamDefs[j];
                                sb.AppendLine($"dict[\"{paramDef.ModuleMethodParamName}\"]={methodDef.ModuleMethodName}Prameters[{j}];");
                            }
                        }
                    }
                    sb.AppendLine($" var result = {className_small}.Execute(dict,nameof({bqjxModuleDef.BqjxModuleClassDef.ModuleClassName}.{methodDef.ModuleMethodName}),globalStatus);");
                    sb.AppendLine($"                 Console.WriteLine(\"执行{methodDef.ModuleMethodDescription}操作结果：\" + result);");
                    sb.AppendLine("    return;");
                    sb.AppendLine("}");
                }
                sb.AppendLine("                        default:");
                sb.AppendLine("                            Console.WriteLine(\"无效选项，请重试！\");");
                sb.AppendLine("   Pause(); break;");
                sb.AppendLine("}");
                sb.AppendLine("}");
            }
            else
            {

            }
            sb.AppendLine("            });");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            sb.AppendLine("}");
            return FormatCode(sb.ToString());
        }
        /// <summary>
        /// 生成模块代码
        /// </summary>
        /// <param name="bqjxModuleDef"></param>
        /// <returns></returns>
        public static string GenerateModuleCode(BqjxModuleDef oWnerbqjxModuleDef,BqjxModuleDef  bqjxModuleDef)
        {
            StringBuilder sb = new();

            //命名空间
            sb.AppendLine("using BQJX.Core.Common.Common.JsonAccess;");
            sb.AppendLine("using BQJX.Core.Common.Interface;");
            sb.AppendLine("using BQJX.Modules.Common;");
            sb.AppendLine("using BQJX.Core.Common.Logger;");
            sb.AppendLine("using BQJX.Modules.Interface;");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using System.Text;");
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine("using BQJX.Modules.Mod.Base;");
            sb.AppendLine("using BQJX.Modules.Models;");
            sb.AppendLine("using BQJX.Core.Common.Attributes;");
            sb.AppendLine("using BQJX.Modules.AutoMod.Base;");
            sb.AppendLine();
            sb.AppendLine("namespace " + bqjxModuleDef.NamespaceName);
            sb.AppendLine("{");
            // 类头部注释
            sb.AppendLine("/// <summary>");
            sb.AppendLine("/// " + bqjxModuleDef.BqjxModuleClassDef.ModuleClassDescription);
            sb.AppendLine("/// </summary>");
            sb.AppendLine("    [Module]");
            // 类声明
            sb.AppendLine($"public class {bqjxModuleDef.BqjxModuleClassDef.ModuleClassName} : {bqjxModuleDef.BqjxModuleClassDef.ModuleBaseClassName}");

            sb.AppendLine("{");

            // 构造函数
            sb.AppendLine($"    public {bqjxModuleDef.BqjxModuleClassDef.ModuleClassName}(ModuleEntity moduleEntity) : base()");
            sb.AppendLine("    {");
            sb.AppendLine("        this.ModuleName = moduleEntity.ModuleName;");
            sb.AppendLine("        this.PlatFormName = moduleEntity.PlatFormName;");
            sb.AppendLine("        _plc = moduleEntity.PLC;");
            sb.AppendLine("        _autoCtlInfo = new AutoControlInfo();");
            sb.AppendLine("  _moduleControlInfo = new ModuleControlInfo();");
            sb.AppendLine("            _moduleControlInfo.ModuleGlobalName = moduleEntity.ModuleName;");
            sb.AppendLine("         _moduleControlInfo.CommandInfoDic = new Dictionary<int, ExecuteCommandInfo>();");
            foreach (var item in oWnerbqjxModuleDef.BqjxModuleClassDef.BqjxModuleMethodDefs)
            {
                sb.AppendLine($" _moduleControlInfo.CommandInfoDic[{item.FuncCode}] = new ExecuteCommandInfo()");
                sb.AppendLine(" {");
                foreach (var propertyInfo in typeof(ExecuteCommandInfo).GetProperties())
                {
                    sb.AppendLine($" {propertyInfo.Name} = \"{propertyInfo.GetValue(item.ExecuteCommandInfo)}\",");
                }
                sb.AppendLine(" };");
            }
            sb.AppendLine($"        _logger = MyLoggerFactory.GetLogger(typeof({bqjxModuleDef.BqjxModuleClassDef.ModuleClassName}));");
            sb.AppendLine("    }");
            sb.AppendLine();

            sb.AppendLine("  }");
            sb.AppendLine("}");
            return FormatCode(sb.ToString());
        }

        static string ToCamelCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // 先转换成 PascalCase
            string pascalCase = Regex.Replace(input, @"(?:^|_)([a-z])", match => match.Groups[1].Value.ToUpper());

            // 再把首字母变小写，变成小驼峰
            return char.ToLower(pascalCase[0], CultureInfo.InvariantCulture) + pascalCase.Substring(1);
        }

        static string FormatCode(string code)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
            SyntaxNode root = syntaxTree.GetRoot();
            using var workspace = new AdhocWorkspace();

            OptionSet options = workspace.Options
                .WithChangedOption(CSharpFormattingOptions.NewLinesForBracesInMethods, true)
                .WithChangedOption(CSharpFormattingOptions.IndentBraces, false);

            SyntaxNode formattedNode = Microsoft.CodeAnalysis.Formatting.Formatter.Format(root, workspace, options);
            return formattedNode.ToFullString();
        }
    }
}
