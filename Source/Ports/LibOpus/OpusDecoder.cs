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
	/// Interface to OpusDecoder API
	/// </summary>
	public class OpusDecoder : IDeepCloneable<OpusDecoder>
	{
		internal OpusDecoderInternal opusDecoder;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private OpusDecoder(OpusDecoderInternal decoder)
		{
			opusDecoder = decoder;
		}



		/********************************************************************/
		/// <summary>
		/// Allocates and initializes a decoder state.
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
		public static OpusDecoder Create(opus_int32 Fs, c_int channels, out OpusError error)
		{
			OpusDecoderInternal decoder = Opus_Decoder.Opus_Decoder_Create(Fs, channels, out error);
			if (error != OpusError.Ok)
				return null;

			return new OpusDecoder(decoder);
		}



		/********************************************************************/
		/// <summary>
		/// Decode an Opus packet
		/// </summary>
		/********************************************************************/
		public c_int Decode(CPointer<byte> data, opus_int32 len, CPointer<opus_int16> pcm, c_int frame_size, bool decode_fec)
		{
			return Opus_Decoder.Opus_Decode(opusDecoder, data, len, pcm, frame_size, decode_fec);
		}



		/********************************************************************/
		/// <summary>
		/// Decode an Opus packet with floating point output
		/// </summary>
		/********************************************************************/
		public c_int Decode_Float(CPointer<byte> data, opus_int32 len, CPointer<c_float> pcm, c_int frame_size, bool decode_fec)
		{
			return Opus_Decoder.Opus_Decode_Float(opusDecoder, data, len, pcm, frame_size, decode_fec);
		}



		/********************************************************************/
		/// <summary>
		/// Perform a CTL function on an Opus decoder
		/// </summary>
		/********************************************************************/
		public OpusError Decoder_Ctl_Get<T>(OpusControlGetRequest request, out T _out) where T : INumber<T>
		{
			return Opus_Decoder.Opus_Decoder_Ctl_Get(opusDecoder, request, out _out);
		}



		/********************************************************************/
		/// <summary>
		/// Perform a CTL function on an Opus decoder
		/// </summary>
		/********************************************************************/
		public OpusError Decoder_Ctl_Set(OpusControlSetRequest request, params object[] args)
		{
			return Opus_Decoder.Opus_Decoder_Ctl_Set(opusDecoder, request, args);
		}



		/********************************************************************/
		/// <summary>
		/// Frees an OpusDecoder allocated by Create()
		/// </summary>
		/********************************************************************/
		public void Destroy()
		{
			Opus_Decoder.Opus_Decoder_Destroy(opusDecoder);
			opusDecoder = null;
		}



		/********************************************************************/
		/// <summary>
		/// Gets the bandwidth of an Opus packet
		/// </summary>
		/********************************************************************/
		public static Bandwidth Packet_Get_Bandwidth(CPointer<byte> data)
		{
			return Opus_Decoder.Opus_Packet_Get_Bandwidth(data);
		}



		/********************************************************************/
		/// <summary>
		/// Gets the number of samples per frame from an Opus packet
		/// </summary>
		/********************************************************************/
		public static c_int Packet_Get_Samples_Per_Frame(CPointer<byte> data, opus_int32 Fs)
		{
			return Opus.Opus_Packet_Get_Samples_Per_Frame(data, Fs);
		}



		/********************************************************************/
		/// <summary>
		/// Gets the number of channels from an Opus packet
		/// </summary>
		/********************************************************************/
		public static c_int Packet_Get_Nb_Channels(CPointer<byte> data)
		{
			return Opus_Decoder.Opus_Packet_Get_Nb_Channels(data);
		}



		/********************************************************************/
		/// <summary>
		/// Gets the number of frames in an Opus packet
		/// </summary>
		/********************************************************************/
		public static c_int Packet_Get_Nb_Frames(CPointer<byte> packet, opus_int32 len)
		{
			return Opus_Decoder.Opus_Packet_Get_Nb_Frames(packet, len);
		}



		/********************************************************************/
		/// <summary>
		/// Gets the number of samples of an Opus packet
		/// </summary>
		/********************************************************************/
		public static c_int Packet_Get_Nb_Samples(CPointer<byte> packet, opus_int32 len, opus_int32 Fs)
		{
			return Opus_Decoder.Opus_Packet_Get_Nb_Samples(packet, len, Fs);
		}



		/********************************************************************/
		/// <summary>
		/// Gets the number of samples of an Opus packet
		/// </summary>
		/********************************************************************/
		public c_int Get_Nb_Samples(CPointer<byte> packet, opus_int32 len)
		{
			return Opus_Decoder.Opus_Decoder_Get_Nb_Samples(opusDecoder, packet, len);
		}



		/********************************************************************/
		/// <summary>
		/// Clone the current object into a new one
		/// </summary>
		/********************************************************************/
		public OpusDecoder MakeDeepClone()
		{
			return new OpusDecoder(opusDecoder.MakeDeepClone());
		}
	}
}
