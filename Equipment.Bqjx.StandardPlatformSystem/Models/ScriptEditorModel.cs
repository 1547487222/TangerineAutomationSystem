using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Equipment.Bqjx.StandardPlatformSystem.Models
{
    public partial class ScriptEditorModel:ObservableObject
    {
        [RelayCommand]
        public void CompileScript(string script)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(script);

            // 获取编译选项
            CSharpCompilationOptions options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

            // 添加引用（必须包含 mscorlib 和 System.Runtime）
            var references = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .Select(a => MetadataReference.CreateFromFile(a.Location))
                .ToList();

            // 编译代码
            CSharpCompilation compilation = CSharpCompilation.Create(
                "DynamicScript",
                new[] { syntaxTree },
                references,
                options
            );
            // 指定输出 DLL 路径
            string outputPath = Path.Combine(Directory.GetCurrentDirectory(), "ScriptDlls", "DynamicScript.dll");
            if (!Directory.Exists(Path.GetDirectoryName(outputPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            }
            // 生成 DLL 文件
            using (var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
            {
                var result = compilation.Emit(fs);
                if (result.Success)
                {
                    MessageBox.Show($"编译成功！DLL 生成于: {outputPath}");
                }
                else
                {
                    foreach (var diag in result.Diagnostics)
                    {
                        MessageBox.Show($"编译错误: {diag}");
                    }
                }
            }
        }

    }
}
