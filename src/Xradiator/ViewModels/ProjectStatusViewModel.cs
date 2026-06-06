using System;
using Avalonia.Media.Imaging;
using Xradiator.Extensions;
using Xradiator.Model;

namespace Xradiator.ViewModels
{
	public class ProjectStatusViewModel : NotifyingClass
	{
		private string _name;
		private string _currentState;
		private string _CurrentMessage;
		private string _ProjectActivity;
		private string _serverName;
		private bool _isBroken;
		private bool _isSuccessful;
		private bool _serverNameVisible;
		private DateTime _lastBuildTime;
		private string _breakerLabel = string.Empty;
		private Bitmap _breakerImage;
		private double _breakerImageSize;

		[Obsolete("only used by xaml")]
		public ProjectStatusViewModel()
		{
		}

		public ProjectStatusViewModel(Model.ProjectStatus ps, Config.IViewSettings vs,
										IBuildBuster imageBuster = null, IBuildBuster nameBuster = null)
		{
			this.CurrentMessage = ps.CurrentMessage;
			this.CurrentState = ps.CurrentState;
			this.IsBroken = ps.IsBroken;
			this.IsSuccessful = ps.IsSuccessful;
			this.Name = ps.Name;
			this.ProjectActivity = ps.ProjectActivity.ToString();
			this.ServerName = ps.ServerName;
			this.LastBuildTime = ps.LastBuildTime;

			this.ServerNameVisible = vs.ShowServerName;

			ResolveBreaker(imageBuster, nameBuster);
		}

		/// <summary>
		/// Resolve build-breaker name + photo. This logic previously lived in the
		/// DI-injected OneBreakerConverter/ImagePathConverter/ImageSizeConverter (WPF).
		/// In Avalonia converters are stateless, so it is computed here in the view-model.
		/// </summary>
		void ResolveBreaker(IBuildBuster imageBuster, IBuildBuster nameBuster)
		{
			if (CurrentMessage.IsEmpty()) return;

			if (nameBuster != null)
			{
				var breaker = nameBuster.FindBreaker(CurrentMessage);
				BreakerLabel = breaker.IsEmpty() ? string.Empty : $"({breaker})";
			}

			BreakerImageSize = 15;

			if (imageBuster != null)
			{
				// cached + reused across polls so we don't leak native bitmap memory (see BreakerImageCache)
				BreakerImage = BreakerImageCache.Get(imageBuster.FindBreaker(CurrentMessage));
			}
		}

		#region ProjectStatusProps

		public string Name
		{
			get => _name;
			set
			{
				if (_name == value) return;
				_name = value;
				Notify("Name");
			}
		}

		public string CurrentState
		{
			get => _currentState;
			set
			{
				if (_currentState == value) return;
				_currentState = value;
				Notify("CurrentState");
			}
		}

		public string CurrentMessage
		{
			get => _CurrentMessage;
			set
			{
				if (_CurrentMessage == value) return;
				_CurrentMessage = value;
				Notify("CurrentMessage");
			}
		}

		public string ProjectActivity
		{
			get => _ProjectActivity;
			set
			{
				if (_ProjectActivity == value) return;
				_ProjectActivity = value;
				Notify("ProjectActivity");
			}
		}

		public bool IsBroken
		{
			get => _isBroken;
			set
			{
				if (_isBroken == value) return;
				_isBroken = value;
				Notify("IsBroken");
			}
		}

		public bool IsSuccessful
		{
			get => _isSuccessful;
			set
			{
				if (_isSuccessful == value) return;
				_isSuccessful = value;
				Notify("IsSuccessful");
			}
		}

		public string ServerName
		{
			get => _serverName;
			set
			{
				if (_serverName == value) return;
				_serverName = value;
				Notify("ServerName");
			}
		}

		public DateTime LastBuildTime
		{
			get => _lastBuildTime;
			set
			{
				if (_lastBuildTime == value) return;
				_lastBuildTime = value;
				Notify("LastBuildTime");
			}
		}

		#endregion

		#region Breaker (photo skin)

		/// <summary> breaker username in brackets, eg "(bsimpson)", or empty </summary>
		public string BreakerLabel
		{
			get => _breakerLabel;
			set
			{
				if (_breakerLabel == value) return;
				_breakerLabel = value;
				Notify("BreakerLabel");
			}
		}

		/// <summary> breaker photo (images/[username].jpg) or null if none </summary>
		public Bitmap BreakerImage
		{
			get => _breakerImage;
			set
			{
				if (_breakerImage == value) return;
				_breakerImage = value;
				Notify("BreakerImage");
			}
		}

		public double BreakerImageSize
		{
			get => _breakerImageSize;
			set
			{
				if (_breakerImageSize == value) return;
				_breakerImageSize = value;
				Notify("BreakerImageSize");
			}
		}

		#endregion

		#region SettingProps

		public bool ServerNameVisible
		{
			get => _serverNameVisible;
			set
			{
				if (_serverNameVisible == value) return;
				_serverNameVisible = value;
				Notify("ServerNameVisible");
			}
		}

		#endregion
	}
}
