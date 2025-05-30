/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Players
{
	/// <summary>
	/// Helper class with common methods for the player implementations
	/// </summary>
	internal static class PlayerHelper
	{
		/********************************************************************/
		/// <summary>
		/// Return the module information for the given player
		/// </summary>
		/********************************************************************/
		public static IEnumerable<string> GetModuleInformation(IModuleInformation playerAgent)
		{
			for (int i = 0; playerAgent.GetInformationString(i, out string description, out string value); i++)
			{
				// Make sure we don't have any invalid characters
				description = string.IsNullOrEmpty(description) ? string.Empty : description.Replace("\t", " ").Replace("\n", " ").Replace("\r", string.Empty);
				value = string.IsNullOrEmpty(value) ? string.Empty : value.Replace("\t", " ").Replace("\n", " ").Replace("\r", string.Empty);

				// Build the information in the list
				yield return $"{description}\t{value}";
			}
		}
	}
}
