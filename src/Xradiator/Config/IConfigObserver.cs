namespace Xradiator.Config
{
	public interface IConfigObserver
	{
		void ConfigUpdated(ConfigSettings newSettings);
	}
}