using System.Xml.Linq;

namespace Xradiator.Extensions
{
	public static class XmlExtensions
	{
		public static string GetValue(this XAttribute attribute)
		{
			return attribute == null ? string.Empty : attribute.Value;
		}
	}
}