using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Xradiator.Converters
{
	public class BuildNameToMessageConverter : IMultiValueConverter
	{
		public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
		{
			var projectName = (values[0]?.ToString() ?? string.Empty).Replace("_", " ");
			var message = values[1]?.ToString() ?? string.Empty;

			return message.Trim().Length == 0 ?
				projectName : $"{projectName}\n{message}";
		}
	}
}
