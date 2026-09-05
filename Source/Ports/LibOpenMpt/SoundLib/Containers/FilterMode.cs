/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// Filter modes
	/// </summary>
	internal enum FilterMode : uint8
	{
		Unchanged = 0xff,
		LowPass = 0,
		HighPass = 1
	}
}
