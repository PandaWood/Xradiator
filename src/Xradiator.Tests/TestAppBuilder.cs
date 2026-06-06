using Avalonia;
using Avalonia.Headless;
using Xradiator.Tests;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace Xradiator.Tests
{
	public class TestAppBuilder
	{
		public static AppBuilder BuildAvaloniaApp() => AppBuilder
			.Configure<Xradiator.App.App>()
			.UseSkia()
			.UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
	}
}
