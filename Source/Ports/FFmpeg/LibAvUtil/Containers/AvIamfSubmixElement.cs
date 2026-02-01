/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// Submix element as defined in section 3.7 of IAMF.
	///
	/// Note: The struct should be allocated with av_iamf_submix_add_element()
	///       and its size is not a part of the public ABI
	/// </summary>
	public class AvIamfSubmixElement : AvClass, IOptionContext
	{
		/// <summary>
		/// 
		/// </summary>
		public AvClass Av_Class => this;

		/// <summary>
		/// The id of the Audio Element this submix element references
		/// </summary>
		public c_uint Audio_Element_Id;

		/// <summary>
		/// Information required required for applying any processing to the
		/// referenced and rendered Audio Element before being summed with other
		/// processed Audio Elements.
		/// The AVIAMFParamDefinition.type "type" must be
		/// AV_IAMF_PARAMETER_DEFINITION_MIX_GAIN
		/// </summary>
		public AvIamfParamDefinition Element_Mix_Config;

		/// <summary>
		/// Default mix gain value to apply when there are no AVIAMFParamDefinition
		/// with element_mix_config "element_mix_config's"
		/// AVIAMFParamDefinition.parameter_id "parameter_id" available for a
		/// given audio frame
		/// </summary>
		public AvRational Default_Mix_Gain;

		/// <summary>
		/// A value that indicates whether the referenced channel-based Audio Element
		/// shall be rendered to stereo loudspeakers or spatialized with a binaural
		/// renderer when played back on headphones.
		/// If the Audio Element is not of AVIAMFAudioElement.audio_element_type
		/// "type" AV_IAMF_AUDIO_ELEMENT_TYPE_CHANNEL, then this field is undefined
		/// </summary>
		public AvIamfHeadphonesMode Headphones_Rendering_Mode;

		/// <summary>
		/// A dictionary of strings describing the submix in different languages.
		/// Must have the same amount of entries as
		/// AVIAMFMixPresentation.annotations "the mix's annotations", stored
		/// in the same order, and with the same key strings.
		///
		/// AVDictionaryEntry.key "key" is a string conforming to BCP-47 that
		/// specifies the language for the string stored in
		/// AVDictionaryEntry.value "value"
		/// </summary>
		public AvDictionary Annotations;
	}
}
