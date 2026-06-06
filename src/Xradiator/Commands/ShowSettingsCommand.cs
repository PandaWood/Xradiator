using System;
using System.Windows.Input;
using Xradiator.Views;

namespace Xradiator.Commands
{
	public class ShowSettingsCommand : ICommand
	{
		readonly IXradiatorView _view;
		readonly ISettingsWindow _settingsWindow;

		public ShowSettingsCommand(IXradiatorView view, ISettingsWindow settingsWindow)
		{
			_view = view;
			_settingsWindow = settingsWindow;
		}

		public bool CanExecute(object parameter)
		{
			return _view != null;
		}

		public event EventHandler CanExecuteChanged;

		public void Execute(object parameter)
		{
			_settingsWindow.ShowSettings();
		}
	}
}
