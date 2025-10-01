using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Equipment.Bqjx.StandardPlatformSystem.Models;
using Equipment.Bqjx.StandardPlatformSystem.Views;
using Equipment.Bqjx.StandardPlatformSystem.WorkFlows;
using HandyControl.Tools.Extension;
using MahApps.Metro.Controls;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Newtonsoft.Json;
using Nodify;
using QStandaedPlatform.Engine.Common.Common;
using QStandaedPlatform.Engine.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Linq;

namespace Equipment.Bqjx.StandardPlatformSystem
{
   public partial class WorkFlowViewModel:ObservableObject
    {

        private readonly ComponentManagementModel _componentManagementModel = new();
        public ObservableCollection<FlowModel> FlowModels { get; set; } = [];
        [ObservableProperty]
        private FlowModel? _selectedModel = null;

        partial void OnSelectedModelChanged(FlowModel? value)
        {
            Task.Delay(500).ContinueWith(p => 
            {
                App.Current.Dispatcher.Invoke(() => 
                {
                    App.ToolEngine.RaisePartCollectionChanged();
                    ParameterModelRepository.RaiseChanged();
                });
            });

            value?.ResetPanelExpanded();
        }
        public WorkFlowViewModel()
        {
            App.Current.Dispatcher.InvokeAsync(Init);
        }

        private  void Init()
        {
           var  flowFileDescriptions= App.ToolEngine.GetFlowFileDescriptions();
            foreach (var fileDescription in flowFileDescriptions)
            {
                if (App.ToolEngine.ReadFlow(fileDescription, out var flow))
                {
                    if (flow != null)
                    {
                        var flowModel = new FlowModel(flow);
                        FlowModels.Add(flowModel);
                    }
                }
            }
            App.ToolEngine.RaisePartCollectionChanged();
            ParameterModelRepository.RaiseChanged();
        }

        [RelayCommand]
        private void NewFlow()
        {
            var guid = Guid.NewGuid();
            var flow = App.ToolEngine.CreateNewFlow(FlowModels.Count + 1+"_"+"未定义流程名",guid);
            FlowModels.Add(new FlowModel(flow));
        }
        [RelayCommand]
        private void DeleteFlow(FlowModel flowModel)
        {
            if (MessageBox.Show($"确定删除流程<{flowModel.FlowName}>吗？", "提示", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (FlowModels.Remove(flowModel))
                {
                   App.ToolEngine.DeleteFlow(flowModel.Flow);
                }
            }
        }
        [RelayCommand]
        private void OpenPartManage()
        {
            var view = new ComponentManagementView
            {
                DataContext = _componentManagementModel
            };
            view.Closed += View_Closed;
            view.ShowDialog();
        }

        private void View_Closed(object? sender, EventArgs e)
        {
            if (sender is Window window)
                window.Closed -= View_Closed;
            App.ToolEngine.SaveAllPart();
            App.ToolEngine.RaisePartCollectionChanged();
        }
        [RelayCommand]
        private void ImportFlow()
        {
            new Action(() => 
            {
                OpenFileDialog openFileDialog = new()
                {
                    Filter = "Flow files (*.flow)|*.flow",
                    DefaultExt = ".flow",
                    Multiselect = false
                };
                Nullable<bool> result = openFileDialog.ShowDialog();
                if (result == true)
                {
                    string filePath = openFileDialog.FileName;
                    if (App.ToolEngine.ReadFlow(filePath, out var flow))
                    {
                        if (flow != null)
                        {
                            FlowModels.Add(new FlowModel(flow));
                            MessageBox.Show($"流程<{flow.FlowName}>导入成功！");
                            App.ToolEngine.RaisePartCollectionChanged();
                            return;
                        }
                    }
                }
                else

                    MessageBox.Show("流程导入失败...");
            }).TryCatch(ex => 
            {
                MessageBox.Show(ex.Message);
            });
        }
        [RelayCommand]
        private void WindowCancel()
        {
            App.Current.Shutdown();
        }

        [RelayCommand]
        private void OpenScriptEditor()
        {
            new Action(()=> new ScriptEditorView() 
            {
                Owner=App.Current.MainWindow
            }.ShowDialog()).TryCatch(ex => 
            {
                MessageBox.Show(ex.Message);
            });
        }
        [RelayCommand]
        private void OpenTemplateCodeGenerationEditor()
        {
            new Action(() => new TemplateCodeGenerationEditorView()
            {
                Owner = App.Current.MainWindow
            }.Show()).TryCatch(ex => 
            {
                MessageBox.Show(ex.Message);
            });
        }
        [RelayCommand]
        private void OpenGrpcProjectGenerate()
        {
            new Action(() => Application.Current.Invoke(() => 
            {
                new GrpcProjectGenerateView()
                {
                    Owner = App.Current.MainWindow
                }.Show();
            })
            ).TryCatch(ex => 
            {
                MessageBox.Show(ex.Message);
            });
        }

        private ParameterTableManagerView _parameterTableManagerView;
        [RelayCommand]
        private void OpenParameterTableManagerView()
        {
            new Action(() =>
            {
                Application.Current.Dispatcher.Invoke((Delegate)(() =>
                {
                    //new ParameterTableManagerView()
                    //{
                    //    Owner = Application.Current.MainWindow
                    //}.Show();
                    _parameterTableManagerView ??= new ParameterTableManagerView();
                    if (!_parameterTableManagerView.IsLoaded)
                        _parameterTableManagerView.Show();
                    else
                        _parameterTableManagerView.OpenTable();
                }));
            }).TryCatch(ex =>
            {
                MessageBox.Show(ex.Message);
            });
            ;
        }
    }
}
