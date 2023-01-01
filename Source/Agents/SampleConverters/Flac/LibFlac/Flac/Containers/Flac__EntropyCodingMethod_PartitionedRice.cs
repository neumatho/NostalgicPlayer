/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Flac.Containers
{
	/// <summary>
	/// Header for a Rice partitioned residual
	/// </summary>
	internal class Flac__EntropyCodingMethod_PartitionedRice : IEntropyCodingMethod
	{
		/// <summary>
		/// The partition order, i.e. # of contexts = 2 ^ Order
		/// </summary>
		public uint32_t Order;

		/// <summary>
		/// The context's Rice parameters and/or raw bits
		/// </summary>
		public Flac__EntropyCodingMethod_PartitionedRiceContents Contents;
	}
}
