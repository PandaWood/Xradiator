using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Xradiator.Model;

namespace Xradiator.Converters
{
	/// <summary>
	/// Builds the per-project background gradient (top = lighter "gradient" colour,
	/// bottom = solid state colour). Replaces the two WPF per-GradientStop bindings,
	/// which don't reliably inherit DataContext on non-visual objects in Avalonia.
	/// </summary>
	public class StateToBackgroundConverter : IValueConverter
	{
		static readonly StateToGradientConverter _gradient = new StateToGradientConverter();
		static readonly StateToColorConverter _solid = new StateToColorConverter();

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var top = (Color)_gradient.Convert(value, typeof(Color), null, culture);
			var bottom = (Color)_solid.Convert(value, typeof(Color), null, culture);

			return new LinearGradientBrush
			{
				StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
				EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
				GradientStops =
				{
					new GradientStop(top, 0),
					new GradientStop(bottom, 0.5),
				}
			};
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
