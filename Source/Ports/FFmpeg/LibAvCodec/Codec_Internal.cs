/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Codec_Internal
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static FFCodec FFCodec(AvCodec codec)
		{
			return (FFCodec)codec;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool FF_Codec_Is_Encoder(AvCodec avCodec)
		{
			FFCodec codec = FFCodec(avCodec);

			return !codec.Is_Decoder;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool FF_Codec_Is_Decoder(AvCodec avCodec)
		{
			FFCodec codec = FFCodec(avCodec);

			return codec.Is_Decoder;
		}
	}
}
