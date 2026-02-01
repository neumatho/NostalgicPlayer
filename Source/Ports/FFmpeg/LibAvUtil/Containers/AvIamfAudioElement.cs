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
	/// Information on how to combine one or more audio streams, as defined in
	/// section 3.6 of IAMF.
	///
	/// Note: The struct should be allocated with av_iamf_audio_element_alloc()
	///       and its size is not a part of the public ABI
	/// </summary>
	public class AvIamfAudioElement : AvClass, IGroupType
	{
		/// <summary>
		/// 
		/// </summary>
		public AvClass Av_Class => this;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<AvIamfLayer> Layers;

		/// <summary>
		/// Number of layers, or channel groups, in the Audio Element.
		/// There may be 6 layers at most, and for audio_element_type
		/// AV_IAMF_AUDIO_ELEMENT_TYPE_SCENE, there may be exactly 1.
		///
		/// Set by av_iamf_audio_element_add_layer(), must not be
		/// modified by any other code
		/// </summary>
		public c_uint Nb_Layers;

		/// <summary>
		/// Demixing information used to reconstruct a scalable channel audio
		/// representation.
		///
		/// The AVIAMFParamDefinition.type "type" must be
		/// AV_IAMF_PARAMETER_DEFINITION_DEMIXING
		/// </summary>
		public AvIamfParamDefinition Demixing_Info;

		/// <summary>
		/// Recon gain information used to reconstruct a scalable channel audio
		/// representation.
		///
		/// The AVIAMFParamDefinition.type "type" must be
		/// AV_IAMF_PARAMETER_DEFINITION_RECON_GAIN
		/// </summary>
		public AvIamfParamDefinition Recon_Gain_Info;

		/// <summary>
		/// Audio element type as defined in section 3.6 of IAMF
		/// </summary>
		public AvIamfAudioElementType Audio_Element_Type;

		/// <summary>
		/// Default weight value as defined in section 3.6 of IAMF
		/// </summary>
		public c_uint Default_W;
	}
}
