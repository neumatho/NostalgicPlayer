/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// Submix layout as defined in section 3.7.6 of IAMF.
	///
	/// Note: The struct should be allocated with av_iamf_submix_add_layout()
	///       and its size is not a part of the public ABI
	/// </summary>
	public class AvIamfSubmixLayout : AvClass, IOptionContext
	{
		/// <summary>
		/// 
		/// </summary>
		public AvClass Av_Class => this;

		/// <summary>
		/// 
		/// </summary>
		public AvIamfSubmixLayoutType Layout_Type;

		/// <summary>
		/// Channel layout matching one of Sound Systems A to J of ITU-2051-3, plus
		/// 7.1.2ch, 3.1.2ch, and binaural.
		/// If layout_type is not AV_IAMF_SUBMIX_LAYOUT_TYPE_LOUDSPEAKERS or
		/// AV_IAMF_SUBMIX_LAYOUT_TYPE_BINAURAL, this field is undefined
		/// </summary>
		public AvChannelLayout Sound_System;

		/// <summary>
		/// The program integrated loudness information, as defined in
		/// ITU-1770-4
		/// </summary>
		public AvRational Integrated_Loudness;

		/// <summary>
		/// The digital (sampled) peak value of the audio signal, as defined
		/// in ITU-1770-4
		/// </summary>
		public AvRational Digital_Peak;

		/// <summary>
		/// The true peak of the audio signal, as defined in ITU-1770-4
		/// </summary>
		public AvRational True_Peak;

		/// <summary>
		/// The Dialogue loudness information, as defined in ITU-1770-4
		/// </summary>
		public AvRational Dialogue_Anchored_Loudness;

		/// <summary>
		/// The Album loudness information, as defined in ITU-1770-4
		/// </summary>
		public AvRational Album_Anchored_Loudness;
	}
}
