/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Containers;

namespace Polycode.NostalgicPlayer.Agent.Player.OpenMpt
{
	/// <summary>
	/// Helper class to identify the different formats
	/// </summary>
	internal static class OpenMptIdentifier
	{
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public static readonly string[] FileExtensions =
		[
			"mod"
		];



		/********************************************************************/
		/// <summary>
		/// Tests the module to see which type of module it is
		/// </summary>
		/********************************************************************/
		public static Probe_File_Header_Info TestModule(PlayerFileInfo fileInfo)
		{
			Probe_File_Header_Result retVal = LibOpenMpt.Probe_File_Header(Probe_File_Header_Flags.Default2, fileInfo.ModuleStream, out Probe_File_Header_Info testInfo);

			if (retVal == Probe_File_Header_Result.Success)
				return testInfo;

			return null;
		}
	}
}
