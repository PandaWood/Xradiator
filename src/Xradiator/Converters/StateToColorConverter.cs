using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Xradiator.Model;

namespace Xradiator.Converters
{
	public class StateToColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			switch (value?.ToString().ToLower())
			{
				case ProjectStatus.SUCCESS:
				case ProjectStatus.NORMAL:
					return Colors.LimeGreen;

				case ProjectStatus.BUILDING:
					return Colors.Yellow;

				case ProjectStatus.FAILURE:
				case ProjectStatus.EXCEPTION:
				case ProjectStatus.ERROR:
					return Colors.Red;

				default:
					return Colors.White;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
