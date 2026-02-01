/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// A layer defining a Channel Layout in the Audio Element.
	///
	/// When AVIAMFAudioElement.audio_element_type "the parent's Audio Element type"
	/// is AV_IAMF_AUDIO_ELEMENT_TYPE_CHANNEL, this corresponds to an Scalable Channel
	/// Layout layer as defined in section 3.6.2 of IAMF.
	/// For AV_IAMF_AUDIO_ELEMENT_TYPE_SCENE, it is an Ambisonics channel
	/// layout as defined in section 3.6.3 of IAMF.
	///
	/// Note: The struct should be allocated with av_iamf_audio_element_add_layer()
	///       and its size is not a part of the public ABI
	/// </summary>
	public class AvIamfLayer : AvClass, IOptionContext
	{
		/// <summary>
		/// 
		/// </summary>
		public AvClass Av_Class => this;

		/// <summary>
		/// 
		/// </summary>
		public AvChannelLayout Ch_Layout;

		/// <summary>
		/// A bitmask which may contain a combination of AV_IAMF_LAYER_FLAG_* flags
		/// </summary>
		public c_uint Flags;

		/// <summary>
		/// Output gain channel flags as defined in section 3.6.2 of IAMF.
		///
		/// This field is defined only if AVIAMFAudioElement.audio_element_type
		/// "the parent's Audio Element type" is AV_IAMF_AUDIO_ELEMENT_TYPE_CHANNEL,
		/// must be 0 otherwise
		/// </summary>
		public c_uint Output_Gain_Flags;

		/// <summary>
		/// Output gain as defined in section 3.6.2 of IAMF.
		///
		/// Must be 0 if output_gain_flags is 0
		/// </summary>
		public AvRational Output_Gain;

		/// <summary>
		/// Ambisonics mode as defined in section 3.6.3 of IAMF.
		///
		/// This field is defined only if AVIAMFAudioElement.audio_element_type
		/// "the parent's Audio Element type" is AV_IAMF_AUDIO_ELEMENT_TYPE_SCENE.
		///
		/// If AV_IAMF_AMBISONICS_MODE_MONO, channel_mapping is defined implicitly
		/// (Ambisonic Order) or explicitly (Custom Order with ambi channels) in
		/// ch_layout.
		/// If AV_IAMF_AMBISONICS_MODE_PROJECTION, demixing_matrix must be set
		/// </summary>
		public AvIamfAmbisonicsMode Ambisonics_Mode;

		/// <summary>
		/// Demixing matrix as defined in section 3.6.3 of IAMF.
		///
		/// The length of the array is ch_layout.nb_channels multiplied by the sum of
		/// the amount of streams in the group plus the amount of streams in the group
		/// that are stereo.
		///
		/// May be set only if ambisonics_mode == AV_IAMF_AMBISONICS_MODE_PROJECTION,
		/// must be NULL otherwise
		/// </summary>
		public AvRational Demixing_Matrix;
	}
}
