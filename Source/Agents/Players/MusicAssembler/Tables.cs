/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.MusicAssembler
{
	/// <summary>
	/// Different tables needed
	/// </summary>
	internal static class Tables
	{
		/********************************************************************/
		/// <summary>
		/// Music Assembler does not map track 0-3 to channel 0-3, but
		/// differently
		/// </summary>
		/********************************************************************/
		public static readonly int[] ChannelMap =
		[
			0, 3, 1, 2
		];



		/********************************************************************/
		/// <summary>
		/// Periods
		/// </summary>
		/********************************************************************/
		public static readonly ushort[] Periods =
		[
			1712, 1616, 1524, 1440, 1356, 1280, 1208, 1140, 1076, 1016,  960,  906,
			 856 , 808,  762,  720,  678,  640,  604,  570,  538,  508,  480,  453,
			 428,  404,  381,  360,  339,  320,  302,  285,  269,  254,  240,  226,
			 214,  202,  190,  180,  170,  160,  151,  143,  135,  127,  120,  113
		];



		/********************************************************************/
		/// <summary>
		/// Used to stop playing a sample
		/// </summary>
		/********************************************************************/
		public static readonly sbyte[] EmptySample = [ 0, 0, 0, 0 ];
	}
}
