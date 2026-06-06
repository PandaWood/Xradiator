using System;
using System.Net.Http;

namespace Xradiator.Services
{
	public class HttpWebClient : IWebClient
	{
		static readonly HttpClient _httpClient = new HttpClient
		{
			Timeout = TimeSpan.FromSeconds(30)
		};

		public string DownloadString(string url)
		{
			// kept synchronous to preserve the IWebClient contract; called on a background worker
			return _httpClient.GetStringAsync(new Uri(url)).GetAwaiter().GetResult();
		}
	}
}
