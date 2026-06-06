using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.NUnit;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Xradiator.Config;
using Xradiator.Model;
using Xradiator.ViewModels;
using Xradiator.Views;
using NUnit.Framework;
using Shouldly;

namespace Xradiator.Tests
{
	[TestFixture]
	public class RenderSmoke_Tests
	{
		const string Xml =
			@"<Projects>
				<Project name='GreenProj' activity='Sleeping' lastBuildStatus='Success' />
				<Project name='RedProj'   activity='Sleeping' lastBuildStatus='Failure' CurrentMessage='Breakers: alice' />
				<Project name='BusyProj'  activity='Building'  lastBuildStatus='Success' />
			</Projects>";

		[AvaloniaTest]
		public void Stack_skin_renders_a_project_wall_from_dummy_data()
		{
			// arrange: transform sample CCNet xml into the view-model the window binds to
			var config = new ConfigSettings();
			var transformer = new BuildDataTransformer(config);
			var projects = transformer.Transform(Xml).ToList();
			projects.Count.ShouldBe(3);

			var vm = new ViewDataViewModel(config, projects);
			vm.ShowProjects.ShouldBeTrue();

			// load the Stack skin the same way SkinLoader does at runtime
			var skin = (ResourceDictionary)AvaloniaXamlLoader.Load(
				new Uri("avares://Xradiator/Skins/StackSkin.axaml"));
			Application.Current.Resources.MergedDictionaries.Add(skin);

			// act: show the window with that data
			var window = new XradiatorWindow(config) { DataContext = vm };
			window.Width = 600;
			window.Height = 360;
			window.Show();
			Dispatcher.UIThread.RunJobs();

			// save a real screenshot so the rendered wall can be eyeballed
			using (var frame = window.CaptureRenderedFrame())
			{
				var outPath = Path.Combine(AppContext.BaseDirectory, "xradiator-stack-skin.png");
				frame?.Save(outPath);
				TestContext.Out.WriteLine($"screenshot: {outPath}");
			}

			// assert: the skin DataTemplate + bindings produced text blocks for each project
			var texts = window.GetVisualDescendants()
				.OfType<TextBlock>()
				.Select(t => t.Text ?? string.Empty)
				.ToList();

			texts.ShouldContain(t => t.Contains("GreenProj"));
			texts.ShouldContain(t => t.Contains("RedProj"));
			texts.ShouldContain(t => t.Contains("BusyProj"));
		}

		[AvaloniaTest]
		public void Breaker_image_is_cached_and_reused_across_polls()
		{
			// write a real, decodable png to disk
			var path = Path.Combine(Path.GetTempPath(), "xradiator-breaker-cache-test.png");
			using (var rtb = new RenderTargetBitmap(new PixelSize(4, 4)))
			{
				rtb.Save(path);
			}

			var first = BreakerImageCache.Get(path);
			var second = BreakerImageCache.Get(path);

			first.ShouldNotBeNull();
			// same instance reused -> no per-poll native bitmap allocation -> no leak
			ReferenceEquals(first, second).ShouldBeTrue();
		}

		[AvaloniaTest]
		public void All_ok_view_shows_smiley_screen_when_only_broken_and_all_pass()
		{
			var config = new ConfigSettings { ShowOnlyBroken = true, ViewName = "team" };
			var transformer = new BuildDataTransformer(config);
			var projects = transformer.Transform(
				"<Projects><Project name='OK' activity='Sleeping' lastBuildStatus='Success'/></Projects>").ToList();

			var vm = new ViewDataViewModel(config, projects);

			vm.ShowAllOK.ShouldBeTrue();
			vm.ShowProjects.ShouldBeFalse();
		}
	}
}
