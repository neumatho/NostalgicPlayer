/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Containers.Types;

namespace Polycode.NostalgicPlayer.Agent.Player.Tfmx
{
	/// <summary>
	/// Different tables needed
	/// </summary>
	internal static class Tables
	{
		/********************************************************************/
		/// <summary>
		/// MD5 checksum for Danger Freak Title song
		/// </summary>
		/********************************************************************/
		public static readonly byte[] DangerFreakTitle =
		[
			0x0a, 0x7b, 0xc5, 0x73, 0x1c, 0x51, 0xf8, 0x1b,
			0x6c, 0x88, 0xe3, 0xd6, 0x03, 0x13, 0xca, 0xba
		];



		/********************************************************************/
		/// <summary>
		/// MD5 checksum for Gem X Title song
		/// </summary>
		/********************************************************************/
		public static readonly byte[] GemXTitle =
		[
			0x92, 0x31, 0xbe, 0xb5, 0x3a, 0x18, 0xb2, 0xf4,
			0xfc, 0x0d, 0x4d, 0xce, 0x0c, 0x1c, 0xa6, 0x79
		];



		/********************************************************************/
		/// <summary>
		/// Panning values for 4 channels modules
		/// </summary>
		/********************************************************************/
		public static readonly ChannelPanningType[] Pan4 =
		[
			ChannelPanningType.Left, ChannelPanningType.Right, ChannelPanningType.Right, ChannelPanningType.Left
		];



		/********************************************************************/
		/// <summary>
		/// Panning values for 7 channels modules
		/// </summary>
		/********************************************************************/
		public static readonly ChannelPanningType[] Pan7 =
		[
			ChannelPanningType.Left, ChannelPanningType.Right, ChannelPanningType.Right,
			ChannelPanningType.Left, ChannelPanningType.Right, ChannelPanningType.Right, ChannelPanningType.Left
		];



		/********************************************************************/
		/// <summary>
		/// Periods
		/// </summary>
		/********************************************************************/
		public static readonly int[] NoteVals =
		[
			0x6ae, 0x64e, 0x5f4, 0x59e, 0x54d, 0x501,
			0x4b9, 0x475, 0x435, 0x3f9, 0x3c0, 0x38c, 0x358, 0x32a, 0x2fc, 0x2d0, 0x2a8, 0x282,
			0x25e, 0x23b, 0x21b, 0x1fd, 0x1e0, 0x1c6, 0x1ac, 0x194, 0x17d, 0x168, 0x154, 0x140,
			0x12f, 0x11e, 0x10e, 0x0fe, 0x0f0, 0x0e3, 0x0d6, 0x0ca, 0x0bf, 0x0b4, 0x0aa, 0x0a0,
			0x097, 0x08f, 0x087, 0x07f, 0x078, 0x071, 0x0d6, 0x0ca, 0x0bf, 0x0b4, 0x0aa, 0x0a0,
			0x097, 0x08f, 0x087, 0x07f, 0x078, 0x071, 0x0d6, 0x0ca, 0x0bf, 0x0b4
		];
	}
}
