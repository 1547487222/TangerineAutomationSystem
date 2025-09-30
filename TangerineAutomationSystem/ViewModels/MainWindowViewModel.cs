using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using TangerineAutomationSystem.Models;
using TangerineAutomationSystem.Services;

namespace TangerineAutomationSystem.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<TreeNodeViewModel> Projects { get; set; } = new();

        private TreeNodeViewModel? _selectedNode;
        public TreeNodeViewModel? SelectedNode
        {
            get => _selectedNode;
            set { _selectedNode = value; OnPropertyChanged(nameof(SelectedNode)); }
        }

        public MainWindowViewModel()
        {
            // ensure TangerineHost created
            _ = TangerineHost.Instance;
            AddProjectInternal("默认项目-实验室1");
        }

        private void AddProjectInternal(string name)
        {
            var lab = new LaboratoryModel { Name = name };
            var labNode = new TreeNodeViewModel { DisplayName = lab.Name, NodeType = "Lab", Model = lab };

            var line = new ProductionLineModel { Name = "产线-A" };
            var lineNode = new TreeNodeViewModel { DisplayName = line.Name, NodeType = "Line", Model = line, Parent = labNode };

            var plat = new PlatformModel { Name = "平台-1" };
            var platNode = new TreeNodeViewModel { DisplayName = plat.Name, NodeType = "Platform", Model = plat, Parent = lineNode };

            var mod = new ModuleModel { Name = "模块X", ModuleType = "Default" };
            var modNode = new TreeNodeViewModel { DisplayName = mod.Name, NodeType = "Module", Model = mod, Parent = platNode };

            plat.Modules.Add(mod);
            platNode.Children.Add(modNode);
            line.Platforms.Add(plat);
            lineNode.Children.Add(platNode);
            lab.ProductionLines.Add(line);
            labNode.Children.Add(lineNode);

            Projects.Add(labNode);
        }

        // helper: use parameter first (TreeNodeViewModel), otherwise SelectedNode
        private TreeNodeViewModel? ResolveTargetNode(object? parameter)
        {
            if (parameter is TreeNodeViewModel pNode) return pNode;
            if (parameter is null && SelectedNode != null) return SelectedNode;
            // if parameter is underlying model (e.g., PlatformModel), try find corresponding TreeNode in Projects
            if (parameter != null)
            {
                // simple match: search by reference equality or by Name property
                foreach (var proj in Projects)
                {
                    var found = FindNodeByModel(proj, parameter);
                    if (found != null) return found;
                }
            }
            return null;
        }

        private TreeNodeViewModel? FindNodeByModel(TreeNodeViewModel node, object model)
        {
            if (ReferenceEquals(node.Model, model)) return node;
            // fall back to name match for common models
            var nodeName = node.DisplayName;
            var modelName = model.GetType().GetProperty("Name")?.GetValue(model) as string;
            if (!string.IsNullOrEmpty(nodeName) && !string.IsNullOrEmpty(modelName) && nodeName == modelName) return node;

            foreach (var child in node.Children)
            {
                var f = FindNodeByModel(child, model);
                if (f != null) return f;
            }
            return null;
        }

        // Commands (each accepts optional parameter)
        public RelayCommand AddProjectCommand => new(obj => AddProjectInternal($"项目_{DateTime.Now:MMddHHmmss}"));

        public RelayCommand AddNodeCommand => new(obj =>
        {
            var target = ResolveTargetNode(obj);
            if (target == null) { MessageBox.Show("请选择节点后再添加。"); return; }
            switch (target.NodeType)
            {
                case "Lab": AddLineCommand.Execute(target); break;
                case "Line": AddPlatformCommand.Execute(target); break;
                case "Platform": AddModuleCommand.Execute(target); break;
                default: MessageBox.Show("当前节点不支持添加子项。"); break;
            }
        });

        public RelayCommand AddLineCommand => new(obj =>
        {
            var target = ResolveTargetNode(obj);
            if (target == null || target.NodeType != "Lab") { MessageBox.Show("请选择实验室节点来添加产线。"); return; }
            var newLine = new ProductionLineModel { Name = "新产线" };
            var ln = new TreeNodeViewModel { DisplayName = newLine.Name, NodeType = "Line", Model = newLine, Parent = target };
            target.Children.Add(ln);
            if (target.Model is LaboratoryModel lm) lm.ProductionLines.Add(newLine);
        });

        public RelayCommand AddPlatformCommand => new(obj =>
        {
            var target = ResolveTargetNode(obj);
            if (target == null || target.NodeType != "Line") { MessageBox.Show("请选择产线节点来添加平台。"); return; }
            var newPlat = new PlatformModel { Name = "新平台" };
            var pn = new TreeNodeViewModel { DisplayName = newPlat.Name, NodeType = "Platform", Model = newPlat, Parent = target };
            target.Children.Add(pn);
            if (target.Model is ProductionLineModel plm) plm.Platforms.Add(newPlat);
        });

        public RelayCommand AddModuleCommand => new(obj =>
        {
            var target = ResolveTargetNode(obj);
            if (target == null || target.NodeType != "Platform") { MessageBox.Show("请选择平台节点来添加模块。"); return; }
            var newMod = new ModuleModel { Name = "新模块", ModuleType = "Default" };
            var mn = new TreeNodeViewModel { DisplayName = newMod.Name, NodeType = "Module", Model = newMod, Parent = target };
            target.Children.Add(mn);
            if (target.Model is PlatformModel pm) pm.Modules.Add(newMod);
        });

        public RelayCommand AddTrayResourceCommand => new(obj =>
        {
            var target = ResolveTargetNode(obj);
            if (target == null || target.NodeType != "Platform") { MessageBox.Show("请选择平台节点来添加托盘资源。"); return; }
            var res = new PlatformResourceModel { Name = "新托盘", ResourceType = "Tray" };
            var rn = new TreeNodeViewModel { DisplayName = res.Name, NodeType = "PlatformResource", Model = res, Parent = target };
            target.Children.Add(rn);
            if (target.Model is PlatformModel pm) pm.PlatformResources.Add(res);
        });

        public RelayCommand DeleteNodeCommand => new(obj =>
        {
            var target = ResolveTargetNode(obj);
            if (target == null || target.Parent == null) { MessageBox.Show("请选择非根节点进行删除。"); return; }
            var parent = target.Parent;
            switch (target.NodeType)
            {
                case "Line":
                    if (parent.Model is LaboratoryModel lab && target.Model is ProductionLineModel plm) lab.ProductionLines.Remove(plm);
                    break;
                case "Platform":
                    if (parent.Model is ProductionLineModel pl && target.Model is PlatformModel pm) pl.Platforms.Remove(pm);
                    break;
                case "Module":
                    if (parent.Model is PlatformModel p && target.Model is ModuleModel mm) p.Modules.Remove(mm);
                    break;
                case "LabResource":
                    if (parent.Model is LaboratoryModel lab2 && target.Model is LabResourceModel lr) lab2.LabResources.Remove(lr);
                    break;
                case "PlatformResource":
                    if (parent.Model is PlatformModel p2 && target.Model is PlatformResourceModel pr) p2.PlatformResources.Remove(pr);
                    break;
            }
            parent.Children.Remove(target);
            SelectedNode = null;
        });

        public RelayCommand RenameNodeCommand => new(obj =>
        {
            var target = ResolveTargetNode(obj);
            if (target == null) return;
            var input = Microsoft.VisualBasic.Interaction.InputBox("请输入名称：", "重命名", target.DisplayName);
            if (string.IsNullOrWhiteSpace(input)) return;
            target.DisplayName = input;
            switch (target.NodeType)
            {
                case "Lab":
                    if (target.Model is LaboratoryModel lab) lab.Name = input;
                    break;
                case "Line":
                    if (target.Model is ProductionLineModel pl) pl.Name = input;
                    break;
                case "Platform":
                    if (target.Model is PlatformModel pf) pf.Name = input;
                    break;
                case "Module":
                    if (target.Model is ModuleModel mm) mm.Name = input;
                    break;
            }
        });

        public RelayCommand SaveProjectCommand => new(obj =>
        {
            var root = ResolveTargetNode(obj);
            if (root == null) { MessageBox.Show("请选择要保存的项目根节点（实验室）。"); return; }
            while (root.Parent != null) root = root.Parent;
            if (root.Model is LaboratoryModel lab)
            {
                var dlg = new SaveFileDialog { Filter = "JSON 文件 (*.json)|*.json", FileName = lab.Name + ".json" };
                if (dlg.ShowDialog() == true)
                {
                    ProjectPersistenceService.SaveToFile(lab, dlg.FileName);
                    MessageBox.Show("保存成功：" + dlg.FileName);
                }
            }
            else MessageBox.Show("请选择实验室根节点。");
        });

        public RelayCommand OpenProjectCommand => new(obj =>
        {
            var dlg = new OpenFileDialog { Filter = "JSON 文件 (*.json)|*.json" };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var model = ProjectPersistenceService.LoadFromFile(dlg.FileName);
                    if (model != null)
                    {
                        var labNode = new TreeNodeViewModel { DisplayName = model.Name, NodeType = "Lab", Model = model };
                        foreach (var lr in model.LabResources)
                        {
                            var rnode = new TreeNodeViewModel { DisplayName = lr.Name, NodeType = "LabResource", Model = lr, Parent = labNode };
                            labNode.Children.Add(rnode);
                        }
                        foreach (var line in model.ProductionLines)
                        {
                            var ln = new TreeNodeViewModel { DisplayName = line.Name, NodeType = "Line", Model = line, Parent = labNode };
                            foreach (var plat in line.Platforms)
                            {
                                var pn = new TreeNodeViewModel { DisplayName = plat.Name, NodeType = "Platform", Model = plat, Parent = ln };
                                foreach (var pr in plat.PlatformResources)
                                {
                                    var prn = new TreeNodeViewModel { DisplayName = pr.Name, NodeType = "PlatformResource", Model = pr, Parent = pn };
                                    pn.Children.Add(prn);
                                }
                                foreach (var mod in plat.Modules)
                                {
                                    var mn = new TreeNodeViewModel { DisplayName = mod.Name, NodeType = "Module", Model = mod, Parent = pn };
                                    pn.Children.Add(mn);
                                }
                                ln.Children.Add(pn);
                            }
                            labNode.Children.Add(ln);
                        }
                        Projects.Add(labNode);
                        MessageBox.Show("打开成功：" + dlg.FileName);
                    }
                }
                catch (Exception ex) { MessageBox.Show("打开失败：" + ex.Message); }
            }
        });

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string p) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }
}