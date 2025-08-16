/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOgg;
using Polycode.NostalgicPlayer.Ports.LibVorbis.Containers;
using Polycode.NostalgicPlayer.Ports.LibVorbis.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibVorbis.Internal
{
	internal static class Floor0
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Free_Info(IVorbisInfoFloor i)
		{
			VorbisInfoFloor0 info = i as VorbisInfoFloor0;

			if (info != null)
				info.Clear();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Free_Look(IVorbisLookFloor i)
		{
			VorbisLookFloor0 look = i as VorbisLookFloor0;

			if (look != null)
			{
				if (look.linearmap != null)
				{
					if (look.linearmap[0] != null)
						look.linearmap[0] = null;

					if (look.linearmap[1] != null)
						look.linearmap[1] = null;

					look.linearmap = null;
				}

				look.Clear();
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static IVorbisInfoFloor Unpack(VorbisInfo vi, OggPack opb)
		{
			CodecSetupInfo ci = (CodecSetupInfo)vi.codec_setup;

			VorbisInfoFloor0 info = new VorbisInfoFloor0();
			info.order = opb.Read(8);
			info.rate = opb.Read(16);
			info.barkmap = opb.Read(16);
			info.ampbits = opb.Read(6);
			info.ampdB = opb.Read(8);
			info.numbooks = opb.Read(4) + 1;

			if (info.order < 1)
				goto ErrOut;

			if (info.rate < 1)
				goto ErrOut;

			if (info.barkmap < 1)
				goto ErrOut;

			if (info.numbooks < 1)
				goto ErrOut;

			for (c_int j = 0; j < info.numbooks; j++)
			{
				info.books[j] = opb.Read(8);

				if ((info.books[j] < 0) || (info.books[j] >= ci.books))
					goto ErrOut;

				if (ci.book_param[info.books[j]].maptype == 0)
					goto ErrOut;

				if (ci.book_param[info.books[j]].dim < 1)
					goto ErrOut;
			}

			return info;

			ErrOut:
			Free_Info(info);

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize Bark scale and normalization lookups. We could do this
		/// with static tables, but Vorbis allows a number of possible
		/// combinations, so it's best to do it computationally.
		///
		/// The below is authoritative in terms of defining scale mapping.
		/// Note that the scale depends on the sampling rate as well as the
		/// linear block and mapping sizes
		/// </summary>
		/********************************************************************/
		private static void Map_Lazy_Init(VorbisBlock vb, IVorbisInfoFloor infoX, VorbisLookFloor0 look)
		{
			if (look.linearmap[vb.W] == null)
			{
				VorbisDspState vd = vb.vd;
				VorbisInfo vi = vd.vi;
				CodecSetupInfo ci = (CodecSetupInfo)vi.codec_setup;
				VorbisInfoFloor0 info = (VorbisInfoFloor0)infoX;

				c_int W = vb.W;
				c_int n = ci.blocksizes[W] / 2;

				// we choose a scaling constant so that:
				//   floor(bark(rate/2-1)*C)=mapped-1
				// floor(bark(rate/2)*C)=mapped
				c_float scale = look.ln / Scales.ToBark(info.rate / 2.0f);

				// The mapping from a linear scale to a smaller bark scale is
				// straightforward. We do *not* make sure that the linear mapping
				// does not skip bark-scale bins; the decoder simply skips them and
				// the encoder may do what it wishes in filling them. They're
				// necessary in some mapping combinations to keep the scale spacing
				// accurate
				look.linearmap[W] = new c_int[n + 1];

				c_int j;
				for (j = 0; j < n; j++)
				{
					c_int val = (c_int)Math.Floor(Scales.ToBark((info.rate / 2.0f) / n * j) * scale);	// Bark numbers represent band edges

					if (val >= look.ln)
						val = look.ln - 1;	// Guard against the approximation

					look.linearmap[W][j] = val;
				}

				look.linearmap[W][j] = -1;
				look.n[W] = n;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static IVorbisLookFloor Look(VorbisDspState vd, IVorbisInfoFloor i)
		{
			VorbisInfoFloor0 info = (VorbisInfoFloor0)i;
			VorbisLookFloor0 look = new VorbisLookFloor0();

			look.m = info.order;
			look.ln = info.barkmap;
			look.vi = info;

			look.linearmap = new c_int[2][];

			return look;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static CPointer<byte> Inverse1(VorbisBlock vb, IVorbisLookFloor i)
		{
			VorbisLookFloor0 look = (VorbisLookFloor0)i;
			VorbisInfoFloor0 info = look.vi;

			c_int ampraw = vb.opb.Read(info.ampbits);

			if (ampraw > 0)	// Also handles the -1 out of data case
			{
				c_long maxval = (1 << info.ampbits) - 1;
				c_float amp = (c_float)ampraw / maxval * info.ampdB;
				c_int booknum = vb.opb.Read(Sharedbook.Ov_ILog((ogg_uint32_t)info.numbooks));

				if ((booknum != -1) && (booknum < info.numbooks))	// Be paranoid
				{
					CodecSetupInfo ci = (CodecSetupInfo)vb.vd.vi.codec_setup;
					Codebook b = ci.fullbooks[info.books[booknum]];
					c_float last = 0.0f;

					// The additional b->dim is a guard against any possible stack
					// smash; b->dim is provably more than we can overflow the
					// vector
					CPointer<byte> retBuffer = Block.Vorbis_Block_Alloc<byte>(vb, sizeof(c_float) * (look.m + b.dim + 1));
					Span<c_float> lsp = MemoryMarshal.Cast<byte, c_float>(retBuffer.AsSpan());

					if (CodebookImpl.Vorbis_Book_Decodev_Set(b, lsp, vb.opb, look.m) == -1)
						goto Eop;

					for (c_int j = 0; j < look.m;)
					{
						for (c_int k = 0; (j < look.m) && (k < b.dim); k++, j++)
							lsp[j] += last;

						last = lsp[j - 1];
					}

					lsp[look.m] = amp;

					return retBuffer;
				}
			}

			Eop:
			return null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Inverse2(VorbisBlock vb, IVorbisLookFloor i, CPointer<byte> memo, CPointer<c_float> @out)
		{
			VorbisLookFloor0 look = (VorbisLookFloor0)i;
			VorbisInfoFloor0 info = look.vi;

			Map_Lazy_Init(vb, info, look);

			if (memo.IsNotNull)
			{
				Span<c_float> lsp = MemoryMarshal.Cast<byte, c_float>(memo.AsSpan());
				c_float amp = lsp[look.m];

				// Take the coefficients back to a spectral envelope curve
				Lsp.Vorbis_Lsp_To_Curve(@out, look.linearmap[vb.W], look.n[vb.W], look.ln, lsp, look.m, amp, info.ampdB);

				return 1;
			}

			@out.Clear(look.n[vb.W]);

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly VorbisFuncFloor ExportBundle = new VorbisFuncFloor
		{
			Unpack = Unpack,
			Look = Look,
			Free_Info = Free_Info,
			Free_Look = Free_Look,
			Inverse1 = Inverse1,
			Inverse2 = Inverse2
		};
	}
}
