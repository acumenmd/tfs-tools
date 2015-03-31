using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TFSBuildDefinitionHelper.Resources.Converters
{
	public class WorkingPathConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var fullPath = value.ToString();
			if (!String.IsNullOrWhiteSpace(fullPath))
			{
				return fullPath.Split('/').Last();
			}

			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
