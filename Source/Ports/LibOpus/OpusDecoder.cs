/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;
using Polycode.NostalgicPlayer.Ports.LibOpus.Internal;

namespace Polycode.NostalgicPlayer.Ports.LibOpus
{
	/// <summary>
	/// Interface to OpusDecoder API
	/// </summary>
	public class OpusDecoder : IDeepCloneable<OpusDecoder>
	{
		private OpusDecoderInternal opusDecoder;

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
		/// 
		/// </summary>
		/********************************************************************/
		public OpusError Init(opus_int32 Fs, c_int channels)
		{
			return Opus_Decoder.Opus_Decoder_Init(opusDecoder, Fs, channels);
		}



		/********************************************************************/
		/// <summary>
		/// 
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
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Decode(Pointer<byte> data, opus_int32 len, Pointer<opus_int16> pcm, c_int frame_size, bool decode_fec)
		{
			return Opus_Decoder.Opus_Decode(opusDecoder, data, len, pcm, frame_size, decode_fec);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public OpusError Decoder_Ctl_Get<T>(OpusControlGetRequest request, out T _out) where T : INumber<T>
		{
			return Opus_Decoder.Opus_Decoder_Ctl_Get(opusDecoder, request, out _out);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public OpusError Decoder_Ctl_Set(OpusControlSetRequest request, params object[] args)
		{
			return Opus_Decoder.Opus_Decoder_Ctl_Set(opusDecoder, request, args);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Destroy()
		{
			Opus_Decoder.Opus_Decoder_Destroy(opusDecoder);
			opusDecoder = null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Bandwidth Packet_Get_Bandwidth(Pointer<byte> data)
		{
			return Opus_Decoder.Opus_Packet_Get_Bandwidth(data);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Packet_Get_Nb_Channels(Pointer<byte> data)
		{
			return Opus_Decoder.Opus_Packet_Get_Nb_Channels(data);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Packet_Get_Nb_Frames(Pointer<byte> packet, opus_int32 len)
		{
			return Opus_Decoder.Opus_Packet_Get_Nb_Frames(packet, len);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Packet_Get_Nb_Samples(Pointer<byte> packet, opus_int32 len, opus_int32 Fs)
		{
			return Opus_Decoder.Opus_Packet_Get_Nb_Samples(packet, len, Fs);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Get_Nb_Samples(Pointer<byte> packet, opus_int32 len)
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
