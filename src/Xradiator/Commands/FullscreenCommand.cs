using System;
using System.Windows.Input;
using Xradiator.Views;

namespace Xradiator.Commands
{
	public class FullscreenCommand : ICommand
	{
		readonly IXradiatorView _view;

		public FullscreenCommand(IXradiatorView view)
		{
			_view = view;
		}

		public bool CanExecute(object parameter)
		{
			return _view != null && _view.Window != null;
		}

		public event EventHandler CanExecuteChanged;

		public void Execute(object parameter)
		{
			_view.ToggleFullScreen();
		}
	}
}
