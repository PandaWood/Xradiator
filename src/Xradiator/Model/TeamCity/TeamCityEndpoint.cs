using System;
using System.Collections.Generic;

namespace Xradiator.Model.TeamCity
{
	/// <summary>
	/// Parses a configured TeamCity URL into the pieces the REST client needs:
	/// the /app/rest base, the server (host) name, and an optional auth token.
	/// </summary>
	public class TeamCityEndpoint
	{
		public string RestBase { get; private set; }
		public string ServerName { get; private set; }
		public string Token { get; private set; }

		public static TeamCityEndpoint Parse(string url)
		{
			var uri = new Uri(url);
			var origin = $"{uri.Scheme}://{uri.Authority}";

			var token = GetQueryValue(uri.Query, "token");

			// preserve the auth segment the user chose; default to guestAuth when no token
			var path = uri.AbsolutePath;
			var authSegment = path.Contains("/guestAuth") ? "/guestAuth"
				: path.Contains("/httpAuth") ? "/httpAuth"
				: string.IsNullOrEmpty(token) ? "/guestAuth" : string.Empty;

			return new TeamCityEndpoint
			{
				RestBase = $"{origin}{authSegment}/app/rest",
				ServerName = uri.Host,
				Token = token,
			};
		}

		static string GetQueryValue(string query, string key)
		{
			if (string.IsNullOrEmpty(query)) return null;
			foreach (var pair in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
			{
				var idx = pair.IndexOf('=');
				if (idx <= 0) continue;
				if (Uri.UnescapeDataString(pair.Substring(0, idx)) == key)
					return Uri.UnescapeDataString(pair.Substring(idx + 1));
			}
			return null;
		}
	}
}
