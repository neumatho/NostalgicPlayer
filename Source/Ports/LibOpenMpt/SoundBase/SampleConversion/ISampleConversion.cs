/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundBase.SampleConversion
{
	/// <summary>
	/// 
	/// </summary>
	internal interface ISampleConversion<TOut> where TOut : unmanaged
	{
		static abstract size_t Input_Inc { get; }
		TOut Convert(CPointer<uint8> inBuf);
	}
}
