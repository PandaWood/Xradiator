using System;
using System.IO;
using System.Reflection;

namespace Xradiator.Config
{
	public interface IConfigLocation
	{
		string FileName { get; }
	}

	public class ConfigLocation : IConfigLocation
	{
		public string FileName
		{
			get
			{
				var name = Assembly.GetExecutingAssembly().GetName().Name;
				return Path.Combine(AppContext.BaseDirectory, name + ".dll.config");
			}
		}
	}

	public interface IAppLocation
	{
		string DirectoryName { get; }
	}

	public class AppLocation : IAppLocation
	{
		public string DirectoryName
		{
			get { return AppContext.BaseDirectory; }
		}
	}
}