using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Equipment.Bqjx.StandardPlatformSystem.Common
{
    public class CSVHelper
    {
        /// <summary>
        /// 将 List 导出为 CSV 文件
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="data">数据列表</param>
        /// <param name="fileName">默认文件名（不带扩展名）</param>
        /// <returns>是否导出成功</returns>
        public static bool ExportToCsv<T>(List<T> data, string fileName = "exported_data")
        {
            if (data == null || data.Count == 0)
            {
                MessageBox.Show("没有数据可导出！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // 创建保存文件对话框
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "选择 CSV 文件保存位置";
                saveFileDialog.Filter = "CSV 文件 (*.csv)|*.csv";
                saveFileDialog.FileName = $"{fileName}_{DateTime.Now:yyyyMMddHHmmss}.csv";
                saveFileDialog.DefaultExt = ".csv";
                saveFileDialog.AddExtension = true;

                // 显示对话框并等待用户选择
                if (saveFileDialog.ShowDialog() != DialogResult.OK)
                {
                    return false; // 用户取消了操作
                }

                string filePath = saveFileDialog.FileName;

                try
                {
                    // 生成 CSV 内容
                    StringBuilder csvContent = new StringBuilder();

                    // 获取属性名作为表头
                    var properties = typeof(T).GetProperties();
                    string header = string.Join(",", properties.Select(p => p.Name));
                    csvContent.AppendLine(header);

                    // 添加数据行
                    foreach (var item in data)
                    {
                        string row = string.Join(",", properties.Select(p =>
                        {
                            var value = p.GetValue(item, null);
                            return value != null ? EscapeCsvValue(value.ToString()) : string.Empty;
                        }));
                        csvContent.AppendLine(row);
                    }

                    // 写入文件
                    File.WriteAllText(filePath, csvContent.ToString(), Encoding.UTF8);

                    MessageBox.Show($"数据已成功导出到：\n{filePath}", "导出成功",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导出失败：\n{ex.Message}", "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
        }

        /// <summary>
        /// 转义 CSV 值（处理包含逗号、换行符等情况）
        /// </summary>
        private static string EscapeCsvValue(string value)
        {
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }
            return value;
        }
    }
}
