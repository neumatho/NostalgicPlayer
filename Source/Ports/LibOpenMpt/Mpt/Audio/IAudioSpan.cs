/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Audio
{
	/// <summary>
	/// 
	/// </summary>
	internal interface IAudioSpan<TSampleType, TSelf> where TSelf : struct, IAudioSpan<TSampleType, TSelf>
	{
		ref TSampleType this[size_t channel, size_t frame] { get; }

		size_t Size_Channels();
		size_t Size_Frames();
	}
}
