using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using TangerineAutomationSystem.Models;

namespace TangerineAutomationSystem.Services
{
    public interface IProjectService
    {
        Project CreateNewProject();
        Project OpenProject();
        void SaveProject(Project project);
        void SaveAsProject(Project project);
        Project LoadRecentProject();
        void CloseProject(Project project);
    }

    public class ProjectService : IProjectService
    {
        private readonly string _projectsDirectory;

        public ProjectService()
        {
            _projectsDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "TangerineAutomationSystem",
                "Projects");
            Directory.CreateDirectory(_projectsDirectory);
        }

        public Project CreateNewProject()
        {
            return new Project
            {
                Name = "新项目",
                CreatedTime = DateTime.Now,
                ModifiedTime = DateTime.Now
            };
        }

        public Project OpenProject()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Tangerine 项目文件 (*.tproj)|*.tproj",
                InitialDirectory = _projectsDirectory
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var json = File.ReadAllText(dialog.FileName);
                    var project = JsonSerializer.Deserialize<Project>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        WriteIndented = true
                    });

                    project.ProjectPath = dialog.FileName;
                    return project;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"打开项目失败: {ex.Message}", "错误",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            return null;
        }

        public void SaveProject(Project project)
        {
            if (string.IsNullOrEmpty(project.ProjectPath))
            {
                SaveAsProject(project);
                return;
            }

            try
            {
                project.ModifiedTime = DateTime.Now;
                var json = JsonSerializer.Serialize(project, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(project.ProjectPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存项目失败: {ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SaveAsProject(Project project)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Tangerine 项目文件 (*.tproj)|*.tproj",
                InitialDirectory = _projectsDirectory,
                FileName = $"{project.Name}.tproj"
            };

            if (dialog.ShowDialog() == true)
            {
                project.ProjectPath = dialog.FileName;
                SaveProject(project);
            }
        }

        public Project LoadRecentProject()
        {
            // 实现加载最近项目的逻辑
            return null;
        }

        public void CloseProject(Project project)
        {
            // 实现关闭项目的逻辑
        }
    }
}
