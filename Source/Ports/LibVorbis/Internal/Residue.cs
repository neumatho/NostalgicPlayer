/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibOgg;
using Polycode.NostalgicPlayer.Ports.LibVorbis.Containers;
using Polycode.NostalgicPlayer.Ports.LibVorbis.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibVorbis.Internal
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Residue
	{
		private delegate c_long Decode_Del(Codebook book, Pointer<c_float> a, OggPack b, c_int n);

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Free_Info(IVorbisInfoResidue i)
		{
			VorbisInfoResidue0 info = i as VorbisInfoResidue0;

			if (info != null)
				info.Clear();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void Free_Look(IVorbisLookResidue i)
		{
			if (i != null)
			{
				VorbisLookResidue0 look = (VorbisLookResidue0)i;

				for (c_int j = 0; j < look.parts; j++)
				{
					if (look.partbooks[j] != null)
						look.partbooks[j] = null;
				}

				look.partbooks = null;

				for (c_int j = 0; j < look.partvals; j++)
				{
					if (look.decodemap[j] != null)
						look.decodemap[j] = null;
				}

				look.decodemap = null;

				look.Clear();
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int ICount(c_uint v)
		{
			c_int ret = 0;

			while (v != 0)
			{
				ret += (c_int)(v & 1);
				v >>= 1;
			}

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static IVorbisInfoResidue Unpack(VorbisInfo vi, OggPack opb)
		{
			c_int acc = 0;

			VorbisInfoResidue0 info = new VorbisInfoResidue0();
			CodecSetupInfo ci = (CodecSetupInfo)vi.codec_setup;

			info.begin = opb.Read(24);
			info.end = opb.Read(24);
			info.grouping = opb.Read(24) + 1;
			info.partitions = opb.Read(6) + 1;
			info.groupbook = opb.Read(8);

			// Check for premature EOP
			if (info.groupbook < 0)
				goto ErrOut;

			for (c_int j = 0; j < info.partitions; j++)
			{
				c_int cascade = opb.Read(3);
				c_int cflag = opb.Read(1);

				if (cflag < 0)
					goto ErrOut;

				if (cflag != 0)
				{
					c_int c = opb.Read(5);
					if (c < 0)
						goto ErrOut;

					cascade |= (c << 3);
				}

				info.secondstages[j] = cascade;

				acc += ICount((c_uint)cascade);
			}

			for (c_int j = 0; j < acc; j++)
			{
				c_int book = opb.Read(8);
				if (book < 0)
					goto ErrOut;

				info.booklist[j] = book;
			}

			if (info.groupbook >= ci.books)
				goto ErrOut;

			for (c_int j = 0; j < acc; j++)
			{
				if (info.booklist[j] >= ci.books)
					goto ErrOut;

				if (ci.book_param[info.booklist[j]].maptype == 0)
					goto ErrOut;
			}

			// Verify the phrasebook is not specifying an impossible or
			// inconsistent partitioning scheme.
			//
			// Modify the phrasebook ranging check from r16327; an early beta
			// encoder had a bug where it used an oversized phrasebook by
			// accident. These files should continue to be playable, but don't
			// allow an exploit
			{
				c_int entries = ci.book_param[info.groupbook].entries;
				c_int dim = ci.book_param[info.groupbook].dim;
				c_int partvals = 1;

				if (dim < 1)
					goto ErrOut;

				while (dim > 0)
				{
					partvals *= info.partitions;
					if (partvals > entries)
						goto ErrOut;

					dim--;
				}

				info.partvals = partvals;
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
		private static IVorbisLookResidue Look(VorbisDspState vd, IVorbisInfoResidue vr)
		{
			VorbisInfoResidue0 info = (VorbisInfoResidue0)vr;
			VorbisLookResidue0 look = new VorbisLookResidue0();
			CodecSetupInfo ci = (CodecSetupInfo)vd.vi.codec_setup;

			c_int acc = 0;
			c_int maxstage = 0;

			look.info = info;

			look.parts = info.partitions;
			look.fullbooks = ci.fullbooks;
			look.phrasebook = ci.fullbooks[info.groupbook];
			c_int dim = look.phrasebook.dim;

			look.partbooks = new Codebook[look.parts][];

			for (c_int j = 0; j < look.parts; j++)
			{
				c_int stages = Sharedbook.Ov_ILog((ogg_uint32_t)info.secondstages[j]);

				if (stages != 0)
				{
					if (stages > maxstage)
						maxstage = stages;

					look.partbooks[j] = new Codebook[stages];

					for (c_int k = 0; k < stages; k++)
					{
						if ((info.secondstages[j] & (1 << k)) != 0)
							look.partbooks[j][k] = ci.fullbooks[info.booklist[acc++]];
					}
				}
			}

			look.partvals = 1;

			for (c_int j = 0; j < dim; j++)
				look.partvals *= look.parts;

			look.stages = maxstage;
			look.decodemap = new c_int[look.partvals][];

			for (c_int j = 0; j < look.partvals; j++)
			{
				c_long val = j;
				c_long mult = look.partvals / look.parts;

				look.decodemap[j] = new c_int[dim];

				for (c_int k = 0; k < dim; k++)
				{
					c_long deco = val / mult;
					val -= deco * mult;
					mult /= look.parts;

					look.decodemap[j][k] = deco;
				}
			}

			return look;
		}



		/********************************************************************/
		/// <summary>
		/// A truncated packet here just means 'stop working'; it's not an
		/// error
		/// </summary>
		/********************************************************************/
		private static c_int _01inverse(VorbisBlock vb, IVorbisLookResidue vl, Pointer<c_float>[] @in, c_int ch, Decode_Del decodepart)
		{
			VorbisLookResidue0 look = (VorbisLookResidue0)vl;
			VorbisInfoResidue0 info = look.info;

			// Move all this setup out later
			c_int samples_per_partition = info.grouping;
			c_int partitions_per_word = look.phrasebook.dim;
			c_int max = vb.pcmend >> 1;
			c_int end = info.end < max ? info.end : max;
			c_int n = end - info.begin;

			if (n > 0)
			{
				c_int partvals = n / samples_per_partition;
				c_int partwords = (partvals + partitions_per_word - 1) / partitions_per_word;
				Pointer<c_int[]>[] partword = new Pointer<c_int[]>[ch];

				for (c_long j = 0; j < ch; j++)
					partword[j] = Block.Vorbis_Block_Alloc<c_int[]>(vb, partwords);

				for (c_long s = 0; s < look.stages; s++)
				{
					// Each loop decodes on partition codeword containing
					// partitions_per_word partitions
					for (c_long i = 0, l = 0; i < partvals; l++)
					{
						if (s == 0)
						{
							// Fetch the partition word for each channel
							for (c_long j = 0; j < ch; j++)
							{
								c_int temp = CodebookImpl.Vorbis_Book_Decode(look.phrasebook, vb.opb);

								if ((temp == -1) || (temp >= info.partvals))
									goto EopBreak;

								partword[j][l] = look.decodemap[temp];

								if (partword[j][l] == null)
									goto ErrOut;
							}
						}

						// Now we decode residual values for the partitions
						for (c_long k = 0; (k < partitions_per_word) && (i < partvals); k++, i++)
						{
							for (c_long j = 0; j < ch; j++)
							{
								c_long offset = info.begin + i * samples_per_partition;

								if ((info.secondstages[partword[j][l][k]] & (1 << s)) != 0)
								{
									Codebook stagebook = look.partbooks[partword[j][l][k]][s];

									if (stagebook != null)
									{
										if (decodepart(stagebook, @in[j] + offset, vb.opb, samples_per_partition) == -1)
											goto EopBreak;
									}
								}
							}
						}
					}
				}
			}

			ErrOut:
			EopBreak:
			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Res0_Inverse(VorbisBlock vb, IVorbisLookResidue vl, Pointer<c_float>[] @in, Pointer<bool> nonzero, c_int ch)
		{
			c_int used = 0;

			for (c_int i = 0; i < ch; i++)
			{
				if (nonzero[i])
					@in[used++] = @in[i];
			}

			if (used != 0)
				return _01inverse(vb, vl, @in, used, CodebookImpl.Vorbis_Book_Decodevs_Add);
			else
				return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Res1_Inverse(VorbisBlock vb, IVorbisLookResidue vl, Pointer<c_float>[] @in, Pointer<bool> nonzero, c_int ch)
		{
			c_int used = 0;

			for (c_int i = 0; i < ch; i++)
			{
				if (nonzero[i])
					@in[used++] = @in[i];
			}

			if (used != 0)
				return _01inverse(vb, vl, @in, used, CodebookImpl.Vorbis_Book_Decodev_Add);
			else
				return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Duplicate code here as speed is somewhat more important
		/// </summary>
		/********************************************************************/
		private static c_int Res2_Inverse(VorbisBlock vb, IVorbisLookResidue vl, Pointer<c_float>[] @in, Pointer<bool> nonzero, c_int ch)
		{
			VorbisLookResidue0 look = (VorbisLookResidue0)vl;
			VorbisInfoResidue0 info = look.info;

			// Move all this setup out later
			c_int samples_per_partition = info.grouping;
			c_int partitions_per_word = look.phrasebook.dim;
			c_int max = (vb.pcmend * ch) >> 1;
			c_int end = info.end < max ? info.end : max;
			c_int n = end - info.begin;

			if (n > 0)
			{
				c_int partvals = n / samples_per_partition;
				c_int partwords = (partvals + partitions_per_word - 1) / partitions_per_word;
				Pointer<c_int[]> partword = Block.Vorbis_Block_Alloc<c_int[]>(vb, partwords);
				c_long i, l;

				for (i = 0; i < ch; i++)
				{
					if (nonzero[i])
						break;
				}

				if (i == ch)
					return 0;	// No nonzero vectors

				for (c_long s = 0; s < look.stages; s++)
				{
					// Each loop decodes on partition codeword containing
					// partitions_per_word partitions
					for (i = 0, l = 0; i < partvals; l++)
					{
						if (s == 0)
						{
							// Fetch the partition word for each channel
							c_int temp = CodebookImpl.Vorbis_Book_Decode(look.phrasebook, vb.opb);

							if ((temp == -1) || (temp >= info.partvals))
								goto EopBreak;

							partword[l] = look.decodemap[temp];

							if (partword[l] == null)
								goto ErrOut;
						}

						// Now we decode residual values for the partitions
						for (c_long k = 0; (k < partitions_per_word) && (i < partvals); k++, i++)
						{
							if ((info.secondstages[partword[l][k]] & (1 << s)) != 0)
							{
								Codebook stagebook = look.partbooks[partword[l][k]][s];

								if (stagebook != null)
								{
									if (CodebookImpl.Vorbis_Book_Decodevv_Add(stagebook, @in, i * samples_per_partition + info.begin, ch, vb.opb, samples_per_partition) == -1)
										goto EopBreak;
								}
							}
						}
					}
				}
			}

			ErrOut:
			EopBreak:
			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly VorbisFuncResidue Residue0_ExportBundle = new VorbisFuncResidue
		{
			Unpack = Unpack,
			Look = Look,
			Free_Info = Free_Info,
			Free_Look = Free_Look,
			Inverse = Res0_Inverse
		};



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly VorbisFuncResidue Residue1_ExportBundle = new VorbisFuncResidue
		{
			Unpack = Unpack,
			Look = Look,
			Free_Info = Free_Info,
			Free_Look = Free_Look,
			Inverse = Res1_Inverse
		};



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static readonly VorbisFuncResidue Residue2_ExportBundle = new VorbisFuncResidue
		{
			Unpack = Unpack,
			Look = Look,
			Free_Info = Free_Info,
			Free_Look = Free_Look,
			Inverse = Res2_Inverse
		};
	}
}
