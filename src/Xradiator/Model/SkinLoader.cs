using Avalonia;
using Avalonia.Controls;
using Xradiator.Views;

namespace Xradiator.Model
{
	public interface ISkinLoader
	{
		void Load(Skin newSkin);
	}

	public class SkinLoader : ISkinLoader
	{
		readonly IXradiatorView _view;
		readonly SkinResourceLoader _skinResourceLoader;
		ResourceDictionary _current;

		public SkinLoader(IXradiatorView view, SkinResourceLoader skinResourceLoader)
		{
			_view = view;
			_skinResourceLoader = skinResourceLoader;
		}

		public void Load(Skin newSkin)
		{
			_view.Invoke(() =>
			{
				newSkin.Resource = _skinResourceLoader.LoadOrGet(newSkin);

				var appResources = Application.Current.Resources.MergedDictionaries;

				if (_current != null)
					appResources.Remove(_current);

				appResources.Add(newSkin.Resource);
				_current = newSkin.Resource;
			});
		}
	}
}
