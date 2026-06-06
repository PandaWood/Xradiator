using System;
using System.Collections.Generic;
using System.Linq;
using Xradiator.Config;
using Ninject;

namespace Xradiator.Model
{
	public class ViewUrl
	{
		// ReSharper disable UnusedMember.Global
		[Inject]
		public ViewUrl(IConfigSettings settings) : this(settings.URL) {}
		// ReSharper restore UnusedMember.Global

		public ViewUrl(string url)
		{
			Url = url;
		}

		public string Url { get; set; }

		public IEnumerable<string> UriList
		{
			get
			{
				return from url in Url.Split(' ')
					   let u = new UrlParser(url)
					   where u.IsValid
					   select u.Url;
			}
		}
	}
}