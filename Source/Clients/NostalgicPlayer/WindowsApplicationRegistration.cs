/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer
{
	/// <summary>
	/// Registers the unpackaged application with the Windows shell
	/// </summary>
	internal static class WindowsApplicationRegistration
	{
		private const string AppUserModelId = "Polycode.NostalgicPlayer";
		private const int AppModelErrorNoPackage = 15700;
		private const int ShowNormal = 1;
		private const uint ShellChangeNotifyCreate = 0x00000002;
		private const uint ShellChangeNotifyUpdateItem = 0x00002000;
		private const uint ShellChangeNotifyPathW = 0x00000005;
		private const uint ShellChangeNotifyFlushNoWait = 0x00002000;

		private static readonly PropertyKey AppUserModelIdProperty = new PropertyKey(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 5);

		/********************************************************************/
		/// <summary>
		/// Register the application with the Windows shell
		/// </summary>
		/********************************************************************/
		public static void Register()
		{
			try
			{
				// An installed package already has an application identity and
				// a Start Menu entry supplied by its manifest
				if (IsPackaged())
					return;

				Marshal.ThrowExceptionForHR(SetCurrentProcessExplicitAppUserModelID(AppUserModelId));

				string executablePath = Environment.ProcessPath;
				string programsPath = Environment.GetFolderPath(Environment.SpecialFolder.Programs);

				if (string.IsNullOrEmpty(executablePath) || string.IsNullOrEmpty(programsPath))
					return;

				string shortcutPath = Path.Combine(programsPath, "NostalgicPlayer.lnk");
				EnsureStartMenuShortcut(shortcutPath, executablePath);
			}
			catch (Exception)
			{
				// Shell registration is optional and must never prevent the
				// application from starting
			}
		}



		/********************************************************************/
		/// <summary>
		/// Make sure the Start Menu shortcut has the expected target and
		/// application identity
		/// </summary>
		/********************************************************************/
		private static void EnsureStartMenuShortcut(string shortcutPath, string executablePath)
		{
			bool shortcutExists = File.Exists(shortcutPath);
			if (shortcutExists && IsShortcutCurrent(shortcutPath, executablePath))
				return;

			object shellLinkObject = new ShellLink();

			try
			{
				IShellLinkW shellLink = (IShellLinkW)shellLinkObject;
				shellLink.SetPath(executablePath);
				shellLink.SetWorkingDirectory(AppContext.BaseDirectory);
				shellLink.SetDescription("NostalgicPlayer");
				shellLink.SetIconLocation(executablePath, 0);
				shellLink.SetShowCmd(ShowNormal);

				PropertyKey propertyKey = AppUserModelIdProperty;
				PropVariant appUserModelId = PropVariant.FromString(AppUserModelId);

				try
				{
					IPropertyStore propertyStore = (IPropertyStore)shellLinkObject;
					propertyStore.SetValue(ref propertyKey, ref appUserModelId);
					propertyStore.Commit();
				}
				finally
				{
					PropVariantClear(ref appUserModelId);
				}

				((IPersistFile)shellLinkObject).Save(shortcutPath, true);
			}
			finally
			{
				Marshal.FinalReleaseComObject(shellLinkObject);
			}

			SHChangeNotify(shortcutExists ? ShellChangeNotifyUpdateItem : ShellChangeNotifyCreate, ShellChangeNotifyPathW | ShellChangeNotifyFlushNoWait, shortcutPath, IntPtr.Zero);
		}



		/********************************************************************/
		/// <summary>
		/// Check if the existing shortcut points to this executable and has
		/// the expected application identity
		/// </summary>
		/********************************************************************/
		private static bool IsShortcutCurrent(string shortcutPath, string executablePath)
		{
			object shellLinkObject = new ShellLink();

			try
			{
				((IPersistFile)shellLinkObject).Load(shortcutPath, 0);

				StringBuilder targetPath = new StringBuilder(32768);
				((IShellLinkW)shellLinkObject).GetPath(targetPath, targetPath.Capacity, IntPtr.Zero, 0);

				PropertyKey propertyKey = AppUserModelIdProperty;
				((IPropertyStore)shellLinkObject).GetValue(ref propertyKey, out PropVariant appUserModelId);

				try
				{
					return string.Equals(targetPath.ToString(), executablePath, StringComparison.OrdinalIgnoreCase) &&
						string.Equals(appUserModelId.GetString(), AppUserModelId, StringComparison.Ordinal);
				}
				finally
				{
					PropVariantClear(ref appUserModelId);
				}
			}
			catch (Exception)
			{
				return false;
			}
			finally
			{
				Marshal.FinalReleaseComObject(shellLinkObject);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return whether the current process has package identity
		/// </summary>
		/********************************************************************/
		private static bool IsPackaged()
		{
			int packageFullNameLength = 0;
			int result = GetCurrentPackageFullName(ref packageFullNameLength, IntPtr.Zero);

			return result != AppModelErrorNoPackage;
		}

		[ComImport]
		[Guid("00021401-0000-0000-C000-000000000046")]
		private class ShellLink
		{
		}

		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		private readonly struct PropertyKey
		{
			private readonly Guid formatId;
			private readonly uint propertyId;

			public PropertyKey(Guid formatId, uint propertyId)
			{
				this.formatId = formatId;
				this.propertyId = propertyId;
			}
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct PropVariant
		{
			private const ushort StringValueType = 31;

			[FieldOffset(0)]
			private ushort valueType;

			[FieldOffset(8)]
			private IntPtr value;

			public static PropVariant FromString(string value)
			{
				return new PropVariant
				{
					valueType = StringValueType,
					value = Marshal.StringToCoTaskMemUni(value)
				};
			}

			public readonly string GetString()
			{
				return valueType == StringValueType ? Marshal.PtrToStringUni(value) : null;
			}
		}

		[ComImport]
		[Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		private interface IPropertyStore
		{
			void GetCount(out uint propertyCount);
			void GetAt(uint propertyIndex, out PropertyKey propertyKey);
			void GetValue(ref PropertyKey propertyKey, out PropVariant value);
			void SetValue(ref PropertyKey propertyKey, ref PropVariant value);
			void Commit();
		}

		[ComImport]
		[Guid("000214F9-0000-0000-C000-000000000046")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		private interface IShellLinkW
		{
			void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder file, int maximumPath, IntPtr findData, uint flags);
			void GetIdList(out IntPtr itemIdList);
			void SetIdList(IntPtr itemIdList);
			void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder name, int maximumName);
			void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string name);
			void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder directory, int maximumPath);
			void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string directory);
			void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder arguments, int maximumPath);
			void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string arguments);
			void GetHotKey(out short hotKey);
			void SetHotKey(short hotKey);
			void GetShowCmd(out int showCommand);
			void SetShowCmd(int showCommand);
			void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder iconPath, int iconPathLength, out int iconIndex);
			void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string iconPath, int iconIndex);
			void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string path, uint reserved);
			void Resolve(IntPtr windowHandle, uint flags);
			void SetPath([MarshalAs(UnmanagedType.LPWStr)] string file);
		}

		[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
		private static extern int SetCurrentProcessExplicitAppUserModelID(string appId);

		[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
		private static extern void SHChangeNotify(uint eventId, uint flags, string item1, IntPtr item2);

		[DllImport("ole32.dll")]
		private static extern int PropVariantClear(ref PropVariant propVariant);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		private static extern int GetCurrentPackageFullName(ref int packageFullNameLength, IntPtr packageFullName);
	}
}
