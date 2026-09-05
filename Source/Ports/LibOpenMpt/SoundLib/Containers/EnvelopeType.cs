/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// Envelope types in instrument editor
	/// </summary>
	internal enum EnvelopeType : uint8
	{
		Volume = 0,
		Panning,
		Pitch,

		MaxTypes
	}
}
