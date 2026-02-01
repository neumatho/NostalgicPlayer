/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// This struct stores per-frame lavc-internal data and is attached to it via
	/// private_ref
	/// </summary>
	public class FrameDecodeData : RefCount, IRefCountData
	{
		/// <summary>
		/// The callback to perform some delayed processing on the frame right
		/// before it is returned to the caller.
		///
		/// Note: This code is called at some unspecified point after the frame is
		/// returned from the decoder's decode/receive_frame call. Therefore it cannot rely
		/// on AVCodecContext being in any specific state, so it does not get to
		/// access AVCodecContext directly at all. All the state it needs must be
		/// stored in the post_process_opaque object
		/// </summary>
		public CodecFunc.Post_Process_Delegate Post_Process;

		/// <summary>
		/// 
		/// </summary>
		public IOpaque Post_Process_Opaque;

		/// <summary>
		/// 
		/// </summary>
		public CodecFunc.Post_Process_Opaque_Free_Delegate Post_Process_Opaque_Free;

		/// <summary>
		/// Per-frame private data for hwaccels
		/// </summary>
		public IPrivateData HwAccel_Priv;

		/// <summary>
		/// 
		/// </summary>
		public CodecFunc.HwAccel_Priv_Free_Delegate HwAccel_Priv_Free;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			Post_Process = null;
			Post_Process_Opaque = null;
			Post_Process_Opaque_Free = null;
			HwAccel_Priv = null;
			HwAccel_Priv_Free = null;
		}
	}
}
