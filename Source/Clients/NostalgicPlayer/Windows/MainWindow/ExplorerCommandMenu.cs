/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.Windows
{
	/// <summary>
	/// COM service provider exposed to hosted Explorer commands
	/// </summary>
	[ComVisible(true)]
	[Guid("6D5140C1-7436-11CE-8034-00AA006009FA")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IExplorerCommandSiteServiceProvider
	{
		/// <summary>
		/// Return a service exposed by the command host
		/// </summary>
		[PreserveSig]
		int QueryService(ref Guid service, ref Guid interfaceId, out IntPtr result);
	}

	/// <summary>
	/// Window handle exposed to hosted Explorer commands
	/// </summary>
	[ComVisible(true)]
	[Guid("00000114-0000-0000-C000-000000000046")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IExplorerCommandSiteOleWindow
	{
		/// <summary>
		/// Return the host window handle
		/// </summary>
		[PreserveSig]
		int GetWindow(out IntPtr windowHandle);

		/// <summary>
		/// Enter or leave context-sensitive help mode
		/// </summary>
		[PreserveSig]
		int ContextSensitiveHelp([MarshalAs(UnmanagedType.Bool)] bool enterMode);
	}

	/// <summary>
	/// UI mode service exposed to hosted Explorer commands
	/// </summary>
	[ComVisible(true)]
	[Guid("4B6832A2-5F04-4C9D-B89D-727A15D103E7")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IExplorerCommandSiteExecuteCommandHost
	{
		/// <summary>
		/// Return the host UI mode
		/// </summary>
		[PreserveSig]
		int GetUiMode(out int uiMode);
	}

	/// <summary>
	/// Hosts an IExplorerCommand and exposes its menu hierarchy
	/// </summary>
	internal sealed class ExplorerCommandMenu : IDisposable
	{
		private const int ClassNotRegistered = unchecked((int)0x80040154);

		private readonly IShellItemArray shellItems;
		private readonly ExplorerCommandSite site;
		private readonly List<IExplorerCommand> commands;
		private readonly List<Entry> entries;

		private bool disposed;

		/********************************************************************/
		/// <summary>
		/// A single command in the Explorer command hierarchy
		/// </summary>
		/********************************************************************/
		internal sealed class Entry
		{
			private readonly ExplorerCommandMenu owner;
			private readonly IExplorerCommand command;
			private Image image;

			public string Text { get; }
			public string ToolTipText { get; }
			public bool Enabled { get; }
			public bool Visible { get; }
			public bool Checked { get; }
			public bool IsSeparator { get; }
			public bool SeparatorBefore { get; }
			public bool SeparatorAfter { get; }
			public IReadOnlyList<Entry> Children { get; }

			/****************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/****************************************************************/
			internal Entry(ExplorerCommandMenu owner, IExplorerCommand command, string text, string toolTipText, Image image, ExplorerCommandState state, ExplorerCommandFlags flags, IReadOnlyList<Entry> children)
			{
				this.owner = owner;
				this.command = command;

				Text = text;
				ToolTipText = toolTipText;
				this.image = image;
				Enabled = (state & ExplorerCommandState.Disabled) == 0;
				Visible = (state & ExplorerCommandState.Hidden) == 0;
				Checked = (state & ExplorerCommandState.Checked) != 0;
				IsSeparator = (flags & ExplorerCommandFlags.IsSeparator) != 0;
				SeparatorBefore = (flags & ExplorerCommandFlags.SeparatorBefore) != 0;
				SeparatorAfter = (flags & ExplorerCommandFlags.SeparatorAfter) != 0;
				Children = children;
			}



			/****************************************************************/
			/// <summary>
			/// Transfer ownership of the command image to the menu item
			/// </summary>
			/****************************************************************/
			public Image TakeImage()
			{
				Image result = image;
				image = null;
				return result;
			}



			/****************************************************************/
			/// <summary>
			/// Invoke the command for the selected files
			/// </summary>
			/****************************************************************/
			public void Invoke()
			{
				owner.ThrowIfDisposed();
				Marshal.ThrowExceptionForHR(command.Invoke(owner.shellItems, null));
			}



			/****************************************************************/
			/// <summary>
			/// Dispose images that have not been transferred to menu items
			/// </summary>
			/****************************************************************/
			internal void DisposeImage()
			{
				image?.Dispose();
				image = null;
			}
		}

		public Entry Root { get; }

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private ExplorerCommandMenu(Guid classId, IntPtr ownerWindow, IReadOnlyList<string> fileNames)
		{
			commands = new List<IExplorerCommand>();
			entries = new List<Entry>();
			site = new ExplorerCommandSite(ownerWindow);
			shellItems = null;

			try
			{
				shellItems = CreateShellItemArray(fileNames);

				Type commandType = Type.GetTypeFromCLSID(classId, true);
				object commandObject = Activator.CreateInstance(commandType);
				IExplorerCommand command = commandObject as IExplorerCommand ?? throw new InvalidCastException();

				Root = CreateEntry(command);
			}
			catch
			{
				ReleaseResources();
				throw;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Try to create the command menu. Null means that the COM package is
		/// not installed
		/// </summary>
		/********************************************************************/
		public static ExplorerCommandMenu TryCreate(Guid classId, IntPtr ownerWindow, IReadOnlyList<string> fileNames)
		{
			try
			{
				return new ExplorerCommandMenu(classId, ownerWindow, fileNames);
			}
			catch (COMException ex) when (ex.HResult == ClassNotRegistered)
			{
				return null;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Release all hosted COM objects
		/// </summary>
		/********************************************************************/
		public void Dispose()
		{
			if (disposed)
				return;

			disposed = true;
			ReleaseResources();
		}



		/********************************************************************/
		/// <summary>
		/// Release images and COM objects created by the command hierarchy
		/// </summary>
		/********************************************************************/
		private void ReleaseResources()
		{
			foreach (Entry entry in entries)
				entry.DisposeImage();

			for (int i = commands.Count - 1; i >= 0; i--)
			{
				IExplorerCommand command = commands[i];

				try
				{
					if (command is IObjectWithSite objectWithSite)
						objectWithSite.SetSite(null);
				}
				catch (COMException)
				{
				}

				Marshal.ReleaseComObject(command);
			}

			commands.Clear();

			if (shellItems != null)
				Marshal.ReleaseComObject(shellItems);
		}



		/********************************************************************/
		/// <summary>
		/// Create a menu entry from an Explorer command
		/// </summary>
		/********************************************************************/
		private Entry CreateEntry(IExplorerCommand command)
		{
			commands.Add(command);
			Image image = null;

			try
			{
				if (command is IObjectWithSite objectWithSite)
					Marshal.ThrowExceptionForHR(objectWithSite.SetSite(site));

				string text = GetCommandString((out IntPtr value) => command.GetTitle(shellItems, out value));
				string toolTipText = GetCommandString((out IntPtr value) => command.GetToolTip(shellItems, out value));
				image = GetCommandImage(GetCommandString((out IntPtr value) => command.GetIcon(shellItems, out value)));

				Marshal.ThrowExceptionForHR(command.GetState(shellItems, true, out ExplorerCommandState state));

				ExplorerCommandFlags flags = ExplorerCommandFlags.Default;
				int result = command.GetFlags(out flags);
				if (result < 0)
					flags = ExplorerCommandFlags.Default;

				List<Entry> children = new List<Entry>();
				if ((flags & ExplorerCommandFlags.HasSubCommands) != 0)
					AddSubCommands(command, children);

				Entry entry = new Entry(this, command, text, toolTipText, image, state, flags, children);
				entries.Add(entry);
				return entry;
			}
			catch
			{
				image?.Dispose();
				throw;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Add all subcommands exposed by the given command
		/// </summary>
		/********************************************************************/
		private void AddSubCommands(IExplorerCommand command, List<Entry> entries)
		{
			int result = command.EnumSubCommands(out IEnumExplorerCommand enumerator);
			if ((result < 0) || (enumerator == null))
				return;

			try
			{
				for (;;)
				{
					result = enumerator.Next(1, out IExplorerCommand childCommand, out uint fetched);
					if ((result != 0) || (fetched == 0))
						break;

					entries.Add(CreateEntry(childCommand));
				}
			}
			finally
			{
				Marshal.ReleaseComObject(enumerator);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Retrieve a string allocated by an Explorer command
		/// </summary>
		/********************************************************************/
		private static string GetCommandString(GetExplorerCommandString getter)
		{
			IntPtr valuePointer = IntPtr.Zero;

			try
			{
				int result = getter(out valuePointer);
				return (result >= 0) && (valuePointer != IntPtr.Zero) ? Marshal.PtrToStringUni(valuePointer) : null;
			}
			finally
			{
				if (valuePointer != IntPtr.Zero)
					Marshal.FreeCoTaskMem(valuePointer);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Create a shell item array containing all selected physical files
		/// </summary>
		/********************************************************************/
		private static IShellItemArray CreateShellItemArray(IReadOnlyList<string> fileNames)
		{
			IntPtr[] itemIdLists = new IntPtr[fileNames.Count];

			try
			{
				for (int i = 0; i < fileNames.Count; i++)
				{
					Marshal.ThrowExceptionForHR(SHParseDisplayName(fileNames[i], null, out itemIdLists[i], 0, out _));
				}

				Marshal.ThrowExceptionForHR(SHCreateShellItemArrayFromIDLists((uint)itemIdLists.Length, itemIdLists, out IShellItemArray shellItems));
				return shellItems;
			}
			finally
			{
				foreach (IntPtr itemIdList in itemIdLists)
				{
					if (itemIdList != IntPtr.Zero)
						Marshal.FreeCoTaskMem(itemIdList);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Load the icon returned by IExplorerCommand
		/// </summary>
		/********************************************************************/
		private static Image GetCommandImage(string iconLocation)
		{
			if (string.IsNullOrEmpty(iconLocation))
				return null;

			int iconIndex = 0;
			int commaIndex = iconLocation.LastIndexOf(',');

			if ((commaIndex >= 0) && int.TryParse(iconLocation.AsSpan(commaIndex + 1).Trim(), out int parsedIndex))
			{
				iconIndex = parsedIndex;
				iconLocation = iconLocation.Substring(0, commaIndex);
			}

			string iconFileName = Environment.ExpandEnvironmentVariables(iconLocation.Trim().Trim('"').TrimStart('@'));
			if (!File.Exists(iconFileName))
				return null;

			IntPtr[] smallIcons = new IntPtr[1];
			if (ExtractIconEx(iconFileName, iconIndex, null, smallIcons, 1) == 0)
				return null;

			try
			{
				using (Icon icon = (Icon)Icon.FromHandle(smallIcons[0]).Clone())
					return icon.ToBitmap();
			}
			finally
			{
				DestroyIcon(smallIcons[0]);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Throw if the hosted command has already been released
		/// </summary>
		/********************************************************************/
		private void ThrowIfDisposed()
		{
			ObjectDisposedException.ThrowIf(disposed, this);
		}

		private delegate int GetExplorerCommandString(out IntPtr value);

		[Flags]
		internal enum ExplorerCommandState : uint
		{
			Enabled = 0,
			Disabled = 0x1,
			Hidden = 0x2,
			CheckBox = 0x4,
			Checked = 0x8,
			RadioCheck = 0x10
		}

		[Flags]
		internal enum ExplorerCommandFlags : uint
		{
			Default = 0,
			HasSubCommands = 0x1,
			HasSplitButton = 0x2,
			HideLabel = 0x4,
			IsSeparator = 0x8,
			HasLuaShield = 0x10,
			SeparatorBefore = 0x20,
			SeparatorAfter = 0x40,
			IsDropDown = 0x80,
			Toggleable = 0x100,
			AutoMenuIcons = 0x200
		}

		[ComImport]
		[Guid("A08CE4D0-FA25-44AB-B57C-C7B1C323E0B9")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		internal interface IExplorerCommand
		{
			[PreserveSig]
			int GetTitle(IShellItemArray shellItems, out IntPtr name);

			[PreserveSig]
			int GetIcon(IShellItemArray shellItems, out IntPtr icon);

			[PreserveSig]
			int GetToolTip(IShellItemArray shellItems, out IntPtr toolTip);

			[PreserveSig]
			int GetCanonicalName(out Guid commandName);

			[PreserveSig]
			int GetState(IShellItemArray shellItems, [MarshalAs(UnmanagedType.Bool)] bool allowSlowOperation, out ExplorerCommandState commandState);

			[PreserveSig]
			int Invoke(IShellItemArray shellItems, IBindCtx bindContext);

			[PreserveSig]
			int GetFlags(out ExplorerCommandFlags flags);

			[PreserveSig]
			int EnumSubCommands(out IEnumExplorerCommand enumerator);
		}

		[ComImport]
		[Guid("A88826F8-186F-4987-AADE-EA0CEF8FBFE8")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		internal interface IEnumExplorerCommand
		{
			[PreserveSig]
			int Next(uint count, out IExplorerCommand command, out uint fetched);

			[PreserveSig]
			int Skip(uint count);

			[PreserveSig]
			int Reset();

			[PreserveSig]
			int Clone(out IEnumExplorerCommand enumerator);
		}

		[ComImport]
		[Guid("FC4801A3-2BA9-11CF-A229-00AA003D7352")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		private interface IObjectWithSite
		{
			[PreserveSig]
			int SetSite([MarshalAs(UnmanagedType.IUnknown)] object site);

			[PreserveSig]
			int GetSite(ref Guid interfaceId, out IntPtr site);
		}

		[ComImport]
		[Guid("B63EA76D-1F85-456F-A19C-48159EFA858B")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		internal interface IShellItemArray
		{
		}

		[ComVisible(true)]
		[ClassInterface(ClassInterfaceType.None)]
		private sealed class ExplorerCommandSite : IExplorerCommandSiteServiceProvider, IExplorerCommandSiteOleWindow, IExplorerCommandSiteExecuteCommandHost
		{
			private const int NoInterface = unchecked((int)0x80004002);

			private static readonly Guid OleWindowInterfaceId = new Guid("00000114-0000-0000-C000-000000000046");
			private static readonly Guid ExecuteCommandHostInterfaceId = new Guid("4B6832A2-5F04-4C9D-B89D-727A15D103E7");

			private readonly IntPtr windowHandle;

			public ExplorerCommandSite(IntPtr windowHandle)
			{
				this.windowHandle = windowHandle;
			}

			public int QueryService(ref Guid service, ref Guid interfaceId, out IntPtr result)
			{
				result = IntPtr.Zero;

				if ((service == ExecuteCommandHostInterfaceId) && (interfaceId == ExecuteCommandHostInterfaceId))
					result = Marshal.GetComInterfaceForObject(this, typeof(IExplorerCommandSiteExecuteCommandHost));
				else if (interfaceId == OleWindowInterfaceId)
					result = Marshal.GetComInterfaceForObject(this, typeof(IExplorerCommandSiteOleWindow));
				else
					return NoInterface;

				return 0;
			}

			public int GetWindow(out IntPtr windowHandle)
			{
				windowHandle = this.windowHandle;
				return 0;
			}

			public int ContextSensitiveHelp(bool enterMode)
			{
				return unchecked((int)0x80004001);
			}

			public int GetUiMode(out int uiMode)
			{
				uiMode = 0;
				return 0;
			}
		}

		[DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = true)]
		private static extern int SHParseDisplayName(string name, IBindCtx bindContext, out IntPtr itemIdList, uint attributes, out uint attributesOut);

		[DllImport("shell32.dll", PreserveSig = true)]
		private static extern int SHCreateShellItemArrayFromIDLists(uint count, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] IntPtr[] itemIdLists, out IShellItemArray shellItems);

		[DllImport("shell32.dll", CharSet = CharSet.Unicode)]
		private static extern uint ExtractIconEx(string fileName, int iconIndex, IntPtr[] largeIcons, IntPtr[] smallIcons, uint iconCount);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool DestroyIcon(IntPtr iconHandle);
	}
}
