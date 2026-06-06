using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xradiator.Config;
using Xradiator.Extensions;
using log4net;

namespace Xradiator.Model
{
	public class BuildBusterImageDecorator : IBuildBuster
	{
		static readonly ILog _log = LogManager.GetLogger(nameof(BuildBusterImageDecorator));

		readonly IBuildBuster _buildBuster;
		readonly string _imageFolder;
		readonly List<char> InvalidCharacterList = Path.GetInvalidFileNameChars().ToList();

		public BuildBusterImageDecorator([InjectBuildBuster] IBuildBuster buildBuster, IAppLocation appLocation)
		{
			_buildBuster = buildBuster;
			_imageFolder = Path.Combine(appLocation.DirectoryName, "images");
		}

		public string FindBreaker(string currentMessage)
		{
			var username = _buildBuster.FindBreaker(currentMessage);

			if (username.ContainsInvalidChars())
			{
				foreach (var c in username.ToCharArray().Where(InvalidCharacterList.Contains))
				{
					username = username.Replace(c.ToString(), "");
				}
			}

			var imagePath = Path.Combine(_imageFolder, $"{username.Trim()}.jpg");

			_log.DebugFormat("Breaker image='{0}'", imagePath);

			return imagePath;
		}
	}
}