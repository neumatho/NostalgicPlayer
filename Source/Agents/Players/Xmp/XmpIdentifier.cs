/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Agent.Player.Xmp
{
	/// <summary>
	/// Helper class to identify the different formats
	/// </summary>
	internal static class XmpIdentifier
	{
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public static readonly string[] FileExtensions =
		[
			"gdm", "xm", "oxm", "s3m", "it", "669", "amf", "far", "imf", "stm", "stx", "ult", "mtm", "mod", "wow", "flx", "ptm", "arch", "xmf",
			"dsym", "rtm", "liq", "mgt", "mdl", "psm", "fnk", "dtm", "j2b", "coco"
		];



		/********************************************************************/
		/// <summary>
		/// Tests the module to see which type of module it is
		/// </summary>
		/********************************************************************/
		public static Xmp_Test_Info TestModule(PlayerFileInfo fileInfo)
		{
			LibXmp libXmp = LibXmp.Xmp_Create_Context();

			int retVal = libXmp.Xmp_Test_Module_From_File(fileInfo.ModuleStream, out Xmp_Test_Info testInfo);

			libXmp.Xmp_Free_Context();

			if (retVal == 0)
				return testInfo;

			return null;
		}
	}
}
