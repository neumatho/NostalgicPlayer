/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// Plugin volumecommand handling options
	/// </summary>
	internal enum PlugVolumeHandling : uint8
	{
		Midi = 0,
		DryWet,
		Ignore,
		Custom,
		Max
	}
}
