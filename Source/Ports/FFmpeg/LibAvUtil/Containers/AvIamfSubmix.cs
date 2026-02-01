/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// Submix layout as defined in section 3.7 of IAMF.
	///
	/// Note: The struct should be allocated with av_iamf_mix_presentation_add_submix()
	///       and its size is not a part of the public ABI
	/// </summary>
	public class AvIamfSubmix : AvClass
	{
		/// <summary>
		/// 
		/// </summary>
		public AvClass Av_Class => this;

		/// <summary>
		/// Array of submix elements.
		///
		/// Set by av_iamf_submix_add_element(), must not be modified by any
		/// other code
		/// </summary>
		public CPointer<AvIamfSubmixElement> Elements;

		/// <summary>
		/// Number of elements in the submix.
		///
		/// Set by av_iamf_submix_add_element(), must not be modified by any
		/// other code
		/// </summary>
		public c_uint Nb_Elements;

		/// <summary>
		/// Array of submix layouts.
		///
		/// Set by av_iamf_submix_add_layout(), must not be modified by any
		/// other code
		/// </summary>
		public CPointer<AvIamfSubmixLayout> Layouts;

		/// <summary>
		/// Number of layouts in the submix.
		///
		/// Set by av_iamf_submix_add_layout(), must not be modified by any
		/// other code
		/// </summary>
		public c_uint Nb_Layouts;

		/// <summary>
		/// Information required for post-processing the mixed audio signal to
		/// generate the audio signal for playback.
		///
		/// The AVIAMFParamDefinition.type "type" must be
		/// AV_IAMF_PARAMETER_DEFINITION_MIX_GAIN
		/// </summary>
		public AvIamfParamDefinition Output_Mix_Config;

		/// <summary>
		/// Default mix gain value to apply when there are no AVIAMFParamDefinition
		/// with output_mix_config "output_mix_config's"
		///
		/// AVIAMFParamDefinition.parameter_id "parameter_id" available for a
		/// given audio frame
		/// </summary>
		public AvRational Default_Mix_Gain;
	}
}
