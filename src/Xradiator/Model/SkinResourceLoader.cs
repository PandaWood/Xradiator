using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Xradiator.Model
{
	public class SkinResourceLoader
	{
		readonly IDictionary<string, ResourceDictionary> _resource = new Dictionary<string, ResourceDictionary>();

		public ResourceDictionary LoadOrGet(Skin skin)
		{
			if (!_resource.ContainsKey(skin.Name))
				_resource[skin.Name] = (ResourceDictionary)AvaloniaXamlLoader.Load(skin.ToUri);

			return _resource[skin.Name];
		}
	}
}
