using System;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using Xradiator.Extensions;
using log4net;

namespace Xradiator.Config
{
	/// <summary>
	/// configuration settings
	/// </summary>
	public class ConfigSettings : ViewSettings, IConfigSettings
	{
		const int DefaultPollingFrequency = 30;

		static readonly ConfigLocation _configLocation = new ConfigLocation();
		readonly IList<IConfigObserver> _observers = new List<IConfigObserver>();
		static readonly ILog _log = LogManager.GetLogger(typeof(ConfigSettings).Name);
		IDictionary<string, string> _usernameMap = new Dictionary<string, string>();
		readonly UserNameMappingReader _userNameMappingReader = new UserNameMappingReader(_configLocation);
		ICollection<ViewSettings> _viewList = new List<ViewSettings>();
		readonly Queue<ViewSettings> _viewQueue = new Queue<ViewSettings>();

		public ConfigSettings()
		{
			ServerNameRegEx = CategoryRegEx = ProjectNameRegEx = ".*";
		}

		public void Load()
		{
			LoadViewSettings();
			ApplyViewSettings();

			var config = OpenExeConfiguration();
			PollFrequency = config.GetIntProperty(PollFrequencyKey, DefaultPollingFrequency);
			ShowCountdown = config.GetBoolProperty(ShowCountdownKey);
			ShowProgress = config.GetBoolProperty(ShowProgressKey);
			PlaySounds = config.GetBoolProperty(PlaySoundsKey);
			BrokenBuildSound = config.GetProperty(BrokenBuildSoundKey).Value;
			FixedBuildSound = config.GetProperty(FixedBuildSoundKey).Value;
			_breakerGuiltStrategy = config.GetProperty(BreakerGuiltStrategyKey).Value;

			_usernameMap = _userNameMappingReader.Read();
		}

		public void Save()
		{
			try
			{
				if (IsOneView)
				{
					ViewSettingsParser.Modify(_configLocation.FileName, new ViewSettings
					{
						URL = URL,
						ProjectNameRegEx = ProjectNameRegEx,
						CategoryRegEx = CategoryRegEx,
						ServerNameRegEx = ServerNameRegEx,
						SkinName = SkinName,
						ViewName = ViewName,
						ShowOnlyBroken = ShowOnlyBroken,
						ShowServerName = ShowServerName,
						ShowOutOfDate = ShowOutOfDate,
						OutOfDateDifferenceInMinutes = OutOfDateDifferenceInMinutes
					});
				}

				var config = OpenExeConfiguration();

				config.AppSettings.Settings[PollFrequencyKey].Value = PollFrequency.ToString();
				config.AppSettings.Settings[ShowCountdownKey].Value = ShowCountdown.ToString();
				config.AppSettings.Settings[ShowProgressKey].Value = ShowProgress.ToString();
				config.AppSettings.Settings[PlaySoundsKey].Value = PlaySounds.ToString();
				config.AppSettings.Settings[BrokenBuildSoundKey].Value = BrokenBuildSound;
				config.AppSettings.Settings[FixedBuildSoundKey].Value = FixedBuildSound;
				config.AppSettings.Settings[BreakerGuiltStrategyKey].Value = _breakerGuiltStrategy;
				config.Save(ConfigurationSaveMode.Minimal);
			}
			catch (Exception ex)
			{
				// config may be edited in the file (manually) - we cannot show an error dialog here
				// because it's entirely reasonable that the user doesn't have access to the machine running 
				// the exe, in order to close a dialog
				_log.Error(ex.Message, ex);		
			}
		}

		/// <summary>
		/// rotate/set the next view in the queue (if >1)
		/// </summary>
		public void RotateView()
		{
			if (IsOneView) return;

			ApplyViewSettings();
			NotifyObservers();
		}

		void ApplyViewSettings()
		{
			if (_viewQueue.Count == 0)
				_viewList.ForEach(_viewQueue.Enqueue);

			var q = _viewQueue.Dequeue();
			URL = q.URL;
			SkinName = q.SkinName;
			ProjectNameRegEx = q.ProjectNameRegEx;
			CategoryRegEx = q.CategoryRegEx;
			ServerNameRegEx = q.ServerNameRegEx;
			ViewName = q.ViewName;
			ShowOnlyBroken = q.ShowOnlyBroken;
			ShowServerName = q.ShowServerName;
			ShowOutOfDate = q.ShowOutOfDate;
			OutOfDateDifferenceInMinutes = q.OutOfDateDifferenceInMinutes;
		}

		private void LoadViewSettings()
		{
			_viewList = ViewSettingsParser.Read(_configLocation.FileName);
			_viewQueue.Clear();
		}

