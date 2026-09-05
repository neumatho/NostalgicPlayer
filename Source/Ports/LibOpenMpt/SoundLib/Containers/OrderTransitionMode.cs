/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// When to execute a position override event
	/// </summary>
	internal enum OrderTransitionMode : uint8
	{
		AtPatternEnd,
		AtMeasureEnd,
		AtBeatEnd,
		AtRowEnd
	}
}
