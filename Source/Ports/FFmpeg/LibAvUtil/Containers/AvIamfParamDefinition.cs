/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// Parameters as defined in section 3.6.1 of IAMF.
	///
	/// The struct is allocated by av_iamf_param_definition_alloc() along with an
	/// array of subblocks, its type depending on the value of type.
	/// This array is placed subblocks_offset bytes after the start of this struct.
	///
	/// Note: This struct's size is not a part of the public ABI
	/// </summary>
	public class AvIamfParamDefinition : AvClass
	{
		/// <summary>
		/// 
		/// </summary>
		public AvClass Av_Class => this;

		/// <summary>
		/// Offset in bytes from the start of this struct, at which the subblocks
		/// array is located
		/// </summary>
		public size_t Subblock_Offset;

		/// <summary>
		/// Size in bytes of each element in the subblocks array
		/// </summary>
		public size_t Subblock_Size;

		/// <summary>
		/// Number of subblocks in the array
		/// </summary>
		public c_uint Nb_Subblocks;

		/// <summary>
		/// Parameters type. Determines the type of the subblock elements
		/// </summary>
		public AvIamfParamDefinitionType Type;

		/// <summary>
		/// Identifier for the parameter substream
		/// </summary>
		public c_uint Parameter_Id;

		/// <summary>
		/// Sample rate for the parameter substream. It must not be 0
		/// </summary>
		public c_uint Parameter_Rate;

		/// <summary>
		/// The accumulated duration of all blocks in this parameter definition,
		/// in units of 1 / parameter_rate.
		///
		/// May be 0, in which case all duration values should be specified in
		/// another parameter definition referencing the same parameter_id
		/// </summary>
		public c_uint Duration;

		/// <summary>
		/// The duration of every subblock in the case where all subblocks, with
		/// the optional exception of the last subblock, have equal durations.
		///
		/// Must be 0 if subblocks have different durations
		/// </summary>
		public c_uint Constant_Subblock_Duration;
	}
}
