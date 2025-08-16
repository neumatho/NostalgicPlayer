/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;
using Polycode.NostalgicPlayer.Ports.LibOpus.Internal;

namespace Polycode.NostalgicPlayer.Ports.LibOpus
{
	/// <summary>
	/// Interface to OpusMsDecoder API
	/// </summary>
	public class OpusMsDecoder : IDeepCloneable<OpusMsDecoder>
	{
		private OpusMsDecoderInternal opusMsDecoder;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private OpusMsDecoder(OpusMsDecoderInternal decoder)
		{
			opusMsDecoder = decoder;
		}



		/********************************************************************/
		/// <summary>
		/// Allocates and initializes a multistream decoder state.
		///
		/// Internally Opus stores data at 48000 Hz, so that should be the
		/// default value for Fs. However, the decoder can efficiently decode
		/// to buffers at 8, 12, 16, and 24 kHz so if for some reason the
		/// caller cannot use data at the full sample rate, or knows the
		/// compressed data doesn't use the full frequency range, it can
		/// request decoding at a reduced rate. Likewise, the decoder is
		/// capable of filling in either mono or interleaved stereo pcm
		/// buffers, at the caller's request
		/// </summary>
		/********************************************************************/
		public static OpusMsDecoder Create(opus_int32 Fs, c_int channels, c_int streams, c_int coupled_streams, CPointer<byte> mapping, out OpusError error)
		{
			OpusMsDecoderInternal decoder = Opus_MultiStream_Decoder.Opus_MultiStream_Decoder_Create(Fs, channels, streams, coupled_streams, mapping, out error);
			if (error != OpusError.Ok)
				return null;

			return new OpusMsDecoder(decoder);
		}



		/********************************************************************/
		/// <summary>
		/// Decode an Opus packet
		/// </summary>
		/********************************************************************/
		public c_int Decode(CPointer<byte> data, opus_int32 len, CPointer<opus_int16> pcm, c_int frame_size, bool decode_fec)
		{
			return Opus_MultiStream_Decoder.Opus_MultiStream_Decode(opusMsDecoder, data, len, pcm, frame_size, decode_fec);
		}



		/********************************************************************/
		/// <summary>
		/// Decode an Opus packet with floating point output
		/// </summary>
		/********************************************************************/
		public c_int Decode_Float(CPointer<byte> data, opus_int32 len, CPointer<c_float> pcm, c_int frame_size, bool decode_fec)
		{
			return Opus_MultiStream_Decoder.Opus_MultiStream_Decode_Float(opusMsDecoder, data, len, pcm, frame_size, decode_fec);
		}



		/********************************************************************/
		/// <summary>
		/// Perform a CTL function on a multistream Opus decoder
		/// </summary>
		/********************************************************************/
		public OpusError Decoder_Ctl_Get<T>(OpusControlGetRequest request, out T _out) where T : INumber<T>
		{
			return Opus_MultiStream_Decoder.Opus_MultiStream_Decoder_Ctl_Get(opusMsDecoder, request, out _out);
		}



		/********************************************************************/
		/// <summary>
		/// Perform a CTL function on a multistream Opus decoder
		/// </summary>
		/********************************************************************/
		public OpusError Decoder_Ctl_Get<T>(OpusControlGetRequest request, c_int arg1, out T _out)
		{
			return Opus_MultiStream_Decoder.Opus_MultiStream_Decoder_Ctl_Get(opusMsDecoder, request, arg1, out _out);
		}



		/********************************************************************/
		/// <summary>
		/// Perform a CTL function on an Opus decoder
		/// </summary>
		/********************************************************************/
		public OpusError Decoder_Ctl_Set(OpusControlSetRequest request, params object[] args)
		{
			return Opus_MultiStream_Decoder.Opus_MultiStream_Decoder_Ctl_Set(opusMsDecoder, request, args);
		}



		/********************************************************************/
		/// <summary>
		/// Frees an OpusMsDecoder allocated by Create()
		/// </summary>
		/********************************************************************/
		public void Destroy()
		{
			Opus_MultiStream_Decoder.Opus_MultiStream_Decoder_Destroy(opusMsDecoder);
			opusMsDecoder = null;
		}



		/********************************************************************/
		/// <summary>
		/// Clone the current object into a new one
		/// </summary>
		/********************************************************************/
		public OpusMsDecoder MakeDeepClone()
		{
			return new OpusMsDecoder(opusMsDecoder.MakeDeepClone());
		}
	}
}
