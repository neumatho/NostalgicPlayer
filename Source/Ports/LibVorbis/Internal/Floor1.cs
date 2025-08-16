/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOgg;
using Polycode.NostalgicPlayer.Ports.LibVorbis.Containers;
using Polycode.NostalgicPlayer.Ports.LibVorbis.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibVorbis.Internal
{
	internal static class Floor1
	{
		private class IComp : IComparer<CPointer<c_int>>
		{
			public int Compare(CPointer<c_int> a, CPointer<c_int> b)
			{
				return a[0] - b[0];
			}
		}

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Free_Info(IVorbisInfoFloor i)
		{
			VorbisInfoFloor1 info = i as VorbisInfoFloor1;

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
			VorbisLookFloor1 look = i as VorbisLookFloor1;

			if (look != null)
				look.Clear();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static IVorbisInfoFloor Unpack(VorbisInfo vi, OggPack opb)
		{
			CodecSetupInfo ci = (CodecSetupInfo)vi.codec_setup;
			c_int count = 0, maxclass = -1;

			VorbisInfoFloor1 info = new VorbisInfoFloor1();

			// Read partitions
			info.partitions = opb.Read(5);		// Only 0 to 31 legal

			for (c_int j = 0; j < info.partitions; j++)
			{
				info.partitionclass[j] = opb.Read(4);	// Only 0 to 15 legal

				if (info.partitionclass[j] < 0)
					goto ErrOut;

				if (maxclass < info.partitionclass[j])
					maxclass = info.partitionclass[j];
			}

			// Read partition classes
			for (c_int j = 0; j < (maxclass + 1); j++)
			{
				info.class_dim[j] = opb.Read(3) + 1;	// 1 to 8
				info.class_subs[j] = opb.Read(2);		// 0,1,2,3 bits

				if (info.class_subs[j] < 0)
					goto ErrOut;

				if (info.class_subs[j] != 0)
					info.class_book[j] = opb.Read(8);

				if ((info.class_book[j] < 0) || (info.class_book[j] >= ci.books))
					goto ErrOut;

				for (c_int k = 0; k < (1 << info.class_subs[j]); k++)
				{
					info.class_subbook[j, k] = opb.Read(8) - 1;

					if ((info.class_subbook[j, k] < -1) || (info.class_subbook[j, k] >= ci.books))
						goto ErrOut;
				}
			}

			// Read the post list
			info.mult = opb.Read(2) + 1;		// Only 1,2,3,4 legal now
			c_int rangebits = opb.Read(4);
			if (rangebits < 0)
				goto ErrOut;

			for (c_int j = 0, k = 0; j < info.partitions; j++)
			{
				count += info.class_dim[info.partitionclass[j]];
				if (count > Constants.Vif_Posit)
					goto ErrOut;

				for (; k < count; k++)
				{
					c_int t = info.postlist[k + 2] = opb.Read(rangebits);
					if ((t < 0) || (t >= (1 << rangebits)))
						goto ErrOut;
				}
			}

			info.postlist[0] = 0;
			info.postlist[1] = 1 << rangebits;

			// Don't allow repeated values in post list as they'd result in
			// zero-length segments
			{
				CPointer<c_int>[] sortpointer = new CPointer<c_int>[Constants.Vif_Posit + 2];

				for (c_int j = 0; j < (count + 2); j++)
					sortpointer[j] = new CPointer<c_int>(info.postlist, j);

				Array.Sort(sortpointer, 0, count + 2, new IComp());

				for (c_int j = 1; j < (count + 2); j++)
				{
					if (sortpointer[j - 1] == sortpointer[j])
						goto ErrOut;
				}
			}

			return info;

			ErrOut:
			Free_Info(info);

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static IVorbisLookFloor Look(VorbisDspState vd, IVorbisInfoFloor @in)
		{
			CPointer<c_int>[] sortpointer = new CPointer<c_int>[Constants.Vif_Posit + 2];
			VorbisInfoFloor1 info = (VorbisInfoFloor1)@in;
			VorbisLookFloor1 look = new VorbisLookFloor1();
			c_int n = 0;

			look.vi = info;
			look.n = info.postlist[1];

			// we drop each position value in-between already decoded values,
			// and use linear interpolation to predict each new value past the
			// edges. The positions are read in the order of the position
			// list... we precompute the bounding positions in the lookup. Of
			// course, the neighbors can change (if a position is declined), but
			// this is an initial mapping
			for (c_int i = 0; i < info.partitions; i++)
				n += info.class_dim[info.partitionclass[i]];

			n += 2;
			look.posts = n;

			// Also store a sorted position index
			for (c_int i = 0; i < n; i++)
				sortpointer[i] = new CPointer<c_int>(info.postlist, i);

			Array.Sort(sortpointer, 0, n, new IComp());

			// Points from sort order back to range number
			for (c_int i = 0; i < n; i++)
				look.forward_index[i] = sortpointer[i] - info.postlist;

			// Points from range order to sorted position
			for (c_int i = 0; i < n; i++)
				look.reverse_index[look.forward_index[i]] = i;

			// We actually need the post values too
			for (c_int i = 0; i < n; i++)
				look.sorted_index[i] = info.postlist[look.forward_index[i]];

			// Quantize values to multiplier spec
			switch (info.mult)
			{
				case 1:	// 1024 -> 256
				{
					look.quant_q = 256;
					break;
				}

				case 2:	// 1024 -> 128
				{
					look.quant_q = 128;
					break;
				}

				case 3:	// 1024 -> 86
				{
					look.quant_q = 86;
					break;
				}

				case 4:	// 1024 -> 64
				{
					look.quant_q = 64;
					break;
				}
			}

			// Discover our neighbors for decode where we don't use fit flags
			// (that would push the neighbors outward)
			for (c_int i = 0; i < (n - 2); i++)
			{
				c_int lo = 0;
				c_int hi = 1;
				c_int lx = 0;
				c_int hx = look.n;
				c_int currentx = info.postlist[i + 2];

				for (c_int j = 0; j < (i + 2); j++)
				{
					c_int x = info.postlist[j];

					if ((x > lx) && (x < currentx))
					{
						lo = j;
						lx = x;
					}

					if ((x < hx) && (x > currentx))
					{
						hi = j;
						hx = x;
					}
				}

				look.loneighbor[i] = lo;
				look.hineighbor[i] = hi;
			}

			return look;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Render_Point(c_int x0, c_int x1, c_int y0, c_int y1, c_int x)
		{
			y0 &= 0x7fff;	// Mask off flag
			y1 &= 0x7fff;

			{
				c_int dy = y1 - y0;
				c_int adx = x1 - x0;
				c_int ady = Math.Abs(dy);
				c_int err = ady * (x - x0);

				c_int off = err / adx;

				if (dy < 0)
					return y0 - off;

				return y0 + off;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Render_Line(c_int n, c_int x0, c_int x1, c_int y0, c_int y1, CPointer<c_float> d)
		{
			c_int dy = y1 - y0;
			c_int adx = x1 - x0;
			c_int ady = Math.Abs(dy);
			c_int @base = dy / adx;
			c_int sy = dy < 0 ? @base - 1 : @base + 1;
			c_int x = x0;
			c_int y = y0;
			c_int err = 0;

			ady -= Math.Abs(@base * adx);

			if (n > x1)
				n = x1;

			if (x < n)
				d[x] *= Tables.Floor1_FromdB_Lookup[y];

			while (++x < n)
			{
				err = err + ady;

				if (err >= adx)
				{
					err -= adx;
					y += sy;
				}
				else
					y += @base;

				d[x] *= Tables.Floor1_FromdB_Lookup[y];
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static CPointer<byte> Inverse1(VorbisBlock vb, IVorbisLookFloor @in)
		{
			VorbisLookFloor1 look = (VorbisLookFloor1)@in;
			VorbisInfoFloor1 info = look.vi;
			CodecSetupInfo ci = (CodecSetupInfo)vb.vd.vi.codec_setup;

			Codebook[] books = ci.fullbooks;

			// Unpack wrapped/predicted values from stream
			if (vb.opb.Read(1) == 1)
			{
				CPointer<byte> retBuffer = Block.Vorbis_Block_Alloc<byte>(vb, look.posts * sizeof(c_int));
				Span<c_int> fit_value = MemoryMarshal.Cast<byte, c_int>(retBuffer.AsSpan());

				fit_value[0] = vb.opb.Read(Sharedbook.Ov_ILog((ogg_uint32_t)look.quant_q - 1));
				fit_value[1] = vb.opb.Read(Sharedbook.Ov_ILog((ogg_uint32_t)look.quant_q - 1));

				// Partition by partition
				for (c_int i = 0, j = 2; i < info.partitions; i++)
				{
					c_int @class = info.partitionclass[i];
					c_int cdim = info.class_dim[@class];
					c_int csubbits = info.class_subs[@class];
					c_int csub = 1 << csubbits;
					c_int cval = 0;

					// Decode the partition's first stage cascade value
					if (csubbits != 0)
					{
						cval = CodebookImpl.Vorbis_Book_Decode(books[info.class_book[@class]], vb.opb);

						if (cval == -1)
							goto Eop;
					}

					for (c_int k = 0; k < cdim; k++)
					{
						c_int book = info.class_subbook[@class, cval & (csub - 1)];
						cval >>= csubbits;

						if (book >= 0)
						{
							fit_value[j + k] = CodebookImpl.Vorbis_Book_Decode(books[book], vb.opb);
							if (fit_value[j + k] == -1)
								goto Eop;
						}
						else
							fit_value[j + k] = 0;
					}

					j += cdim;
				}

				// Unwrap positive values and reconsitute via linear interpolation
				for (c_int i = 2; i < look.posts; i++)
				{
					c_int predicted = Render_Point(info.postlist[look.loneighbor[i - 2]], info.postlist[look.hineighbor[i - 2]], fit_value[look.loneighbor[i - 2]], fit_value[look.hineighbor[i - 2]], info.postlist[i]);
					c_int hiroom = look.quant_q - predicted;
					c_int loroom = predicted;
					c_int room = (hiroom < loroom ? hiroom : loroom) << 1;
					c_int val = fit_value[i];

					if (val != 0)
					{
						if (val >= room)
						{
							if (hiroom > loroom)
								val = val - loroom;
							else
								val = -1 - (val - hiroom);
						}
						else
						{
							if ((val & 1) != 0)
								val = -((val + 1) >> 1);
							else
								val >>= 1;
						}

						fit_value[i] = (val + predicted) & 0x7fff;
						fit_value[look.loneighbor[i - 2]] &= 0x7fff;
						fit_value[look.hineighbor[i - 2]] &= 0x7fff;
					}
					else
						fit_value[i] = predicted | 0x8000;
				}

				return retBuffer;
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
			VorbisLookFloor1 look = (VorbisLookFloor1)i;
			VorbisInfoFloor1 info = look.vi;

			CodecSetupInfo ci = (CodecSetupInfo)vb.vd.vi.codec_setup;
			c_int n = ci.blocksizes[vb.W] / 2;

			if (memo.IsNotNull)
			{
				// Render the lines
				Span<c_int> fit_value = MemoryMarshal.Cast<byte, c_int>(memo.AsSpan());
				c_int hx = 0;
				c_int lx = 0;
				c_int ly = fit_value[0] * info.mult;

				// Guard lookup against out-of-range values
				ly = ly < 0 ? 0 : ly > 255 ? 255 : ly;

				for (c_int j = 1; j < look.posts; j++)
				{
					c_int current = look.forward_index[j];
					c_int hy = fit_value[current] & 0x7fff;

					if (hy == fit_value[current])
					{
						hx = info.postlist[current];
						hy *= info.mult;

						// Guard lookup against out-of-range values
						hy = hy < 0 ? 0 : hy > 255 ? 255 : hy;

						Render_Line(n, lx, hx, ly, hy, @out);

						lx = hx;
						ly = hy;
					}
				}

				for (c_int j = hx; j < n; j++)
					@out[j] *= Tables.Floor1_FromdB_Lookup[ly];	// Be certain

				return 1;
			}

			@out.Clear(n);

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
