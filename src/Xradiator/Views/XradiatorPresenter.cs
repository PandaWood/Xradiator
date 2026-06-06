using Xradiator.Commands;
using Xradiator.Config;
using Xradiator.Config.ChangeHandlers;
using Xradiator.Model;

namespace Xradiator.Views
{
	public class XradiatorPresenter : IConfigObserver
	{
		readonly IXradiatorView _view;
		readonly IConfigSettings _configSettings;
		readonly ConfigChangeHandlerFarm _changeHandlerFarm;
		readonly ISkinLoader _skinLoader;
		readonly IScreenUpdater _screenUpdater;
		readonly IConfigFileWatcher _configFileWatcher;

		public XradiatorPresenter(IXradiatorView view, IConfigSettings configSettings,
								  IConfigFileWatcher configFileWatcher, ISkinLoader skinLoader,
								  ConfigChangeHandlerFarm changeHandlerFarm, IScreenUpdater screenUpdater,
								  InputBindingAdder inputBindingAdder)
		{
			_view = view;
			view.Presenter = this;
			_configSettings = configSettings;
			_configFileWatcher = configFileWatcher;
			_screenUpdater = screenUpdater;
			_skinLoader = skinLoader;
			_changeHandlerFarm = changeHandlerFarm;

			inputBindingAdder.AddBindings();
			configSettings.AddObserver(this);
		}

		public void Init()
		{
			_view.ShowCountdown(_configSettings.ShowCountdown);
			_skinLoader.Load(new Skin(_configSettings.SkinName));
			UpdateScreen();
			_configFileWatcher.Start();
		}

		public void UpdateScreen()
		{
			_screenUpdater.Update();
		}

		public void ConfigUpdated(ConfigSettings newSettings)
		{
			_changeHandlerFarm.UpdateAll(newSettings);
		}
	}
}