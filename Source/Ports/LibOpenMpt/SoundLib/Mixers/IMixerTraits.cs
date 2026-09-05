/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Mixers
{
	/// <summary>
	/// 
	/// </summary>
	internal interface IMixerTraits<TIn> where TIn : unmanaged
	{
		static abstract c_int NumChannelsIn  { get; }
		static abstract c_int NumChannelsOut { get; }
		static abstract mixsample_t Convert(TIn x);
	}
}
