using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Xml.Linq;
using Xradiator.Model;
using Xradiator.Model.TeamCity;
using log4net;

namespace Xradiator.Services
{
	/// <summary>
	/// Talks to TeamCity's native REST API (/app/rest) and produces the legacy
	/// CruiseControl-style &lt;Projects&gt; XML that <see cref="BuildDataTransformer"/> already
	/// understands. This keeps the rich data (breakers, category, server name) that the
	/// built-in cctray feed drops, while reusing the whole existing transform/skin pipeline.
	///
	/// URL conventions (all standard http/https so the UrlParser accepts them):
	///   http://host/guestAuth/app/rest          -> guest auth
	///   http://host/app/rest?token=XXXX          -> bearer-token auth
	///   http://host/httpAuth/app/rest?token=XXXX -> bearer-token auth
	///
	/// NOTE: the exact REST field names below should be validated against the live
	/// TeamCity 2026 server; they follow the long-stable /app/rest schema.
	/// </summary>
	public class TeamCityRestWebClient : IWebClient
	{
		static readonly ILog _log = LogManager.GetLogger(nameof(TeamCityRestWebClient));
		static readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

		const string BuildTypesFields =
			"count,buildType(id,name,projectName,webUrl," +
			"builds($locator(count:1,running:any,canceled:any),build(number,status,state,finishDate,statusText)))";

		public static bool Handles(string url)
		{
			if (string.IsNullOrEmpty(url)) return false;
			var u = url.ToLowerInvariant();
			return u.Contains("/app/rest") && !u.Contains("/app/rest/cctray");
		}

		public string DownloadString(string url)
		{
			var endpoint = TeamCityEndpoint.Parse(url);
			var json = Get(endpoint, $"{endpoint.RestBase}/buildTypes?fields={Uri.EscapeDataString(BuildTypesFields)}");

			var projects = ParseBuildTypes(endpoint, json);
			return BuildCCNetXml(endpoint, projects).ToString();
		}

		IEnumerable<TeamCityBuildType> ParseBuildTypes(TeamCityEndpoint endpoint, string json)
		{
			using var doc = JsonDocument.Parse(json);
			if (!doc.RootElement.TryGetProperty("buildType", out var buildTypes)) yield break;

			foreach (var bt in buildTypes.EnumerateArray())
			{
				var build = bt.TryGetProperty("builds", out var builds)
							&& builds.TryGetProperty("build", out var buildArr)
							&& buildArr.GetArrayLength() > 0
					? buildArr[0]
					: default;

				yield return new TeamCityBuildType
				{
					Id = Str(bt, "id"),
					Name = Str(bt, "name"),
					ProjectName = Str(bt, "projectName"),
					WebUrl = Str(bt, "webUrl"),
					Status = Str(build, "status"),
					State = Str(build, "state"),
					Number = Str(build, "number"),
					FinishDate = Str(build, "finishDate"),
				};
			}
		}

		XDocument BuildCCNetXml(TeamCityEndpoint endpoint, IEnumerable<TeamCityBuildType> buildTypes)
		{
			var root = new XElement("Projects", new XAttribute("CCType", "TeamCity"));

			foreach (var bt in buildTypes)
			{
				var lastBuildStatus = MapStatus(bt.Status, bt.Number);
				var activity = string.Equals(bt.State, "running", StringComparison.OrdinalIgnoreCase)
					? "Building" : "Sleeping";

				var breakers = lastBuildStatus == "Failure" ? FindBreakers(endpoint, bt.Id) : string.Empty;
				var currentMessage = breakers.Length == 0 ? string.Empty : $"Breakers: {breakers}";

				var project = new XElement("Project",
					new XAttribute("name", bt.Name ?? bt.Id ?? string.Empty),
					new XAttribute("category", bt.ProjectName ?? string.Empty),
					new XAttribute("activity", activity),
					new XAttribute("lastBuildStatus", lastBuildStatus),
					new XAttribute("lastBuildLabel", bt.Number ?? string.Empty),
					new XAttribute("lastBuildTime", ToIso(bt.FinishDate)),
					new XAttribute("webUrl", bt.WebUrl ?? string.Empty),
					new XAttribute("serverName", endpoint.ServerName),
					new XAttribute("CurrentMessage", currentMessage));

				if (breakers.Length > 0)
				{
					project.Add(new XElement("messages",
						new XElement("message",
							new XAttribute("text", breakers),
							new XAttribute("kind", "Breakers"))));
				}

				root.Add(project);
			}

			return new XDocument(root);
		}

		string FindBreakers(TeamCityEndpoint endpoint, string buildTypeId)
		{
			if (string.IsNullOrEmpty(buildTypeId)) return string.Empty;

			try
			{
				var locator = Uri.EscapeDataString($"buildType:(id:{buildTypeId})");
				var url = $"{endpoint.RestBase}/changes?locator={locator},count:5&fields=change(username,user(username,name))";
				var json = Get(endpoint, url);

				using var doc = JsonDocument.Parse(json);
				if (!doc.RootElement.TryGetProperty("change", out var changes)) return string.Empty;

				var names = new List<string>();
				foreach (var change in changes.EnumerateArray())
				{
					var name = Str(change, "username");
					if (string.IsNullOrEmpty(name) && change.TryGetProperty("user", out var user))
						name = Str(user, "username");
					if (!string.IsNullOrEmpty(name) && !names.Contains(name)) names.Add(name);
				}
				return string.Join(", ", names);
			}
			catch (Exception ex)
			{
				_log.DebugFormat("Could not resolve breakers for {0}: {1}", buildTypeId, ex.Message);
				return string.Empty;
			}
		}

		string Get(TeamCityEndpoint endpoint, string url)
		{
			using var request = new HttpRequestMessage(HttpMethod.Get, url);
			request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			if (!string.IsNullOrEmpty(endpoint.Token))
				request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", endpoint.Token);

			using var response = _http.Send(request);
			response.EnsureSuccessStatusCode();
			using var reader = new System.IO.StreamReader(response.Content.ReadAsStream());
			return reader.ReadToEnd();
		}

		static string MapStatus(string teamCityStatus, string buildNumber)
		{
			if (string.IsNullOrEmpty(buildNumber)) return "Unknown"; // never built
			return teamCityStatus?.ToUpperInvariant() switch
			{
				"SUCCESS" => "Success",
				"FAILURE" => "Failure",
				"ERROR" => "Exception",
				_ => "Unknown",
			};
		}

		/// <summary> TeamCity dates are "yyyyMMdd'T'HHmmsszzzz" (no separators); normalise to ISO 8601. </summary>
		static string ToIso(string teamCityDate)
		{
			if (string.IsNullOrEmpty(teamCityDate)) return DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");

			if (DateTime.TryParseExact(teamCityDate, "yyyyMMdd'T'HHmmsszzz",
					System.Globalization.CultureInfo.InvariantCulture,
					System.Globalization.DateTimeStyles.None, out var dt))
				return dt.ToString("yyyy-MM-ddTHH:mm:ss");

			return DateTime.TryParse(teamCityDate, out var any)
				? any.ToString("yyyy-MM-ddTHH:mm:ss")
				: DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
		}

		static string Str(JsonElement element, string property)
		{
			if (element.ValueKind != JsonValueKind.Object) return string.Empty;
			return element.TryGetProperty(property, out var v) && v.ValueKind == JsonValueKind.String
				? v.GetString()
				: string.Empty;
		}
	}
}
