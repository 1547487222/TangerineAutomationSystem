using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Equipment.Bqjx.StandardPlatformSystem.Converters
{
    public class ToolStateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() switch
            {
                "None" => "#78909C",
                "Error" => "#D32F2F",
                "Finish" => "#388E3C",
                "Running" => "#0288D1",
                "Forewarn" => "#F57C00",
                "SyncWaiting" => "#FFCA28",
                _ => "#78909C"
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
