using Xradiator.Model.TeamCity;
using Xradiator.Services;
using NUnit.Framework;
using Shouldly;

namespace Xradiator.Tests
{
	[TestFixture]
	public class TeamCity_Tests
	{
		[TestCase("http://host/app/rest", true)]
		[TestCase("http://host/guestAuth/app/rest", true)]
		[TestCase("http://host/app/rest?token=abc", true)]
		[TestCase("http://host/guestAuth/app/rest/cctray/projects.xml", false)] // cctray -> plain http
		[TestCase("http://ccnetlive/ccnet/XmlStatusReport.aspx", false)]
		[TestCase("debughttp://host", false)]
		[TestCase("", false)]
		public void Handles_only_native_rest_urls(string url, bool expected)
		{
			TeamCityRestWebClient.Handles(url).ShouldBe(expected);
		}

		[Test]
		public void Endpoint_defaults_to_guestAuth_when_no_token()
		{
			var endpoint = TeamCityEndpoint.Parse("http://buildserver:8111/app/rest");

			endpoint.RestBase.ShouldBe("http://buildserver:8111/guestAuth/app/rest");
			endpoint.ServerName.ShouldBe("buildserver");
			endpoint.Token.ShouldBeNull();
		}

		[Test]
		public void Endpoint_uses_token_and_no_guestAuth_segment()
		{
			var endpoint = TeamCityEndpoint.Parse("http://buildserver/app/rest?token=SECRET");

			endpoint.RestBase.ShouldBe("http://buildserver/app/rest");
			endpoint.Token.ShouldBe("SECRET");
		}

		[Test]
		public void Endpoint_preserves_explicit_guestAuth_segment()
		{
			var endpoint = TeamCityEndpoint.Parse("https://tc.example.com/guestAuth/app/rest");

			endpoint.RestBase.ShouldBe("https://tc.example.com/guestAuth/app/rest");
			endpoint.ServerName.ShouldBe("tc.example.com");
		}
	}
}
