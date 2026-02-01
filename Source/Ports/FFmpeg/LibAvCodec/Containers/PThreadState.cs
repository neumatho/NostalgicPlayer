/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal enum PThreadState
	{
		/// <summary>
		/// Set when the thread is awaiting a packet
		/// </summary>
		Input_Ready,

		/// <summary>
		/// Set before the codec has called ff_thread_finish_setup()
		/// </summary>
		Setting_Up,

		/// <summary>
		/// Set after the codec has called ff_thread_finish_setup()
		/// </summary>
		Setup_Finished
	}
}
