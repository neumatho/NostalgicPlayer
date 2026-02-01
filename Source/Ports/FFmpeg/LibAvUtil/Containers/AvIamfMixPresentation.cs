/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// Information on how to render and mix one or more AVIAMFAudioElement to generate
	/// the final audio output, as defined in section 3.7 of IAMF.
	///
	/// Note: The struct should be allocated with av_iamf_mix_presentation_alloc()
	///       and its size is not a part of the public ABI
	/// </summary>
	public class AvIamfMixPresentation : AvClass, IGroupType
	{
		/// <summary>
		/// 
		/// </summary>
		public AvClass Av_Class => this;

		/// <summary>
		/// Array of submixes.
		///
		/// Set by av_iamf_mix_presentation_add_submix(), must not be modified
		/// by any other code
		/// </summary>
		public CPointer<AvIamfSubmix> Submixes;

		/// <summary>
		/// Number of submixes in the presentation.
		///
		/// Set by av_iamf_mix_presentation_add_submix(), must not be modified
		/// by any other code
		/// </summary>
		public c_uint Nb_Submixes;

		/// <summary>
		/// A dictionary of strings describing the mix in different languages.
		/// Must have the same amount of entries as every
		/// AVIAMFSubmixElement.annotations "Submix element annotations",
		/// stored in the same order, and with the same key strings.
		///
		/// AVDictionaryEntry.key "key" is a string conforming to BCP-47
		/// that specifies the language for the string stored in
		/// AVDictionaryEntry.value "value"
		/// </summary>
		public AvDictionary Annotations;
	}
}
