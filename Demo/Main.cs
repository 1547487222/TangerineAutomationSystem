using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Demo
{
    using System;
    using System.IO;
    using System.Text;
    using System.Windows.Forms;

    namespace ConsoleApp
    {
        class Program
        {
            // 配置：要查找和替换的文本
            private const string SearchText = "QStandaedPlatform.Engine.Laboratory.";
            private const string ReplaceText = "QStandaedPlatform.Engine.Laboratory.Documents.";

            [STAThread]
            static void Main(string[] args)
            {
                // 注册代码页编码提供程序以支持 GBK 等编码
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                // 设置控制台输出编码为 UTF-8
                Console.OutputEncoding = Encoding.UTF8;

                // 创建 OpenFileDialog
                using (var dialog = new OpenFileDialog())
                {
                    dialog.Title = "请选择要处理的文件";
                    dialog.Filter = "文本文件|*.txt;*.cs;*.json;*.xml;*.html;*.css;*.js;*.flow|所有文件|*.*";
                    dialog.Multiselect = true; // 允许多选

                    Console.WriteLine("正在打开文件选择窗口...");
                    DialogResult result = dialog.ShowDialog();

                    if (result != DialogResult.OK)
                    {
                        Console.WriteLine("未选择任何文件。");
                        return;
                    }

                    Console.WriteLine($"已选择 {dialog.FileNames.Length} 个文件，开始处理...\n");

                    int successCount = 0;
                    int failCount = 0;

                    foreach (string filePath in dialog.FileNames)
                    {
                        try
                        {
                            if (!File.Exists(filePath))
                            {
                                Console.WriteLine($"❌ 文件不存在：{filePath}");
                                failCount++;
                                continue;
                            }

                            // 读取文件内容并检测编码
                            (string content, Encoding encoding) = ReadFileWithEncoding(filePath);

                            // 执行替换
                            string newContent = content.Replace(SearchText, ReplaceText);

                            // 写回文件，使用原始编码
                            File.WriteAllText(filePath, newContent, encoding);

                            Console.WriteLine($"✅ 已处理：{filePath}");
                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ 处理失败 {filePath}：{ex.Message}");
                            failCount++;
                        }
                    }

                    Console.WriteLine($"\n✅ 成功：{successCount}，❌ 失败：{failCount}");
                }

                Console.WriteLine("按任意键退出...");
                Console.ReadKey();
            }

            // 读取文件内容并检测编码
            static (string content, Encoding encoding) ReadFileWithEncoding(string filePath)
            {
                // 尝试使用 StreamReader 检测编码
                using (var reader = new StreamReader(filePath, Encoding.UTF8, true))
                {
                    // 读取全部内容
                    string content = reader.ReadToEnd();
                    // 获取实际检测到的编码
                    Encoding encoding = reader.CurrentEncoding;
                    return (content, encoding);
                }
            }
        }
    }
}
/// <summary>
/// 数据读取请求类型
/// </summary>
public enum ReadKind
{
    DataPoint,  // 普通数据点读取
    Command,    // 指令执行
    Config      // 配置读取
}

/// <summary>
/// 数据读取请求
/// </summary>
public class ReadCommand
{
    /// <summary>
    /// 寄存器地址、变量名、路径
    /// </summary>
    public string Target { get; init; } = string.Empty;

    /// <summary>
    /// 请求类型
    /// </summary>
    public ReadKind Kind { get; init; }

    /// <summary>
    /// 请求的参数
    /// </summary>
    public Dictionary<string, object> Parameters { get; init; } = [];

    /// <summary>
    /// 期望的数据类型
    /// </summary>
    public Type? ExpectedType { get; init; }
}
/// <summary>
/// 数据读取器接口
/// </summary>
public interface IDataReader
{
    string ReadString(ReadCommand readCommand);
    int ReadInt(ReadCommand readCommand);
    bool ReadBool(ReadCommand readCommand);
    double ReadDouble(ReadCommand readCommand);
}
