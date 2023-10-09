using System.Diagnostics;
using System.Security.Principal;

using Microsoft.Win32;

namespace FlipFlopWheel;

internal static class Program
{
	private static void Main(string[] args)
	{
		const string hidPath = @"SYSTEM\CurrentControlSet\Enum\HID";
		const string flipFlopWheel = "FlipFlopWheel";
		if (IsAdministrator())
		{
			SearchForAndSetValue(Registry.LocalMachine, hidPath, flipFlopWheel, 1);
		}
		else
		{
			Console.WriteLine($"{flipFlopWheel} must run as Administrator!");
		}
	}

	private static bool IsAdministrator()
	{
		var identity = WindowsIdentity.GetCurrent();
		var principal = new WindowsPrincipal(identity);
		return principal.IsInRole(WindowsBuiltInRole.Administrator);
	}

	private static void SearchForAndSetValue(RegistryKey baseKey, string path, string target, int newValue)
	{
		using RegistryKey? key = baseKey.OpenSubKey(path, writable: true);
		if (key is not null)
		{
			if (key.GetValue(target) is int value && value == 0)
			{
				key.SetValue(target, newValue);
				Console.WriteLine($@"{key.Name}\{target}: changed from {value} to {newValue}");
			}

			foreach (string subKeyName in key.GetSubKeyNames())
			{
				try
				{
					SearchForAndSetValue(key, subKeyName, target, newValue);
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"Error processing {subKeyName}: {ex.Message}");
				}
			}
		}
	}
}
