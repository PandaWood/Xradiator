using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Xradiator.Config;
using Xradiator.Services;
using Xradiator.Views;
using log4net;
using Ninject;

namespace Xradiator.App
{
	public partial class App : Application
	{
		static readonly ILog _log = LogManager.GetLogger(nameof(App));

		public override void Initialize()
		{
			ConfigureLogging();
			AvaloniaXamlLoader.Load(this);
		}

		static void ConfigureLogging()
		{
			try
			{
				var configFile = new System.IO.FileInfo(new ConfigLocation().FileName);
				if (configFile.Exists)
					log4net.Config.XmlConfigurator.Configure(configFile);
				else
					log4net.Config.XmlConfigurator.Configure();
			}
			catch
			{
				// logging must never take the app down
			}
		}

		public override void OnFrameworkInitializationCompleted()
		{
			if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			{
				try
				{
					var configSettings = new ConfigSettings();
					configSettings.Load();

					var mainWindow = new XradiatorWindow(configSettings);

					var module = new XradiatorNinjaModule(mainWindow, configSettings);
					var kernel = new StandardKernel(module);
					var presenter = kernel.Get<XradiatorPresenter>();

					desktop.MainWindow = mainWindow;
					// Init (first fetch + skin load + file-watch) once the window is up
					mainWindow.Opened += (s, e) => presenter.Init();
				}
				catch (Exception exception)
				{
					_log.Error(exception.Message, exception);
					desktop.MainWindow = MessageWindow.CreateStandalone(
						"Application Exception - see log for details\nShutting down...");
				}
			}

			base.OnFrameworkInitializationCompleted();
		}
	}
}
