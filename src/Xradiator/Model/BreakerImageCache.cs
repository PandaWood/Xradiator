using System;
using System.Collections.Concurrent;
using System.IO;
using Avalonia.Media.Imaging;
using log4net;

namespace Xradiator.Model
{
	/// <summary>
	/// Caches breaker photos by file path for the lifetime of the app.
	///
	/// Why: a new <see cref="ViewDataViewModel"/> (and a fresh set of
	/// ProjectStatusViewModels) is built on every poll. An Avalonia <see cref="Bitmap"/>
	/// wraps native (Skia) memory and is not disposed when the view-model is replaced, so
	/// allocating one per project per poll leaks native memory until GC finalization.
	/// Breaker photos are keyed by username, i.e. a small fixed set, so we load each file
	/// once and reuse it — bounded memory, and no re-decoding the JPEG every interval.
	/// </summary>
	public static class BreakerImageCache
	{
		static readonly ILog _log = LogManager.GetLogger(nameof(BreakerImageCache));
		static readonly ConcurrentDictionary<string, Bitmap> _cache = new ConcurrentDictionary<string, Bitmap>();

		public static Bitmap Get(string imagePath)
		{
			if (string.IsNullOrEmpty(imagePath)) return null;

			if (_cache.TryGetValue(imagePath, out var cached)) return cached;

			if (!File.Exists(imagePath)) return null;

			try
			{
				var bitmap = new Bitmap(imagePath);
				_cache[imagePath] = bitmap;   // only successful loads are cached
				return bitmap;
			}
			catch (Exception ex)
			{
				_log.DebugFormat("Could not load breaker image '{0}': {1}", imagePath, ex.Message);
				return null;
			}
		}
	}
}
