using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace todolist.Converters
{
    public class ShrinkConverter : MarkupExtension, IValueConverter
    {
        private static ShrinkConverter _instance;

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new ShrinkConverter());
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToDouble(value) - System.Convert.ToDouble(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}