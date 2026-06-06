using System;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using Xradiator.Config;
using log4net;

namespace Xradiator.Views
{
	public partial class XradiatorWindow : Window, IXradiatorView, IConfigObserver
	{
		static readonly ILog _log = LogManager.GetLogger(nameof(XradiatorWindow));

		int _pollFrequency;
		bool _isShowProgressConfigured;
		WindowState _previousState = WindowState.Normal;

		// designer-only
		public XradiatorWindow()
		{
			InitializeComponent();
		}

		public XradiatorWindow(IConfigSettings configSettings)
		{
			try
			{
				InitializeComponent();
			}
			catch (Exception exception)
			{
				_log.Error(exception);  // usually xaml issues
				throw;
			}

			_pollFrequency = configSettings.PollFrequency;
			_isShowProgressConfigured = configSettings.ShowProgress;
			configSettings.AddObserver(this);
		}

		public new XradiatorPresenter Presenter { get; set; }

		bool IXradiatorView.ShowProgress
		{
			set
			{
				Invoke(() => progressBar.IsVisible = value && _isShowProgressConfigured);
			}
		}

		object IXradiatorView.DataContext
		{
			set { Invoke(() => DataContext = value); }
		}

		Window IXradiatorView.Window => this;

		public void AddKeyBinding(KeyGesture gesture, ICommand command)
		{
			KeyBindings.Add(new KeyBinding { Gesture = gesture, Command = command });
		}

		public void BindSettingsClick(ICommand command)
		{
			settingsLink.PointerPressed += (s, e) =>
			{
				if (command.CanExecute(null)) command.Execute(null);
			};
		}

		public void BindFullscreenDoubleClick(ICommand command)
		{
			DoubleTapped += (s, e) =>
			{
				if (command.CanExecute(null)) command.Execute(null);
			};
		}

		public void ToggleFullScreen()
		{
			Invoke(() =>
			{
				if (WindowState == WindowState.FullScreen)
				{
					SystemDecorations = SystemDecorations.Full;
					Topmost = false;
					WindowState = _previousState;
				}
				else
				{
					_previousState = WindowState == WindowState.Minimized ? WindowState.Normal : WindowState;
					SystemDecorations = SystemDecorations.None;
					Topmost = true;
					WindowState = WindowState.FullScreen;
				}
			});
		}

		public event EventHandler ScreenUpdating;

		void IXradiatorView.UpdateScreen()
		{
			ScreenUpdating?.Invoke(this, EventArgs.Empty);
			Presenter?.UpdateScreen();
		}

		void IXradiatorView.ShowMessage(string message)
		{
			Invoke(() =>
			{
				var messageWindow = new MessageWindow(this);
				messageWindow.ShowMessage(_pollFrequency, message);
			});
		}

		void IXradiatorView.UpdateCountdownTimer(TimeSpan timeRemaining)
		{
			Invoke(() => countdownText.Text = string.Format("{0:00}:{1:00}",
				timeRemaining.Minutes, timeRemaining.Seconds));
		}

		public void Invoke(Action action)
		{
			if (Dispatcher.UIThread.CheckAccess()) action();
			else Dispatcher.UIThread.Post(action);
		}

		void IXradiatorView.ShowCountdown(bool show)
		{
			Invoke(() => countdownText.IsVisible = show);
		}

		public void ConfigUpdated(ConfigSettings newSettings)
		{
			_isShowProgressConfigured = newSettings.ShowProgress;
			_pollFrequency = newSettings.PollFrequency;
		}
	}
}
