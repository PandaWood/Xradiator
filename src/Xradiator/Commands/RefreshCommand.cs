using System;
using System.Windows.Input;
using Xradiator.Views;

namespace Xradiator.Commands
{
	public class RefreshCommand : ICommand
	{
		readonly IXradiatorView _view;

		public RefreshCommand(IXradiatorView view)
		{
			_view = view;
		}

		public bool CanExecute(object parameter)
		{
			return _view != null;
		}

		public event EventHandler CanExecuteChanged;

		public void Execute(object parameter)
		{
			_view.UpdateScreen();
		}
	}
}
