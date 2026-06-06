using System.Collections.Generic;
using System.Linq;
using Xradiator.Config;
using Xradiator.Model;

namespace Xradiator.Audio
{
	/// <summary>
	/// Plays a sound when a build newly breaks or is newly fixed.
	/// (Speech announcements were removed in the cross-platform port.)
	/// </summary>
	public class DiscJockey : IConfigObserver
	{
		IEnumerable<ProjectStatus> _previousBuildData = new List<ProjectStatus>();
		readonly IAudioPlayer _audioPlayer;
		string _brokenBuildSound;
		string _fixedBuildSound;

		public DiscJockey(IConfigSettings configSettings, IAudioPlayer audioPlayer)
		{
			_audioPlayer = audioPlayer;
			_brokenBuildSound = configSettings.BrokenBuildSound;
			_fixedBuildSound = configSettings.FixedBuildSound;

			configSettings.AddObserver(this);
		}

		public void PlaySounds(IEnumerable<ProjectStatus> currentBuildData)
		{
			var currentBuildDataList = currentBuildData as IList<ProjectStatus> ?? currentBuildData.ToList();

			var newlyBrokenBuilds =
				currentBuildDataList.Where(proj => proj.IsBroken)
									.Intersect(_previousBuildData.Where(proj => !proj.IsBroken)).ToList();

			if (newlyBrokenBuilds.Any())
			{
				_audioPlayer.Play(_brokenBuildSound);
			}
			else
			{
				var newlyFixedBuilds =
					currentBuildDataList.Where(proj => proj.IsSuccessful)
										.Intersect(_previousBuildData.Where(proj => !proj.IsSuccessful)).ToList();

				if (newlyFixedBuilds.Any())
				{
					_audioPlayer.Play(_fixedBuildSound);
				}
			}

			_previousBuildData = currentBuildDataList;
		}

		public void ConfigUpdated(ConfigSettings newSettings)
		{
			_brokenBuildSound = newSettings.BrokenBuildSound;
			_fixedBuildSound = newSettings.FixedBuildSound;
		}
	}
}
