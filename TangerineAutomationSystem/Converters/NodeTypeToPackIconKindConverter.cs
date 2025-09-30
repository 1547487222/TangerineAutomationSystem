using System;
using System.Globalization;
using System.Windows.Data;
using MaterialDesignThemes.Wpf;

namespace TangerineAutomationSystem.Converters
{
    public class NodeTypeToPackIconKindConverter : IValueConverter
    {
        // Map your node types to PackIcon kinds; expand as needed
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return PackIconKind.CubeOutline;
            var s = value.ToString();
            return s switch
            {
                "Lab" => PackIconKind.Library,
                "Line" => PackIconKind.AccountTie,
                "Platform" => PackIconKind.GridLarge,
                "Module" => PackIconKind.Cog,
                "LabResource" => PackIconKind.Storage,
                "PlatformResource" => PackIconKind.Grid,
                _ => PackIconKind.CubeOutline,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}