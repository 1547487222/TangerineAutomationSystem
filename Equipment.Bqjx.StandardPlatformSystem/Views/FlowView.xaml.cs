using HandyControl.Controls;
using MahApps.Metro.Controls;
using Nodify;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Equipment.Bqjx.StandardPlatformSystem.Views
{
    /// <summary>
    /// FlowView.xaml 的交互逻辑
    /// </summary>
    public partial class FlowView : UserControl
    {
        public FlowView()
        {
            InitializeComponent();
            NodifyEditor.EnableCuttingLinePreview = true;
            EditorGestures.Mappings.Editor.ZoomModifierKey = ModifierKeys.Control;
            EditorGestures.Mappings.Editor.PanWithMouseWheel = true;
        }

        private void ScrollViewer_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Shift)
                return;

            var scrollViewer = (System.Windows.Controls.ScrollViewer)sender;

            if (e.Key == Key.Left)
            {
                scrollViewer.PageLeft();
                e.Handled = true;
            }
            else if (e.Key == Key.Right)
            {
                scrollViewer.PageRight();
                e.Handled = true;
            }
        }

        private void DrawerHost_DrawerOpened(object sender, MaterialDesignThemes.Wpf.DrawerOpenedEventArgs e)
        {
            //linknodeTabcontrol.SelectedIndex = 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var canvas = editor;

            Size canvasSize = new Size(2000, 2500);

            // 创建 RenderTargetBitmap
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                (int)canvasSize.Width,
                (int)canvasSize.Height,
                96d,
                96d,
                PixelFormats.Pbgra32);

            // 将 Canvas 渲染到 RenderTargetBitmap
            canvas.Measure(canvasSize);
            canvas.Arrange(new Rect(canvasSize));
            renderBitmap.Render(editor);

            // 创建 PngBitmapEncoder 并添加 RenderTargetBitmap
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PNG Files (*.png)|*.png",
                DefaultExt = ".png"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    encoder.Save(fileStream);
                    System.Windows.MessageBox.Show("Canvas exported to PNG successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
    }
}
