using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlzEx.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;
using QStandaedPlatform.Engine.Common.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equipment.Bqjx.StandardPlatformSystem.Models
{
    public partial class GrpcProjectGenerateEditViewModel : ObservableObject
    {
        private readonly GrpcProjectModel _grpcProjectModel;
        public GrpcProjectGenerateEditViewModel(GrpcProjectModel grpcProjectModel)
        {
            _grpcProjectModel = grpcProjectModel;

        }

        public GrpcProjectModel Model => _grpcProjectModel;


        [RelayCommand]
        private void SelectSolutionRootPath()
        {
            //选择文件夹
            using CommonOpenFileDialog dialog = new();
            dialog.IsFolderPicker = true;

            dialog.Title = "请选择解决方案根目录";

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {

                string? selectedPath = dialog.FileName;
                if (!string.IsNullOrEmpty(selectedPath))
                    Model.SolutionRootPath = selectedPath;
            }
        }
    }
}
