/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Mixers
{
	/// <summary>
	/// 
	/// </summary>
	internal interface IFilter
	{
		void Init(ModChannel chn, c_int numChannelsIn);
		void Filter(Span<mixsample_t> outSample, ModChannel chn, c_int numChannelsIn);
		void Done(ModChannel channel, c_int channelsIn);
	}
}
