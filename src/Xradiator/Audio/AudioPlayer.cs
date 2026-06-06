using System.IO;
using Xradiator.Config;
using log4net;

namespace Xradiator.Audio
{
	public interface IAudioPlayer
	{
		void Play(string filename);
	}

	public class AudioPlayer : IAudioPlayer, IConfigObserver
	{
		static readonly ILog _log = LogManager.GetLogger(nameof(AudioPlayer));

		readonly string _wavFileFolder;
		bool _playSounds;

		public AudioPlayer(IConfigSettings configSettings, IAppLocation appLocation)
		{
			_playSounds = configSettings.PlaySounds;
			_wavFileFolder = Path.Combine(appLocation.DirectoryName, "sounds");
			configSettings.AddObserver(this);
		}

		/// <summary>
		/// this is for testing only
		/// </summary>
		public AudioPlayer(string path)
		{
			_wavFileFolder = path;
			_playSounds = true;
		}

		public void Play(string soundFileName)
		{
			if (!_playSounds || string.IsNullOrEmpty(soundFileName)) return;

			var soundFile = Path.Combine(_wavFileFolder, soundFileName);
			if (!File.Exists(soundFile))
			{
				_log.WarnFormat("Sound file not found: {0}", soundFile);
				return;
			}

			CrossPlatformSoundPlayer.Play(soundFile);
		}

		void IConfigObserver.ConfigUpdated(ConfigSettings newSettings)
		{
			_playSounds = newSettings.PlaySounds;
		}
	}
}
