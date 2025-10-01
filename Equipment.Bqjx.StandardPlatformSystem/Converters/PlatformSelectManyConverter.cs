using Equipment.Bqjx.StandardPlatformSystem.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Equipment.Bqjx.StandardPlatformSystem.Converters
{
    public class PlatformSelectManyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ObservableCollection<PlatformCreateModel> platformCreateModels)
            {
                return platformCreateModels.SelectMany(p =>p.StartTaskFiles).ToList();
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
