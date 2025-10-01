using CodeGenerationTask.BqjxModuleDefs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Equipment.Bqjx.StandardPlatformSystem.Models
{
    public partial class TemplateCodeGenerationEditorModel:ObservableObject
    {
        [ObservableProperty]
        private TemplateCodeGenerateEditorModel templateCodeGenerateEditorModel;

        private readonly string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TemplateCodeGenereates.json");

        public ObservableCollection<TemplateCodeGenerateEditorModel> GenerateEditorModels { get; set; } = [];

        public TemplateCodeGenerationEditorModel()
        {
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var datas = JsonSerializer.Deserialize<List<BqjxModuleMake>>(json);
                foreach (var item in datas)
                {
                    GenerateEditorModels.Add(new TemplateCodeGenerateEditorModel(item));
                }
            }
        }
        [RelayCommand]
        private void AddGenerateEditor()
        {
            GenerateEditorModels.Add(new TemplateCodeGenerateEditorModel(new BqjxModuleMake()));
        }
        [RelayCommand]
        public void SaveGenerateEditor()
        {
            try
            {
                var datas = new List<BqjxModuleMake>();
                foreach (var item in GenerateEditorModels)
                {
                    datas.Add(item.BqjxModuleMake);
                }
                var json = JsonSerializer.Serialize(datas);
                if (!File.Exists(path))
                {
                    File.Create(path).Close();
                }
                File.WriteAllText(path, json);
                MessageBox.Show("保存成功");
            }
            catch (Exception e)
            {
                MessageBox.Show("保存失败："+e.ToString());
            }
        }
    }
}
