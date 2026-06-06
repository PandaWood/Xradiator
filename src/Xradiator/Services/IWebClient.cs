using System;

namespace Xradiator.Services
{
	public interface IWebClient
	{
		string DownloadString(string url);
	}
}