/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class AvBitStreamFilter
	{
		/// <summary>
		/// 
		/// </summary>
		public CPointer<char> Name;

		/// <summary>
		/// A list of codec ids supported by the filter, terminated by
		/// AV_CODEC_ID_NONE.
		/// May be NULL, in that case the bitstream filter works with any codec id
		/// </summary>
		public CPointer<AvCodecId> Codec_Ids;

		/// <summary>
		/// A class for the private data, used to declare bitstream filter private
		/// AVOptions. This field is NULL for bitstream filters that do not declare
		/// any options.
		///
		/// If this field is non-NULL, the first member of the filter private data
		/// must be a pointer to AVClass, which will be set by libavcodec generic
		/// code to this class
		/// </summary>
		public AvClass Priv_Class;
	}
}
