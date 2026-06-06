using Xradiator.Model;
using log4net;

namespace Xradiator.Services
{
	public interface IWebClientFactory
	{
		IWebClient GetWebClient(string url);
	}

	public class WebClientFactory : IWebClientFactory
	{
		static readonly ILog _log = LogManager.GetLogger(nameof(WebClientFactory));

		public IWebClient GetWebClient(string url)
		{
			IWebClient webClient;

			var uri = new UrlParser(url);
			if (uri.IsDebug)
			{
				webClient = new SandboxWebClient();
			}
			else if (TeamCityRestWebClient.Handles(url))
			{
				// native TeamCity REST endpoint (rich data: breakers, categories, server)
				webClient = new TeamCityRestWebClient();
			}
			else
			{
				// plain HTTP: legacy CCNet XmlStatusReport or TeamCity's /app/rest/cctray feed
				webClient = new HttpWebClient();
			}

			_log.DebugFormat("Using WebClient: {0}", webClient.GetType());
			return webClient;
		}
	}
}