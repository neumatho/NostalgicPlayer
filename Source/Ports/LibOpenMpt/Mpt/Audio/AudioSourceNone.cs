/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Audio
{
	/// <summary>
	/// 
	/// </summary>
	internal class AudioSourceNone : IAudioSource
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Process(Audio_Span_Planar<MixSampleInt> buffer)
		{
			for (size_t channel = 0; channel < buffer.Size_Channels(); ++channel)
			{
				for (size_t frame = 0; frame < buffer.Size_Frames(); ++frame)
					buffer[channel, frame] = 0;
			}
		}
	}
}
