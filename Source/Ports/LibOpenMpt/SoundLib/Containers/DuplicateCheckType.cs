/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// DCT types (Duplicate Check Types)
	/// </summary>
	internal enum DuplicateCheckType : uint8
	{
		None = 0,
		Note = 1,
		Sample = 2,
		Instrument = 3,
		Plugin = 4
	}
}
