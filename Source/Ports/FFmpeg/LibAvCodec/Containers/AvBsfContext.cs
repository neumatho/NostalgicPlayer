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
	/// The bitstream filter state.
	/// 
	/// This struct must be allocated with av_bsf_alloc() and freed with
	/// av_bsf_free().
	/// 
	/// The fields in the struct will only be changed (by the caller or by the
	/// filter) as described in their documentation, and are to be considered
	/// immutable otherwise
	/// </summary>
	public class AvBsfContext : AvClass, IOptionContext
	{
		/// <summary>
		/// A class for logging and AVOptions
		/// </summary>
		public AvClass Av_Class => this;

		/// <summary>
		/// The bitstream filter this context is an instance of
		/// </summary>
		public AvBitStreamFilter Filter;

		/// <summary>
		/// Opaque filter-specific private data. If filter->priv_class is non-NULL,
		/// this is an AVOptions-enabled struct
		/// </summary>
		public IPrivateData Priv_Data;

		/// <summary>
		/// Parameters of the input stream. This field is allocated in
		/// av_bsf_alloc(), it needs to be filled by the caller before
		/// av_bsf_init()
		/// </summary>
		public AvCodecParameters Par_In;

		/// <summary>
		/// Parameters of the output stream. This field is allocated in
		/// av_bsf_alloc(), it is set by the filter in av_bsf_init()
		/// </summary>
		public AvCodecParameters Par_Out;

		/// <summary>
		/// The timebase used for the timestamps of the input packets. Set by the
		/// caller before av_bsf_init()
		/// </summary>
		public AvRational Time_Base_In;

		/// <summary>
		/// The timebase used for the timestamps of the output packets. Set by the
		/// filter in av_bsf_init()
		/// </summary>
		public AvRational Time_Base_Out;
	}
}
