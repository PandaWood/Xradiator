using Avalonia.Input;
using Xradiator.Views;

namespace Xradiator.Commands
{
	public class InputBindingAdder
	{
		readonly IXradiatorView _view;
		readonly CommandContainer _commandContainer;

		public InputBindingAdder(IXradiatorView view, CommandContainer commandContainer)
		{
			_view = view;
			_commandContainer = commandContainer;
		}

		public void AddBindings()
		{
			_view.AddKeyBinding(new KeyGesture(Key.F5), _commandContainer.RefreshCommand);
			_view.AddKeyBinding(new KeyGesture(Key.F11), _commandContainer.FullscreenCommand);
			_view.AddKeyBinding(new KeyGesture(Key.F12), _commandContainer.ShowSettingsCommand);

			_view.BindSettingsClick(_commandContainer.ShowSettingsCommand);
			_view.BindFullscreenDoubleClick(_commandContainer.FullscreenCommand);
		}
	}
}
