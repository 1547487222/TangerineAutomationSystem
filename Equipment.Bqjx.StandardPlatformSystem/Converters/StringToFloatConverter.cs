using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Equipment.Bqjx.StandardPlatformSystem.Converters
{
    public class StringToFloatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return 0f;
            }
            if (value.ToString() == "")
            { 
                return 0f;
            }
            if (float.TryParse(value.ToString(), out float result))
            {
                return result;
            }
            return 0f;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
         return   Binding.DoNothing;
        }
    }
}
