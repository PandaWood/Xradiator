using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Xradiator.Config;

namespace Xradiator.Views
{
	public interface ISettingsWindow
	{
		void ShowSettings();
	}

	public partial class SettingsWindow : Window, ISettingsWindow
	{
		readonly IConfigSettings _configSettings;
		readonly IXradiatorView _view;

		// designer-only
		public SettingsWindow()
		{
			InitializeComponent();
		}

		public SettingsWindow(IConfigSettings configSettings, IXradiatorView view)
		{
			_configSettings = configSettings;
			_view = view;

			InitializeComponent();

			skinCombo.ItemsSource = new[] { "Stack", "Grid", "StackPhoto" };
			DataContext = _configSettings;

			_view.Closed += (s, e) => Close();
		}

		public void ShowSettings()
		{
			if (IsVisible)
			{
				Activate();
				return;
			}

			var owner = _view?.Window;
			if (owner != null) Show(owner);
			else Show();
		}

		// hide instead of close, so the singleton window can be reused
		protected override void OnClosing(WindowClosingEventArgs e)
		{
			if (!e.IsProgrammatic)
			{
				e.Cancel = true;
				Hide();
			}
			base.OnClosing(e);
		}

		void Save_Click(object sender, RoutedEventArgs e)
		{
			_configSettings.Save();
			Hide();
		}

		void Cancel_Click(object sender, RoutedEventArgs e)
		{
			_configSettings.Load();   // 'Cancel' = reload from config, discarding edits
			Hide();
		}
	}
}
