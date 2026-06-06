using System;
using Avalonia.Controls;

namespace Xradiator.Model
{
	public class Skin
	{
		readonly string _name;

		public Skin(string skinName)
		{
			_name = skinName;
		}

		public string Name => _name;

		public ResourceDictionary Resource { get; set; }

		public Uri ToUri => new Uri($"avares://Xradiator/Skins/{Name}Skin.axaml");
	}
}
