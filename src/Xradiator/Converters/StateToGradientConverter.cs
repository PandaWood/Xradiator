using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Xradiator.Model;

namespace Xradiator.Converters
{
	public class StateToGradientConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			switch (value?.ToString().ToLower())
			{
				case ProjectStatus.SUCCESS:
				case ProjectStatus.NORMAL:
					return Colors.LightGreen;

				case ProjectStatus.BUILDING:
					return Color.FromArgb(255, 255, 255, 200);

				case ProjectStatus.FAILURE:
				case ProjectStatus.EXCEPTION:
				case ProjectStatus.ERROR:
					return Color.FromArgb(255, 255, 150, 150);

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
