/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Audio
{
	/// <summary>
	/// Views another span with the first offsetFrames frames skipped.
	/// TNE: The original also has make_audio_span_with_offset(), which only
	/// exists to deduce the buffer type. Here both type arguments are known
	/// at the call site, so the constructor is used directly
	/// </summary>
	internal struct Audio_Span_With_Offset<TBuffer, TSampleType> : IAudioSpan<TSampleType, Audio_Span_With_Offset<TBuffer, TSampleType>> where TBuffer : struct, IAudioSpan<TSampleType, TBuffer>
	{
		private TBuffer m_Buffer;
		private readonly size_t m_Offset;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Audio_Span_With_Offset(TBuffer buffer, size_t offsetFrames)
		{
			m_Buffer = buffer;
			m_Offset = offsetFrames;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ref TSampleType this[size_t channel, size_t frame]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref m_Buffer[channel, m_Offset + frame];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public size_t Size_Channels()
		{
			return m_Buffer.Size_Channels();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public size_t Size_Frames()
		{
			return m_Buffer.Size_Frames() - m_Offset;
		}
	}
}
