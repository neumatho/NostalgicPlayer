/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Celt;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Containers
{
	/// <summary>
	/// Decoder state
	/// </summary>
	internal class OpusCustomDecoder : IClearable, IDeepCloneable<OpusCustomDecoder>
	{
		public OpusCustomMode mode;
		public c_int overlap;
		public c_int channels;
		public c_int stream_channels;

		public c_int downsample;
		public c_int start, end;
		public c_int signalling;
		public bool disable_inv;
		public c_int complexity;
		public c_int arch;

		// Everything beyond this point gets cleared on a reset
		public opus_uint32 rng;
		public bool error;
		public c_int last_pitch_index;
		public c_int loss_duration;
		public bool skip_plc;
		public c_int postfilter_period;
		public c_int postfilter_period_old;
		public opus_val16 postfilter_gain;
		public opus_val16 postfilter_gain_old;
		public c_int postfilter_tapset;
		public c_int postfilter_tapset_old;
		public bool prefilter_and_fold;

		public celt_sig[] preemph_memD = new celt_sig[2];

		// TNE: In the original code, all these arrays are allocated as
		// one big block of memory and stored in decode_mem. The pointers
		// are then calculated. I have decided to use separate arrays instead
		public CPointer<celt_sig> decode_mem;
		public CPointer<opus_val16> lpc;
		public CPointer<opus_val16> oldEBands;
		public CPointer<opus_val16> oldLogE;
		public CPointer<opus_val16> oldLogE2;
		public CPointer<opus_val16> backgroundLogE;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public OpusCustomDecoder(c_int channels)
		{
			CeltMode m = Modes.Opus_Custom_Mode_Create(48000, 960, out _);

			decode_mem = new CPointer<celt_sig>(channels * (Celt_Decoder.Decode_Buffer_Size + m.overlap));
			lpc = new CPointer<opus_val16>(channels * Constants.Celt_Lpc_Order);
			oldEBands = new CPointer<opus_val16>(2 * m.nbEBands);
			oldLogE = new CPointer<opus_val16>(2 * m.nbEBands);
			oldLogE2 = new CPointer<opus_val16>(2 * m.nbEBands);
			backgroundLogE = new CPointer<opus_val16>(2 * m.nbEBands);
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private OpusCustomDecoder()
		{
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			mode = null;
			overlap = 0;
			channels = 0;
			stream_channels = 0;

			downsample = 0;
			start = 0;
			end = 0;
			signalling = 0;
			disable_inv = false;
			complexity = 0;
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
			rng = 0;
			error = false;
			last_pitch_index = 0;
			loss_duration = 0;
			skip_plc = false;
			postfilter_period = 0;
			postfilter_period_old = 0;
			postfilter_gain = 0;
			postfilter_gain_old = 0;
			postfilter_tapset = 0;
			postfilter_tapset_old = 0;
			prefilter_and_fold = false;

			Array.Clear(preemph_memD);

			if (lpc != null)
			{
				lpc.Clear();
				oldEBands.Clear();
				oldLogE.Clear();
				oldLogE2.Clear();
				backgroundLogE.Clear();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Clone the current object into a new one
		/// </summary>
		/********************************************************************/
		public OpusCustomDecoder MakeDeepClone()
		{
			OpusCustomDecoder clone = new OpusCustomDecoder
			{
				mode = mode,
				overlap = overlap,
				channels = channels,
				stream_channels = stream_channels,

				downsample = downsample,
				start = start,
				end = end,
				signalling = signalling,
				disable_inv = disable_inv,
				arch = arch,

				rng = rng,
				error = error,
				last_pitch_index = last_pitch_index,
				loss_duration = loss_duration,
				skip_plc = skip_plc,
				postfilter_period = postfilter_period,
				postfilter_period_old = postfilter_period_old,
				postfilter_gain = postfilter_gain,
				postfilter_gain_old = postfilter_gain_old,
				postfilter_tapset = postfilter_tapset,
				postfilter_tapset_old = postfilter_tapset_old,
				prefilter_and_fold = prefilter_and_fold
			};

			Array.Copy(preemph_memD, clone.preemph_memD, preemph_memD.Length);

			clone.decode_mem = decode_mem.MakeDeepClone();
			clone.lpc = lpc.MakeDeepClone();
			clone.oldEBands = oldEBands.MakeDeepClone();
			clone.oldLogE = oldLogE.MakeDeepClone();
			clone.oldLogE2 = oldLogE2.MakeDeepClone();
			clone.backgroundLogE = backgroundLogE.MakeDeepClone();

			return clone;
		}
	}
}
