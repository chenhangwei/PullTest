using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PullTest.Converters
{
    public  class DoubleToStringConverter : IValueConverter

    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str="";
            if (value is  null)
            {
                return "空的啊";
            }
            else
            {
                str = System.Convert.ToString(value)??"没有值";
            return str;

            }
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double d=0.0;
            if (value is null)
            {
                return 0.0;
            }
            else
            {
                d = System.Convert.ToDouble(value);
                return d;
            }


        }
    }
}
