/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Mixers
{
	/// <summary>
	/// 
	/// </summary>
	internal interface IMixer
	{
		void Init(ModChannel chn);
		void Mix(ReadOnlySpan<mixsample_t> outSample, ModChannel chn, CPointer<mixsample_t> outBuffer);
		void Done(ModChannel chn);
	}
}
