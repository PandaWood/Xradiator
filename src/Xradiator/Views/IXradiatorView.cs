using System;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Input;

namespace Xradiator.Views
{
	public interface IXradiatorView
	{
		void ShowMessage(string message);
		object DataContext { set; }
		void UpdateCountdownTimer(TimeSpan timeRemaining);
		void Invoke(Action action);
		void ShowCountdown(bool show);
		Window Window { get; }
		XradiatorPresenter Presenter { set; }
		bool ShowProgress { set; }
		void UpdateScreen();

		// input wiring (replaces WPF InputBindings/MouseBindings)
		void AddKeyBinding(KeyGesture gesture, ICommand command);
		void BindSettingsClick(ICommand command);
		void BindFullscreenDoubleClick(ICommand command);

		void ToggleFullScreen();

		event EventHandler ScreenUpdating;
		event EventHandler Activated;
		event EventHandler Closed;
	}
}
