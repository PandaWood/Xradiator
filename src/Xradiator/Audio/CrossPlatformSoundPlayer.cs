using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using log4net;

namespace Xradiator.Audio
{
	/// <summary>
	/// Plays a .wav file by shelling out to the platform's native audio player,
	/// so we avoid the Windows-only System.Media.SoundPlayer. Fire-and-forget; never blocks the UI.
	/// </summary>
	public static class CrossPlatformSoundPlayer
	{
		static readonly ILog _log = LogManager.GetLogger(nameof(CrossPlatformSoundPlayer));

		public static void Play(string wavFilePath)
		{
			try
			{
				var psi = BuildStartInfo(wavFilePath);
				if (psi == null)
				{
					_log.Warn("No supported audio backend found for this platform");
					return;
				}
				Process.Start(psi);
			}
			catch (Exception ex)
			{
				_log.ErrorFormat("Failed to play sound '{0}': {1}", wavFilePath, ex);
			}
		}

		static ProcessStartInfo BuildStartInfo(string wav)
		{
			ProcessStartInfo Make(string file, string args) => new ProcessStartInfo
			{
				FileName = file,
				Arguments = args,
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
			};

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				// PlaySync inside a short-lived hidden PowerShell process
				var script = $"(New-Object Media.SoundPlayer '{wav}').PlaySync();";
				return Make("powershell", $"-NoProfile -WindowStyle Hidden -Command \"{script}\"");
			}

			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				return Make("afplay", $"\"{wav}\"");
			}

			// Linux / *nix: try the first player that exists on PATH
			foreach (var (player, args) in new[]
			{
				("paplay", $"\"{wav}\""),
				("aplay", $"-q \"{wav}\""),
				("ffplay", $"-nodisp -autoexit -loglevel quiet \"{wav}\""),
			})
			{
				if (ExistsOnPath(player)) return Make(player, args);
			}

			return null;
		}

		static bool ExistsOnPath(string command)
		{
			try
			{
				using var p = Process.Start(new ProcessStartInfo
				{
					FileName = "which",
					Arguments = command,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
				});
				p.WaitForExit(2000);
				return p.ExitCode == 0;
			}
			catch
			{
				return false;
			}
		}
	}
}
