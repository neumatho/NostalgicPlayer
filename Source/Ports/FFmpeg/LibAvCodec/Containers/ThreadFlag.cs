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
	internal enum ThreadFlag
	{
		/// <summary>
		/// Thread has not been created, AVCodec->close mustn't be called
		/// </summary>
		Uninitialized,

		/// <summary>
		/// FFCodec->close needs to be called
		/// </summary>
		Needs_Close,

		/// <summary>
		/// Thread has been properly set up
		/// </summary>
		Initialized
	}
}