		public void AddObserver(IConfigObserver observer)
		{
			_observers.Add(observer);
		}

		public void NotifyObservers()
		{
			foreach (var observer in _observers)
			{
				observer.ConfigUpdated(this);
			}
		}

		private static Configuration OpenExeConfiguration()
		{
			return ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
		}

		public void Log()
		{
			_log.InfoFormat("Config file updated. {0}Settings: {1}", IsMultiView ? "(non-view) " : "", this);
		}

		public override string ToString()
		{
			if (IsOneView)
				return string.Format("Url={0}, SkinName={1}, PollFrequency={2}, ProjectNameRegEx={3}, ShowCountdown={4}, ShowProgress={5}, PlaySounds={6}, BrokenBuildSound={7}, FixedBuildSound={8}, CategoryRegEx={9}, BreakerGuiltStrategy={10}",
					_url, _skinName, _pollFrequency, _projectNameRegEx, _showCountdown, _showProgress, _playSounds, _brokenBuildSound, _fixedBuildSound, _categoryRegEx, _breakerGuiltStrategy);

			return string.Format("PollFrequency={0}, ShowCountdown={1}, ShowProgress={2}, BrokenBuildSound={3}, FixedBuildSound={4}, PlaySounds={5}, BreakerGuiltStrategy={6}",
				_pollFrequency, _showCountdown, _showProgress, _brokenBuildSound, _fixedBuildSound, _playSounds, _breakerGuiltStrategy);
		}


		/// <summary> interval at which to poll (in seconds) </summary>
		private int _pollFrequency;
		public int PollFrequency
		{
			get { return _pollFrequency; }
			set
			{
				if (_pollFrequency == value) return;
				_pollFrequency = value;
				Notify("PollFrequency");
			}
		}

		private bool _showCountdown;
		public bool ShowCountdown
		{
			get { return _showCountdown; }
			set
			{
				if (_showCountdown == value) return;
				_showCountdown = value;
				Notify("ShowCountdown");
			}
		}

		private bool _showProgress;
		public bool ShowProgress
		{
			get { return _showProgress; }
			set
			{
				if (_showProgress == value) return;
				_showProgress = value;
				Notify("ShowProgress");
			}
		}

		public IDictionary<string, string> UsernameMap
		{
			get { return _usernameMap; }
			set { _usernameMap = value; }
		}

		private string _brokenBuildSound;
		public string BrokenBuildSound
		{
			get { return _brokenBuildSound; }
			set
			{
				if (_brokenBuildSound == value) return;
				_brokenBuildSound = value;
				Notify("BrokenBuildSound");
			}
		}

		private string _fixedBuildSound;
		public string FixedBuildSound
		{
			get { return _fixedBuildSound; }
			set
			{
				if (_fixedBuildSound == value) return;
				_fixedBuildSound = value;
				Notify("FixedBuildSound");
			}
		}

		private bool _playSounds;
		public bool PlaySounds
		{
			get { return _playSounds; }
			set
			{
				if (_playSounds == value) return;
				_playSounds = value;
				Notify("PlaySounds");
			}
		}

		private string _breakerGuiltStrategy;
		public GuiltStrategyType BreakerGuiltStrategy
		{
			get { return _breakerGuiltStrategy == "First" ? GuiltStrategyType.First : GuiltStrategyType.Last; }
		}

		public TimeSpan PollFrequencyTimeSpan
		{
			get { return TimeSpan.FromSeconds(PollFrequency); }
		}

		// ReSharper disable UnusedMember.Global
		public bool IsMultiView
		{
			get { return _viewList.Count > 1; }
		}
		// ReSharper restore UnusedMember.Global

		// ReSharper disable MemberCanBePrivate.Global
		public bool IsOneView
		{
			get { return _viewList.Count == 1; }
		}
		// ReSharper restore MemberCanBePrivate.Global

		// the placement of these variables is commensurate with their importance - low
		const string PollFrequencyKey = "PollFrequency";
		const string ShowCountdownKey = "ShowCountdown";
		const string ShowProgressKey = "ShowProgress";
		const string PlaySoundsKey = "PlaySounds";
		const string BrokenBuildSoundKey = "BrokenBuildSound";
		const string FixedBuildSoundKey = "FixedBuildSound";
		const string BreakerGuiltStrategyKey = "BreakerGuiltStrategy";
	}

	public enum GuiltStrategyType
	{
		First,
		Last
	}

	public class ConfigSettingsException : Exception
	{
		public ConfigSettingsException(string key) : 
			base(string.Format("Configuration setting missing: '{0}'", key)) 
		{  }
	}
}