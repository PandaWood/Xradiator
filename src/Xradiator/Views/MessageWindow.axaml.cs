using System;
using Avalonia.Threading;

namespace Xradiator.Views
{
	public partial class MessageWindow : Avalonia.Controls.Window
	{
		const double PercentageOfPollFrequency = 1.2;

		readonly IXradiatorView _mainView;
		DispatcherTimer _timer;

		public MessageWindow()
		{
			InitializeComponent();
		}

		public MessageWindow(IXradiatorView view) : this()
		{
			_mainView = view;
		}

		/// <summary>
		/// Standalone error message (used when bootstrap fails and there is no main view).
		/// Closes itself after a few seconds.
		/// </summary>
		public static MessageWindow CreateStandalone(string message)
		{
			var window = new MessageWindow();
			window.Opened += (s, e) => window.ShowMessage(5, message);
			return window;
		}

		public void ShowMessage(int pollFrequency, string message)
		{
			Message.Text = message;
			Opacity = 0.85;

			// auto-close before the next screen update (delay = ~80% of pollFrequency)
			_timer = new DispatcherTimer
			{
				Interval = TimeSpan.FromSeconds(pollFrequency / PercentageOfPollFrequency)
			};
			_timer.Tick += Timer_Tick;
			_timer.Start();

			if (_mainView != null)
			{
				_mainView.ScreenUpdating += MainScreenUpdating;
				_mainView.Closed += MainViewClosed;
			}

			if (!IsVisible) Show();
		}

		void MainViewClosed(object sender, EventArgs e) => SafeClose();

		// if the main screen updated abruptly/manually then force a close
		void MainScreenUpdating(object sender, EventArgs e)
		{
			_timer?.Stop();
			SafeClose();
		}

		void Timer_Tick(object sender, EventArgs e)
		{
			_timer.Stop();
			SafeClose();
		}

		void SafeClose()
		{
			if (Dispatcher.UIThread.CheckAccess()) Close();
			else Dispatcher.UIThread.Post(Close);
		}

		protected override void OnClosed(EventArgs e)
		{
			if (_mainView != null)
			{
				_mainView.ScreenUpdating -= MainScreenUpdating;
				_mainView.Closed -= MainViewClosed;
			}
			if (_timer != null) _timer.Tick -= Timer_Tick;

			base.OnClosed(e);
		}
	}
}
