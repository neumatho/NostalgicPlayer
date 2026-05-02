/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Codec.Containers
{
	/// <summary>
	/// Channel group for channel transformations
	/// </summary>
	internal class WmaProChannelGrp
	{
		/// <summary>
		/// Number of channels in the group
		/// </summary>
		public uint8_t Num_Channels;

		/// <summary>
		/// Transform on / off
		/// </summary>
		public int8_t Transform;

		/// <summary>
		/// Controls if the transform is enabled for a certain band
		/// </summary>
		public readonly int8_t[] Transform_Band = new int8_t[WmaConstants.Max_Bands];

		/// <summary>
		/// 
		/// </summary>
		public readonly c_float[] Decorrelation_Matrix = new c_float[WmaConstants.WmaPro_Max_Channels * WmaConstants.WmaPro_Max_Channels];

		/// <summary>
		/// Transformation coefficients
		/// </summary>
		public readonly CPointer<c_float>[] Channel_Data = ArrayHelper.InitializeArray<CPointer<c_float>>(WmaConstants.WmaPro_Max_Channels);
	}
}
