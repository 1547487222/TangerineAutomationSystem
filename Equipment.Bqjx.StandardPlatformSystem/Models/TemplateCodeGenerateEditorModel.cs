using CodeGenerationTask.BqjxModuleDefs;
using CodeGenerationTask.CodeGenerator;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Equipment.Bqjx.StandardPlatformSystem.Views;
using Microsoft.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Equipment.Bqjx.StandardPlatformSystem.Models
{
    public partial class TemplateCodeGenerateEditorModel : ObservableObject
    {
        [ObservableProperty]
        private string _bqjxModuleMakeName;
        public TemplateCodeGenerateEditorModel(BqjxModuleMake bqjxModuleMake)
        {
            this.BqjxModuleMake = bqjxModuleMake;
            _bqjxModuleMakeName = bqjxModuleMake.BqjxModuleBaseDef.BqjxModuleClassDef.ModuleClassDescription??"未定义类型名称";
            Content = new GenerateEditorContentView();
        }

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public BqjxModuleMake BqjxModuleMake { get; set; }

        public object Content { get; set; }

        [RelayCommand]
        private void AddModuleImpl()
        {
            BqjxModuleMake.BqjxModuleImplDef.Add(new BqjxModuleDef()
            {
                ImportPath = "E:\\Code\\Module.Src.Projects\\Projects.bqjx.modules\\BQJX.Modules\\BQJX.Modules\\AutoMod\\Tube2ml\\",
                NamespaceName = "BQJX.Modules.AutoMod.Tube2ml",
                ModuleTestImportRootPath = "E:\\Code\\Module.Src.Projects\\Projects.bqjx.modules\\BQJX.Modules\\ModuleUnitTest\\Tube2ml\\",
                BqjxModuleClassDef = new BqjxModuleClassDef
                {
                    ModuleBaseClassName = BqjxModuleMake.BqjxModuleBaseDef.BqjxModuleClassDef.ModuleClassName,
                }
            });
        }

        [RelayCommand]
        private void ExportScript()
        {
            try
            {

                //TODO
                if (BqjxModuleMake.BqjxModuleBaseDef.BqjxModuleClassDef.ModuleClassName != string.Empty)
                {
                    var baseScript = ModuleCodeGenerator.GenerateModuleBaseCode(BqjxModuleMake.BqjxModuleBaseDef);
                    if (!Directory.Exists(Path.GetDirectoryName(BqjxModuleMake.BqjxModuleBaseDef.ImportPath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(BqjxModuleMake.BqjxModuleBaseDef.ImportPath));
                    }
                    if (!File.Exists(BqjxModuleMake.BqjxModuleBaseDef.ImportPath))
                    {
                        File.Create(BqjxModuleMake.BqjxModuleBaseDef.ImportPath).Close();
                    }
                    File.WriteAllText(BqjxModuleMake.BqjxModuleBaseDef.ImportPath, baseScript);
                }

                foreach (var bqjxModuleDef in BqjxModuleMake.BqjxModuleImplDef)
                {
                    if (bqjxModuleDef.BqjxModuleClassDef.ModuleClassName != string.Empty)
                    {
                        var implScript = ModuleCodeGenerator.GenerateModuleCode(BqjxModuleMake.BqjxModuleBaseDef, bqjxModuleDef);
                        if (!Directory.Exists(Path.GetDirectoryName(bqjxModuleDef.ImportPath)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(bqjxModuleDef.ImportPath));
                        }
                        if (!File.Exists(bqjxModuleDef.ImportPath))
                        {
                            File.Create(bqjxModuleDef.ImportPath).Close();
                        }
                        File.WriteAllText(bqjxModuleDef.ImportPath, implScript);
                        var testScript = ModuleCodeGenerator.GenerateModuleTestCode(BqjxModuleMake.BqjxModuleBaseDef, bqjxModuleDef);
                        if (!Directory.Exists(Path.GetDirectoryName(bqjxModuleDef.ModuleTestImportRootPath)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(bqjxModuleDef.ModuleTestImportRootPath));
                        }
                        var testScriptPath = Path.Combine(bqjxModuleDef.ModuleTestImportRootPath, "ModuleUnit_" + bqjxModuleDef.BqjxModuleClassDef.ModuleClassName+".cs");
                        if (!File.Exists(testScriptPath))
                        {
                            File.Create(testScriptPath).Close();
                        }
                        File.WriteAllText(testScriptPath, testScript);
                    }
                }
                MessageBox.Show("导出成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show("导出失败："+ex.ToString());
            }
        }
    }
}
