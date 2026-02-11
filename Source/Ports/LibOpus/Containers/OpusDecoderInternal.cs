/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class OpusDecoderInternal : IClearable, IDeepCloneable<OpusDecoderInternal>
	{
		public CeltDecoder celt_dec;
		public Silk_Decoder silk_dec;
		public c_int channels;

		/// <summary>
		/// Sampling rate (at the API level)
		/// </summary>
		public opus_int32 Fs;

		public Silk_DecControlStruct DecControl = new Silk_DecControlStruct();
		public c_int decode_gain;
		public c_int complexity;
		public c_int ignore_extensions;
		public c_int arch;

		// Everything beyond this point gets cleared on a reset
		public c_int stream_channels;

		public Bandwidth bandwidth;
		public PacketMode mode;
		public PacketMode prev_mode;
		public c_int frame_size;
		public bool prev_redundancy;
		public c_int last_packet_duration;
		public readonly opus_val16[] softclip_mem = new opus_val16[2];

		public opus_uint32 rangeFinal;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			celt_dec?.Clear();
			silk_dec?.Clear();
			channels = 0;

			Fs = 0;

			DecControl.Clear();
			decode_gain = 0;
			complexity = 0;
			ignore_extensions = 0;
			arch = 0;

			Reset();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			stream_channels = 0;

			bandwidth = Bandwidth.None;
			mode = PacketMode.None;
			prev_mode = PacketMode.None;
			frame_size = 0;
			prev_redundancy = false;
			last_packet_duration = 0;
			Array.Clear(softclip_mem);

			rangeFinal = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Clone the current object into a new one
		/// </summary>
		/********************************************************************/
		public OpusDecoderInternal MakeDeepClone()
		{
			OpusDecoderInternal clone = new OpusDecoderInternal
			{
				celt_dec = celt_dec?.MakeDeepClone(),
				silk_dec = silk_dec?.MakeDeepClone(),
				channels = channels,

				Fs = Fs,

				DecControl = DecControl.MakeDeepClone(),
				decode_gain = decode_gain,
				complexity = complexity,
				ignore_extensions = ignore_extensions,
				arch = arch,

				stream_channels = stream_channels,

				bandwidth = bandwidth,
				mode = mode,
				prev_mode = prev_mode,
				frame_size = frame_size,
				prev_redundancy = prev_redundancy,
				last_packet_duration = last_packet_duration,

				rangeFinal = rangeFinal
			};

			Array.Copy(softclip_mem, clone.softclip_mem, softclip_mem.Length);

			return clone;
		}
	}
}
