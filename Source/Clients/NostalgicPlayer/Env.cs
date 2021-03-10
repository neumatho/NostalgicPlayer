/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Microsoft.Win32;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer
{
	/// <summary>
	/// Helper class to detect different things in the current environment
	/// </summary>
	public static class Env
	{
		/********************************************************************/
		/// <summary>
		/// Checks if running on Windows 10S
		/// </summary>
		/********************************************************************/
		public static bool IsWindows10S
		{
			get
			{
				if (!windows10S.HasValue)
				{
					object val = Registry.GetValue(@"HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\CI\Policy", "SkuPolicyRequired", "0");
					windows10S = val?.ToString() == "1";
				}

				return windows10S.Value;
			}
		}
		private static bool? windows10S = null;
	}
}
