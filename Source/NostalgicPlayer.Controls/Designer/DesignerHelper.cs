/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;

namespace Polycode.NostalgicPlayer.Controls.Designer
{
	/// <summary>
	/// Helper class with designer methods
	/// </summary>
	internal static class DesignerHelper
	{
		// True when the current process is a Visual Studio designer host.
		// "DesignToolsServer" is the out-of-process WinForms designer used by
		// modern .NET projects; "devenv" covers the in-process designer. The
		// process name never changes, so this is computed once
		private static readonly bool isDesignProcess = IsProcessNamed("DesignToolsServer") || IsProcessNamed("devenv");

		/********************************************************************/
		/// <summary>
		/// Check if we are running inside the designer, without needing a
		/// control instance. Unlike the Site/Parent based check, this also
		/// works from a constructor (where Site and Parent are still null)
		/// and for child controls that are never sited directly on the
		/// designer.
		/// This is needed so themed fonts can be applied before the
		/// designer's first layout pass, otherwise font dependent sizes
		/// keep changing after deserialization and the designer marks the
		/// file as modified
		/// </summary>
		/********************************************************************/
		public static bool IsInDesignMode()
		{
			return isDesignProcess || (LicenseManager.UsageMode == LicenseUsageMode.Designtime);
		}



		/********************************************************************/
		/// <summary>
		/// Check if the control is in design mode by walking up the parent
		/// chain. This is needed because DesignMode returns false for child
		/// controls that are not directly sited on the designer
		/// </summary>
		/********************************************************************/
		public static bool IsInDesignMode(Control ctrl)
		{
			while (ctrl != null)
			{
				if (ctrl.Site?.DesignMode == true)
					return true;

				ctrl = ctrl.Parent;
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Check if the current process has the given name
		/// </summary>
		/********************************************************************/
		private static bool IsProcessNamed(string name)
		{
			try
			{
				using (Process process = Process.GetCurrentProcess())
				{
					return string.Equals(process.ProcessName, name, StringComparison.OrdinalIgnoreCase);
				}
			}
			catch
			{
				return false;
			}
		}
	}
}
