/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOgg;
using Polycode.NostalgicPlayer.Ports.LibVorbis.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibVorbis.Internal
{
	/// <summary>
	/// 
	/// </summary>
	internal static class CodebookImpl
	{
		/********************************************************************/
		/// <summary>
		/// Unpacks a codebook from the packet buffer into the codebook
		/// struct, readies the codebook auxiliary structures for decode
		/// </summary>
		/********************************************************************/
		public static StaticCodebook Vorbis_Staticbook_Unpack(OggPack opb)
		{
			StaticCodebook s = new StaticCodebook();
			s.allocedp = true;

			// Make sure alignment is correct
			if (opb.Read(24) != 0x564342)
				goto Eofout;

			// First the basic parameters
			s.dim = opb.Read(16);
			s.entries = opb.Read(24);
			if (s.entries == -1)
				goto Eofout;

			if ((Sharedbook.Ov_ILog((ogg_uint32_t)s.dim) + Sharedbook.Ov_ILog((ogg_uint32_t)s.entries)) > 24)
				goto Eofout;

			// Codeword ordering.... length ordered or unordered?
			switch (opb.Read(1))
			{
				case 0:
				{
					// Allocated but unused entries?
					c_long unused = opb.Read(1);

					if (((s.entries * (unused != 0 ? 1 : 5) + 7) >> 3) > opb.Buffer.Storage - opb.Bytes())
						goto Eofout;

					// Unordered
					s.lengthlist = new byte[s.entries];

					// Allocated but unused entries?
					if (unused != 0)
					{
						// Yes, unused entries
						for (c_long i = 0; i < s.entries; i++)
						{
							if (opb.Read(1) != 0)
							{
								c_long num = opb.Read(5);
								if (num == -1)
									goto Eofout;

								s.lengthlist[i] = (byte)(num + 1);
							}
							else
								s.lengthlist[i] = 0;
						}
					}
					else
					{
						// All entries used; no tagging
						for (c_long i = 0; i < s.entries; i++)
						{
							c_long num = opb.Read(5);
							if (num == -1)
								goto Eofout;

							s.lengthlist[i] = (byte)(num + 1);
						}
					}

					break;
				}

				case 1:
				{
					// Ordered
					c_long length = opb.Read(5) + 1;
					if (length == 0)
						goto Eofout;

					s.lengthlist = new byte[s.entries];

					for (c_long i = 0; i < s.entries;)
					{
						c_long num = opb.Read(Sharedbook.Ov_ILog((ogg_uint32_t)(s.entries - i)));
						if (num == -1)
							goto Eofout;

						if ((length > 32) || (num > (s.entries - i)) || ((num > 0) && (((num - 1) >> (c_int)(length - 1)) > 1)))
							goto Errout;

						if (length > 32)
							goto Errout;

						for (c_long j = 0; j < num; j++, i++)
							s.lengthlist[i] = (byte)length;

						length++;
					}

					break;
				}

				default:
				{
					// EOF
					goto Eofout;
				}
			}

			// Do we have a mapping to unpack?
			s.maptype = (c_int)opb.Read(4);

			switch (s.maptype)
			{
				case 0:
				{
					// No mapping
					break;
				}

				case 1:	// Implicitly populated value mapping
				case 2:	// Explicitly populated value mapping
				{
					s.q_min = opb.Read(32);
					s.q_delta = opb.Read(32);
					s.q_quant = (c_int)opb.Read(4) + 1;
					s.q_sequencep = (c_int)opb.Read(1);

					if (s.q_sequencep == -1)
						goto Eofout;

					{
						c_int quantvals = 0;

						switch (s.maptype)
						{
							case 1:
							{
								quantvals = (s.dim == 0 ? 0 : (c_int)Sharedbook.Book_Maptype1_Quantvals(s));
								break;
							}

							case 2:
							{
								quantvals = (c_int)(s.entries * s.dim);
								break;
							}
						}

						// Quantized values
						if (((quantvals * s.q_quant + 7) >> 3) > (opb.Buffer.Storage - opb.Bytes()))
							goto Eofout;

						s.quantlist = new c_long[quantvals];

						for (c_long i = 0; i < quantvals; i++)
							s.quantlist[i] = opb.Read(s.q_quant);

						if ((quantvals != 0) && (s.quantlist[quantvals - 1] == -1))
							goto Eofout;
					}

					break;
				}

				default:
					goto Errout;
			}

			// All set
			return s;

			Errout:
			Eofout:
			Sharedbook.Vorbis_Staticbook_Destroy(s);

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// The 'eliminate the decode tree' optimization actually requires
		/// the codewords to be MSb first, not LSb. This is an annoying
		/// inelegancy (and one of the first places where carefully thought
		/// out design turned out to be wrong; Vorbis II and future Ogg
		/// codecs should go to an MSb bitpacker), but not actually the huge
		/// hit it appears to be. The first-stage decode table catches most
		/// words so that bitreverse is not in the main execution path
		/// </summary>
		/********************************************************************/
		private static ogg_uint32_t Bitreverse(ogg_uint32_t x)
		{
			x = ((x >> 16) & 0x0000ffff) | ((x << 16) & 0xffff0000);
			x = ((x >> 8) & 0x00ff00ff) | ((x << 8) & 0xff00ff00);
			x = ((x >> 4) & 0x0f0f0f0f) | ((x << 4) & 0xf0f0f0f0);
			x = ((x >> 2) & 0x33333333) | ((x << 2) & 0xcccccccc);
			return ((x >> 1) & 0x55555555) | ((x << 1) & 0xaaaaaaaa);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static c_long Decode_Packed_Entry_Number(Codebook book, OggPack b)
		{
			c_int read = book.dec_maxlength;
			c_long lo, hi;
			c_long lok = b.Look(book.dec_firsttablen);

			if (lok >= 0)
			{
				ogg_uint32_t entry = book.dec_firsttable[lok];

				if ((entry & 0x80000000UL) != 0)
				{
					lo = (c_long)((entry >> 15) & 0x7fff);
					hi = (c_long)(book.used_entries - (entry & 0x7fff));
				}
				else
				{
					b.Adv(book.dec_codelengths[entry - 1]);

					return (c_long)entry - 1;
				}
			}
			else
			{
				lo = 0;
				hi = book.used_entries;
			}

			// Single entry codebooks use a firsttablen of 1 and a
			// dec_maxlength of 1. If a single-entry codebook gets here (due to
			// failure to read one bit above), the next look attempt will also
			// fail and we'll correctly kick out instead of trying to walk the
			// underformed tree
			lok = b.Look(read);

			while ((lok < 0) && (read > 1))
				lok = b.Look(--read);

			if (lok < 0)
				return -1;

			// Bisect search for the codeword in the ordered list
			{
				ogg_uint32_t testword = Bitreverse((ogg_uint32_t)lok);

				while ((hi - lo) > 1)
				{
					c_long p = (hi - lo) >> 1;
					c_long test = book.codelist[lo + p] > testword ? 1 : 0;

					lo += p & (test - 1);
					hi -= p & (-test);
				}

				if (book.dec_codelengths[lo] <= read)
				{
					b.Adv(book.dec_codelengths[lo]);

					return lo;
				}
			}

			b.Adv(read);

			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// Decode side is specced and easier, because we don't need to find
		/// matches using different criteria; we simply read and map. There
		/// are two things we need to do 'depending':
		///
		/// We may need to support interleave. We don't really, but it's
		/// convenient to do it here rather than rebuild the vector later.
		///
		/// Cascades may be additive or multiplicitive; this is not inherent
		/// in the codebook, but set in the code using the codebook. Like
		/// interleaving, it's easiest to do it here.
		/// addmul==0 -> declarative (set the value)
		/// addmul==1 -> additive
		/// addmul==2 -> multiplicitive
		///
		/// Returns the [original, not compacted] entry number or -1 on eof
		/// </summary>
		/********************************************************************/
		public static c_long Vorbis_Book_Decode(Codebook book, OggPack b)
		{
			if (book.used_entries > 0)
			{
				c_long packed_entry = Decode_Packed_Entry_Number(book, b);

				if (packed_entry >= 0)
					return book.dec_index[packed_entry];
			}

			// If there's no dec_index, the codebook unpacking isn't collapsed
			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// Returns 0 on OK or -1 on eof
		/// Decode vector / dim granularity guarding is done in the upper
		/// layer
		/// </summary>
		/********************************************************************/
		public static c_long Vorbis_Book_Decodevs_Add(Codebook book, CPointer<c_float> a, OggPack b, c_int n)
		{
			if (book.used_entries > 0)
			{
				c_int step = (c_int)(n / book.dim);
				c_long[] entry = new c_long[step];
				CPointer<c_float>[] t = new CPointer<c_float>[step];

				for (c_int i = 0; i < step; i++)
				{
					entry[i] = Decode_Packed_Entry_Number(book, b);

					if (entry[i] == -1)
						return -1;

					t[i] = book.valuelist + entry[i] * book.dim;
				}

				for (c_int i = 0, o = 0; i < book.dim; i++, o += step)
				{
					for (c_int j = 0; ((o + j) < n) && (j < step); j++)
						a[o + j] += t[j][i];
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Decode vector / dim granularity guarding is done in the upper
		/// layer
		/// </summary>
		/********************************************************************/
		public static c_long Vorbis_Book_Decodev_Add(Codebook book, CPointer<c_float> a, OggPack b, c_int n)
		{
			if (book.used_entries > 0)
			{
				for (c_int i = 0; i < n;)
				{
					c_int entry = (c_int)Decode_Packed_Entry_Number(book, b);

					if (entry == -1)
						return -1;

					CPointer<c_float> t = book.valuelist + entry * book.dim;

					for (c_int j = 0; (i < n) && (j < book.dim);)
						a[i++] += t[j++];
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// Unlike the others, we guard against n not being an integer number
		/// of *dim* internally rather than in the upper layer (called only
		/// by floor0)
		/// </summary>
		/********************************************************************/
		public static c_long Vorbis_Book_Decodev_Set(Codebook book, Span<c_float> a, OggPack b, c_int n)
		{
			if (book.used_entries > 0)
			{
				for (c_int i = 0; i < n;)
				{
					c_int entry = (c_int)Decode_Packed_Entry_Number(book, b);

					if (entry == -1)
						return -1;

					CPointer<c_float> t = book.valuelist + entry * book.dim;

					for (c_int j = 0; (i < n) && (j < book.dim);)
						a[i++] = t[j++];
				}
			}
			else
			{
				for (c_int i = 0; i < n;)
					a[i++] = 0.0f;
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_long Vorbis_Book_Decodevv_Add(Codebook book, CPointer<c_float>[] a, c_long offset, c_int ch, OggPack b, c_int n)
		{
			c_int chptr = 0;

			if (book.used_entries > 0)
			{
				c_int m = (c_int)((offset + n) / ch);

				for (c_long i = offset / ch; i < m;)
				{
					c_long entry = Decode_Packed_Entry_Number(book, b);

					if (entry == -1)
						return -1;

					{
						CPointer<c_float> t = book.valuelist + entry * book.dim;

						for (c_long j = 0; (i < m) && (j < book.dim); j++)
						{
							a[chptr++][i] += t[j];

							if (chptr == ch)
							{
								chptr = 0;
								i++;
							}
						}
					}
				}
			}

			return 0;
		}
	}
}
