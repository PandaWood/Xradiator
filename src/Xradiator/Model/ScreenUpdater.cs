using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xradiator.Audio;
using Xradiator.Config;
using Xradiator.ViewModels;
using Xradiator.Views;
using log4net;

namespace Xradiator.Model
{
	public interface IScreenUpdater :  IConfigObserver
	{
		void Update();
	}

	public class ScreenUpdater : IScreenUpdater
	{
		static readonly ILog _log = LogManager.GetLogger(nameof(ScreenUpdater));

		readonly IXradiatorView _view;
		readonly DiscJockey _discJockey;
		readonly ICountdownTimer _countdownTimer;
		readonly IPollTimer _pollTimer;
		private readonly IConfigSettings _configSettings;
		readonly BuildDataFetcher _fetcher;
		readonly BuildDataTransformer _transformer;
		readonly FetchExceptionHandler _fetchExceptionHandler;
		readonly IBuildBuster _imageBuster;
		readonly IBuildBuster _nameBuster;
		readonly BackgroundWorker _worker;
		private IViewSettings _viewSettings;

		public ScreenUpdater(IXradiatorView view, DiscJockey discJockey, ICountdownTimer countdownTimer,
							 IPollTimer pollTimer, IConfigSettings configSettings,
							 BuildDataFetcher buildDataFetcher, BuildDataTransformer transformer,
							 FetchExceptionHandler fetchExceptionHandler,
							 [InjectBuildBusterImageDecorator] IBuildBuster imageBuster,
							 [InjectBuildBuster] IBuildBuster nameBuster,
							 BackgroundWorker worker)
		{
			_view = view;
			_discJockey = discJockey;
			_countdownTimer = countdownTimer;
			_pollTimer = pollTimer;
			_configSettings = configSettings;
			_pollTimer.Tick = (sender, e) => PollTimeup();
			_fetcher = buildDataFetcher;
			_fetchExceptionHandler = fetchExceptionHandler;
			_transformer = transformer;
			_imageBuster = imageBuster;
			_nameBuster = nameBuster;

			SetLocalValuesFromConfig(configSettings);

			_configSettings.AddObserver(this);

			_worker = worker;
			worker.DoWork += FetchData;
			worker.RunWorkerCompleted += DataFetched;
		}

		private void PollTimeup()
		{
			_configSettings.RotateView();
			Update();
		}

		public void Update()
		{
			_countdownTimer.Stop();
			_pollTimer.Stop();
			_view.ShowProgress = true;

			_worker.RunWorkerAsync();
		}

		void FetchData(object sender, DoWorkEventArgs e)
		{
			e.Result = _fetcher.Fetch();
		}

		void DataFetched(object sender, RunWorkerCompletedEventArgs e)
		{
			try
			{
				if (e.Error != null)
				{
					_fetchExceptionHandler.Handle(e.Error);
					return;
				}

				var xmlResults = e.Result as IEnumerable<string>;
				IEnumerable<ProjectStatus> projectData = new List<ProjectStatus>();
				if (xmlResults != null)
				{
					projectData = xmlResults.SelectMany(xml =>
					{
						try
						{
							return _transformer.Transform(xml);
						}
						catch (Exception exception)
						{
							_log.Error(exception);
							return new List<ProjectStatus>();
						}
					}).ToList();
				}

				var data = projectData.ToList();
				_log.InfoFormat("Screen updated with {0} project(s) for view '{1}'",
					data.Count, _viewSettings?.ViewName);

				_view.Invoke(() =>
					_view.DataContext = new ViewDataViewModel(_viewSettings, data, _imageBuster, _nameBuster));

				_discJockey.PlaySounds(data);
			}
			finally
			{
				_view.ShowProgress = false;

				_pollTimer.Start();
				_countdownTimer.Reset();
				_countdownTimer.Start();
			}
		}

		public void ConfigUpdated(ConfigSettings newSettings)
		{
			SetLocalValuesFromConfig(newSettings);
		}

		void SetLocalValuesFromConfig(IConfigSettings newSettings)
		{
			_viewSettings = newSettings as IViewSettings;
		}
	}
}
