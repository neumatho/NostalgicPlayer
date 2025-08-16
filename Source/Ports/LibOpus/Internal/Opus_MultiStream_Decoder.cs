/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Linq;
using System.Numerics;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;
using Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Celt;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Opus_MultiStream_Decoder
	{
		private delegate void Opus_Copy_Channel_Out_Func<T>(CPointer<T> dst, c_int dst_stride, c_int dst_channel, CPointer<opus_val16> src, c_int src_stride, c_int frame_size, object user_data);

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static OpusError Opus_MultiStream_Decoder_Init(OpusMsDecoderInternal st, opus_int32 Fs, c_int channels, c_int streams, c_int coupled_streams, CPointer<byte> mapping)
		{
			c_int i;

			if ((channels > 255) || (channels < 1) || (coupled_streams > streams) || (streams < 1) || (coupled_streams < 0) || (streams > (255 - coupled_streams)))
				return OpusError.Bad_Arg;

			st.layout.nb_channels = channels;
			st.layout.nb_streams = streams;
			st.layout.nb_coupled_streams = coupled_streams;

			for (i = 0; i < st.layout.nb_channels; i++)
				st.layout.mapping[i] = mapping[i];

			if (!Opus_MultiStream.Validate_Layout(st.layout))
				return OpusError.Bad_Arg;

			st.coupledDecoders = new OpusDecoder[st.layout.nb_coupled_streams];

			for (i = 0; i < st.layout.nb_coupled_streams; i++)
			{
				st.coupledDecoders[i] = OpusDecoder.Create(Fs, 2, out OpusError ret);
				if (ret != OpusError.Ok)
					return ret;
			}

			st.monoDecoders = new OpusDecoder[st.layout.nb_streams - st.layout.nb_coupled_streams];

			for (c_int j = 0; i < st.layout.nb_streams; i++, j++)
			{
				st.monoDecoders[j] = OpusDecoder.Create(Fs, 1, out OpusError ret);
				if (ret != OpusError.Ok)
					return ret;
			}

			return OpusError.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static OpusMsDecoderInternal Opus_MultiStream_Decoder_Create(opus_int32 Fs, c_int channels, c_int streams, c_int coupled_streams, CPointer<byte> mapping, out OpusError error)
		{
			if ((channels > 255) || (channels < 1) || (coupled_streams > streams) || (streams < 1) || (coupled_streams < 0) || (streams > (255 - coupled_streams)))
			{
				error = OpusError.Bad_Arg;
				return null;
			}

			OpusMsDecoderInternal st = Memory.Opus_Alloc<OpusMsDecoderInternal>();
			if (st == null)
			{
				error = OpusError.Alloc_Fail;
				return null;
			}

			error = Opus_MultiStream_Decoder_Init(st, Fs, channels, streams, coupled_streams, mapping);
			if (error != OpusError.Ok)
			{
				Memory.Opus_Free(st);
				st = null;
			}

			return st;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Opus_MultiStream_Packet_Validate(CPointer<byte> data, opus_int32 len, c_int nb_streams, opus_int32 Fs)
		{
			opus_int16[] size = new opus_int16[48];
			c_int samples = 0;

			for (c_int s = 0; s < nb_streams; s++)
			{
				if (len <= 0)
					return (c_int)OpusError.Invalid_Packet;

				c_int count = Opus.Opus_Packet_Parse_Impl(data, len, s != nb_streams - 1, out byte toc, null, size, out _, out opus_int32 packet_offset, out _, out _);
				if (count < 0)
					return count;

				c_int tmp_samples = Opus_Decoder.Opus_Packet_Get_Nb_Samples(data, packet_offset, Fs);

				if ((s != 0) && (samples != tmp_samples))
					return (c_int)OpusError.Invalid_Packet;

				samples = tmp_samples;
				data += packet_offset;
				len -= packet_offset;
			}

			return samples;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Opus_MultiStream_Decode_Native<T>(OpusMsDecoderInternal st, CPointer<byte> data, opus_int32 len, CPointer<T> pcm, Opus_Copy_Channel_Out_Func<T> copy_channel_out, c_int frame_size, bool decode_fec, bool soft_clip, object user_data)
		{
			bool do_plc = false;

			if (frame_size <= 0)
				return (c_int)OpusError.Bad_Arg;

			// Limit frame size to avoid excessive stack allocation
			if (Opus_MultiStream_Decoder_Ctl_Get(st, OpusControlGetRequest.Opus_Get_Sample_Rate, out opus_int32 Fs) != OpusError.Ok)
				return (c_int)OpusError.Internal_Error;

			frame_size = Arch.IMIN(frame_size, Fs / 25 * 3);
			CPointer<opus_val16> buf = new CPointer<opus_val16>(2 * frame_size);

			if (len == 0)
				do_plc = true;

			if (len < 0)
				return (c_int)OpusError.Bad_Arg;

			if (!do_plc && (len < (2 * st.layout.nb_streams - 1)))
				return (c_int)OpusError.Invalid_Packet;

			if (!do_plc)
			{
				c_int ret = Opus_MultiStream_Packet_Validate(data, len, st.layout.nb_streams, Fs);

				if (ret < 0)
					return ret;
				else if (ret > frame_size)
					return (c_int)OpusError.Buffer_Too_Small;
			}

			for (c_int s = 0; s < st.layout.nb_streams; s++)
			{
				OpusDecoder dec = s < st.layout.nb_coupled_streams ? st.coupledDecoders[s] : st.monoDecoders[s - st.layout.nb_coupled_streams];

				if (!do_plc && (len <= 0))
					return (c_int)OpusError.Internal_Error;

				opus_int32 packet_offset = 0;

				c_int ret = Opus_Decoder.Opus_Decode_Native(dec.opusDecoder, data, len, buf, frame_size, decode_fec, s != st.layout.nb_streams - 1, out packet_offset, soft_clip, null, 0);

				if (!do_plc)
				{
					data += packet_offset;
					len -= packet_offset;
				}

				if (ret <= 0)
					return ret;

				frame_size = ret;

				if (s < st.layout.nb_coupled_streams)
				{
					c_int chan;
					c_int prev = -1;

					// Copy "left" audio to the channel(s) where it belongs
					while ((chan = Opus_MultiStream.Get_Left_Channel(st.layout, s, prev)) != -1)
					{
						copy_channel_out(pcm, st.layout.nb_channels, chan, buf, 2, frame_size, user_data);
						prev = chan;
					}

					prev = -1;

					// Copy "right" audio to the channel(s) where it belongs
					while ((chan = Opus_MultiStream.Get_Right_Channel(st.layout, s, prev)) != -1)
					{
						copy_channel_out(pcm, st.layout.nb_channels, chan, buf + 1, 2, frame_size, user_data);
						prev = chan;
					}
				}
				else
				{
					c_int chan;
					c_int prev = -1;

					// Copy audio to the channel(s) where it belongs
					while ((chan = Opus_MultiStream.Get_Mono_Channel(st.layout, s, prev)) != -1)
					{
						copy_channel_out(pcm, st.layout.nb_channels, chan, buf, 1, frame_size, user_data);
						prev = chan;
					}
				}
			}

			// Handle muted channels
			for (c_int c = 0; c < st.layout.nb_channels; c++)
			{
				if (st.layout.mapping[c] == 255)
					copy_channel_out(pcm, st.layout.nb_channels, c, null, 0, frame_size, user_data);
			}

			return frame_size;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Opus_Copy_Channel_Out_Short(CPointer<opus_int16> dst, c_int dst_stride, c_int dst_channel, CPointer<opus_val16> src, c_int src_stride, c_int frame_size, object user_data)
		{
			CPointer<opus_int16> short_dst = dst;

			if (src.IsNotNull)
			{
				for (opus_int32 i = 0; i < frame_size; i++)
					short_dst[i * dst_stride + dst_channel] = Float_Cast.Float2Int16(src[i * src_stride]);
			}
			else
			{
				for (opus_int32 i = 0; i < frame_size; i++)
					short_dst[i * dst_stride + dst_channel] = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Opus_Copy_Channel_Out_Float(CPointer<c_float> dst, c_int dst_stride, c_int dst_channel, CPointer<opus_val16> src, c_int src_stride, c_int frame_size, object user_data)
		{
			CPointer<c_float> float_dst = dst;

			if (src.IsNotNull)
			{
				for (opus_int32 i = 0; i < frame_size; i++)
					float_dst[i * dst_stride + dst_channel] = src[i * src_stride];
			}
			else
			{
				for (opus_int32 i = 0; i < frame_size; i++)
					float_dst[i * dst_stride + dst_channel] = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Opus_MultiStream_Decode(OpusMsDecoderInternal st, CPointer<byte> data, opus_int32 len, CPointer<opus_int16> pcm, c_int frame_size, bool decode_fec)
		{
			return Opus_MultiStream_Decode_Native(st, data, len, pcm, Opus_Copy_Channel_Out_Short, frame_size, decode_fec, true, null);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Opus_MultiStream_Decode_Float(OpusMsDecoderInternal st, CPointer<byte> data, opus_int32 len, CPointer<opus_val16> pcm, c_int frame_size, bool decode_fec)
		{
			return Opus_MultiStream_Decode_Native(st, data, len, pcm, Opus_Copy_Channel_Out_Float, frame_size, decode_fec, false, null);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static OpusError Opus_MultiStream_Decoder_Ctl_Get<T>(OpusMsDecoderInternal st, OpusControlGetRequest request, out T _out) where T : INumber<T>
		{
			// I know, the casting below in the case statements are not pretty
			if (typeof(T) == typeof(c_int))
			{
				switch (request)
				{
					case OpusControlGetRequest.Opus_Get_Bandwidth:
					case OpusControlGetRequest.Opus_Get_Sample_Rate:
					case OpusControlGetRequest.Opus_Get_Gain:
					case OpusControlGetRequest.Opus_Get_Last_Packet_Duration:
					{
						// Just query the first stream
						OpusDecoder dec = st.coupledDecoders.Concat(st.monoDecoders).First();
						return dec.Decoder_Ctl_Get(request, out _out);
					}
				}
			}
			else if (typeof(T) == typeof(opus_uint32))
			{
				switch (request)
				{
					case OpusControlGetRequest.Opus_Get_Final_Range:
					{
						opus_uint32 value = 0;

						foreach (OpusDecoder dec in st.coupledDecoders.Concat(st.monoDecoders))
						{
							OpusError ret = dec.Decoder_Ctl_Get(request, out opus_uint32 tmp);
							if (ret != OpusError.Ok)
								break;

							value ^= tmp;
						}

						_out = (T)(object)value;
						return OpusError.Ok;
					}
				}
			}

			_out = default;
			return OpusError.Unimplemented;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static OpusError Opus_MultiStream_Decoder_Ctl_Get<T>(OpusMsDecoderInternal st, OpusControlGetRequest request, c_int arg1, out T _out)
		{
			// I know, the casting below in the case statements are not pretty
			if (typeof(T) == typeof(OpusDecoder))
			{
				switch (request)
				{
					case OpusControlGetRequest.Opus_MultiStream_Decoder_State:
					{
						opus_int32 stream_id = arg1;
						_out = (T)(object)null;

						if ((stream_id < 0) || (stream_id >= st.layout.nb_streams))
							return OpusError.Bad_Arg;

						foreach (OpusDecoder dec in st.coupledDecoders.Concat(st.monoDecoders))
						{
							if (stream_id == 0)
							{
								_out = (T)(object)dec;
								break;
							}

							stream_id--;
						}

						return OpusError.Ok;
					}
				}
			}

			_out = default;
			return OpusError.Unimplemented;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static OpusError Opus_MultiStream_Decoder_Ctl_Set(OpusMsDecoderInternal st, OpusControlSetRequest request, params object[] args)
		{
			switch (request)
			{
				case OpusControlSetRequest.Opus_Reset_State:
				{
					foreach (OpusDecoder dec in st.coupledDecoders.Concat(st.monoDecoders))
					{
						OpusError ret = dec.Decoder_Ctl_Set(OpusControlSetRequest.Opus_Reset_State);
						if (ret != OpusError.Ok)
							return ret;
					}
					break;
				}

				case OpusControlSetRequest.Opus_Set_Gain:
				{
					if ((args.Length == 0) || (args[0].GetType() != typeof(opus_int32)))
						return OpusError.Bad_Arg;

					opus_int32 value = (opus_int32)args[0];

					foreach (OpusDecoder dec in st.coupledDecoders.Concat(st.monoDecoders))
					{
						OpusError ret = dec.Decoder_Ctl_Set(request, value);
						if (ret != OpusError.Ok)
							return ret;
					}
					break;
				}

				default:
					return OpusError.Unimplemented;
			}

			return OpusError.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Opus_MultiStream_Decoder_Destroy(OpusMsDecoderInternal st)
		{
			Memory.Opus_Free(st);
		}
	}
}
