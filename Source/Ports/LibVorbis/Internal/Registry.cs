/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibVorbis.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibVorbis.Internal
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Registry
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly VorbisFuncFloor[] floor_P =
		[
			Floor0.ExportBundle,
			Floor1.ExportBundle
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly VorbisFuncResidue[] residue_P =
		[
			Residue.Residue0_ExportBundle,
			Residue.Residue1_ExportBundle,
			Residue.Residue2_ExportBundle
		];



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly VorbisFuncMapping[] mapping_P =
		[
			Mapping0.ExportBundle
		];
	}
}
