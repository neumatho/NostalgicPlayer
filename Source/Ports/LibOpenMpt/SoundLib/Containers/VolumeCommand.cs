/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// Volume Column commands
	/// </summary>
	internal enum VolumeCommand : uint8
	{
		None = 0,
		Volume = 1,
		Panning = 2,
		VolSlideUp = 3,
		VolSlideDown = 4,
		FineVolUp = 5,
		FineVolDown = 6,
		VibratoSpeed = 7,
		VibratoDepth = 8,
		PanSlideLeft = 9,
		PanSlideRight = 10,
		TonePortamento = 11,
		PortaUp = 12,
		PortaDown = 13,
		PlayControl = 14,
		Offset = 15,

		Max_VolCmds
	}
}
