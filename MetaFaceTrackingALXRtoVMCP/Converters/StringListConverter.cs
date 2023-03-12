using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MetaFaceTrackingALXRtoVMCP.Converters
{
    [ValueConversion(typeof(List<string>), typeof(string))]
    public class StringListConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(string))
                throw new InvalidOperationException("The target must be a String");

            return String.Join("\n", ((List<string>)value).ToArray());
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string s = (string)value;
            s = s.Replace("\r\n", "\n");
            var list = s.Split('\n').ToList();
            return list;
        }
    }
}
