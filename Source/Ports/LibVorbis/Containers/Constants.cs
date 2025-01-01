/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibVorbis.Containers
{
	/// <summary>
	/// Different constants
	/// </summary>
	internal static class Constants
	{
		// Codec_internal.h
		public const int PacketBlobs = 15;

		// Registry.h
		public const int Vi_TransformB = 1;
		public const int Vi_WindowB = 1;
		public const int Vi_TimeB = 1;
		public const int Vi_FloorB = 2;
		public const int Vi_ResB = 3;
		public const int Vi_MapB = 1;

		// VorbisInfoFloor1.c
		public const int Vif_Posit = 63;
		public const int Vif_Class = 16;
		public const int Vif_Parts = 31;
	}
}
