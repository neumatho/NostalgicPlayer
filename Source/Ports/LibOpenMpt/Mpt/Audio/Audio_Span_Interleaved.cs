/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Audio
{
	/// <summary>
	/// LRLRLRLRLR
	/// </summary>
	internal readonly struct Audio_Span_Interleaved<TSampleType> : IAudioSpan<TSampleType, Audio_Span_Interleaved<TSampleType>>
	{
		private readonly CPointer<TSampleType> m_Buffer;
		private readonly size_t m_Channels;
		private readonly size_t m_Frames;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Audio_Span_Interleaved(CPointer<TSampleType> buffer, size_t channels, size_t frames)
		{
			m_Buffer = buffer;
			m_Channels = channels;
			m_Frames = frames;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ref TSampleType this[size_t channel, size_t frame]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref m_Buffer[(m_Channels * frame) + channel];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public size_t Size_Channels()
		{
			return m_Channels;
		}




		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public size_t Size_Frames()
		{
			return m_Frames;
		}
	}
}
