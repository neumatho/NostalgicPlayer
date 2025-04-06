/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class OpusMsDecoderInternal : IDeepCloneable<OpusMsDecoderInternal>
	{
		public ChannelLayout layout = new ChannelLayout();

		public OpusDecoder[] coupledDecoders;
		public OpusDecoder[] monoDecoders;

		/********************************************************************/
		/// <summary>
		/// Clone the current object into a new one
		/// </summary>
		/********************************************************************/
		public OpusMsDecoderInternal MakeDeepClone()
		{
			OpusMsDecoderInternal clone = new OpusMsDecoderInternal
			{
				layout = layout.MakeDeepClone(),
			};

			clone.coupledDecoders = ArrayHelper.CloneObjectArray(coupledDecoders);
			clone.monoDecoders = ArrayHelper.CloneObjectArray(monoDecoders);

			return clone;
		}
	}
}
