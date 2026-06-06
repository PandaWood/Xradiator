using System.ComponentModel;
using System.Linq;
using Xradiator.Audio;
using Xradiator.Config;
using Xradiator.Config.ChangeHandlers;
using Xradiator.Model;
using Xradiator.Views;
using Xradiator.Extensions;
using Ninject.Modules;

namespace Xradiator.Services
{
	public class XradiatorNinjaModule : NinjectModule
	{
		readonly IXradiatorView _view;
		readonly IConfigSettings _configSettings;

		public XradiatorNinjaModule(IXradiatorView view, IConfigSettings settings)
		{
			_view = view;
			_configSettings = settings;
		}

		public override void Load()
		{
			Bind<IXradiatorView>().ToConstant(_view);
			Bind<IConfigSettings>().ToConstant(_configSettings);

			// BackgroundWorker is a plain dependency of ScreenUpdater
			Bind<BackgroundWorker>().ToSelf();

			Bind<IWebClientFactory>().To<WebClientFactory>().InSingletonScope();
			Bind<IAudioPlayer>().To<AudioPlayer>().InSingletonScope();
			Bind<ICountdownTimer>().To<CountdownTimer>().InSingletonScope();
			Bind<IPollTimer>().To<PollTimer>().InSingletonScope();
			Bind<ISkinLoader>().To<SkinLoader>().InSingletonScope();
			Bind<IScreenUpdater>().To<ScreenUpdater>().InSingletonScope();
			Bind<ISettingsWindow>().To<SettingsWindow>().InSingletonScope();
			Bind<IAppLocation>().To<AppLocation>().InSingletonScope();

			IConfigLocation configLocation = new ConfigLocation();
			Bind<IConfigLocation>().ToConstant(configLocation).InSingletonScope();
			Bind<IConfigFileWatcher>().ToConstant(new ConfigFileWatcher(_configSettings, configLocation.FileName));

			Bind<IBuildBuster>().To<BuildBuster>()
				.WhenTargetHas<InjectBuildBusterAttribute>().InSingletonScope();

			Bind<IBuildBuster>().To<BuildBusterImageDecorator>()
				.WhenTargetHas<InjectBuildBusterImageDecoratorAttribute>().InSingletonScope();

			Bind<IBuildBuster>().To<BuildBusterFullNameDecorator>()
				.WhenTargetHas<InjectBuildBusterFullNameDecoratorAttribute>().InSingletonScope();

			Bind<XradiatorPresenter>().ToSelf().InSingletonScope();

			BindConfigChangeHandlers();
		}

		private void BindConfigChangeHandlers()
		{
			var changeHandlers = from type in typeof(ConfigChangeHandlerFarm).Assembly.GetExportedTypes()
								 where !type.IsInterface
								 where typeof(IConfigChangeHandler).IsAssignableFrom(type)
								 select type;

			changeHandlers.ForEach(handler => Bind<IConfigChangeHandler>().To(handler).InSingletonScope());
		}
	}
}
