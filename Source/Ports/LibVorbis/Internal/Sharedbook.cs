/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using Polycode.NostalgicPlayer.CKit;
using Polycode.NostalgicPlayer.Ports.LibOgg;
using Polycode.NostalgicPlayer.Ports.LibVorbis.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibVorbis.Internal
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Sharedbook
	{
		private const int VQ_FMAN = 21;
		private const int VQ_FEXP_BIAS = 768;	// Bias toward values smaller than 1

		private class Sort32a : IComparer<CPointer<ogg_uint32_t>>
		{
			public int Compare(CPointer<ogg_uint32_t> a, CPointer<ogg_uint32_t> b)
			{
				return (a[0] > b[0] ? 1 : 0) - (a[0] < b[0] ? 1 : 0);
			}
		}

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Ov_ILog(ogg_uint32_t v)
		{
			c_int ret;

			for (ret = 0; v != 0; ret++)
				v >>= 1;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 32 bit float (not IEEE; nonnormalized mantissa +
		/// biased exponent) : neeeeeee eeemmmmm mmmmmmmm mmmmmmmm
		/// Why not IEEE? It's just not that important here
		/// </summary>
		/********************************************************************/
		private static c_float Float32_Unpack(c_long val)
		{
			c_double mant = val & 0x1fffff;
			bool sign = (val & 0x80000000) != 0;
			c_long exp = (c_long)((val & 0x7fe00000L) >> VQ_FMAN);

			if (sign)
				mant = -mant;

			exp = exp - (VQ_FMAN - 1) - VQ_FEXP_BIAS;

			// Clamp excessive exponent values
			if (exp > 63)
				exp = 63;

			if (exp < -63)
				exp = -63;

			return (c_float)CMath.ldexp(mant, exp);
		}



		/********************************************************************/
		/// <summary>
		/// Given a list of word lengths, generate a list of codewords. Works
		/// for length ordered or unordered, always assigns the lowest valued
		/// codewords first. Extended to handle unused entries (length 0)
		/// </summary>
		/********************************************************************/
		private static CPointer<ogg_uint32_t> Make_Words(CPointer<byte> l, c_long n, c_long sparsecount)
		{
			c_long i, count = 0;
			ogg_uint32_t[] marker = new ogg_uint32_t[33];
			CPointer<ogg_uint32_t> r = Memory.Ogg_MAlloc<ogg_uint32_t>((size_t)(sparsecount != 0 ? sparsecount : n));

			for (i = 0; i < n; i++)
			{
				c_long length = l[i];

				if (length > 0)
				{
					ogg_uint32_t entry = marker[length];

					// When we claim a node for an entry, we also claim the nodes
					// below it (pruning off the imagined tree that may have dangled
					// from it) as well as blocking the use of any nodes directly
					// above for leaves

					// Update ourself
					if ((length < 32) && ((entry >> length) != 0))
					{
						// Error condition; the lengths must specify an overpopulated tree
						Memory.Ogg_Free(r);

						return null;
					}

					r[count++] = entry;

					// Look to see if the next shorter marker points to the node
					// above. If so, update it and repeat
					{
						for (c_long j = length; j > 0; j--)
						{
							if ((marker[j] & 1) != 0)
							{
								// Have to jump branches
								if (j == 1)
									marker[1]++;
								else
									marker[j] = marker[j - 1] << 1;

								break;	// Invariant says next upper marker would already have been moved if it was on the same path
							}

							marker[j]++;
						}
					}

					// Prune the tree; the implicit invariant says all the longer
					// markers were dangling from out just-taken node. Dangle them
					// from our *new* node
					for (c_long j = length + 1; j < 33; j++)
					{
						if ((marker[j] >> 1) == entry)
						{
							entry = marker[j];
							marker[j] = marker[j - 1] << 1;
						}
						else
							break;
					}
				}
				else
				{
					if (sparsecount == 0)
						count++;
				}
			}

			// Any underpopulated tree must be rejected.
			// Single-entry codebooks are a retconned extension to the spec.
			// They have a single codeword '0' of length 1 that results in an
			// underpopulated tree. Shield that case from the underformed tree check
			if (!(count == 1) && (marker[2] == 2))
			{
				for (i = 1; i < 33; i++)
				{
					if ((marker[i] & (0xffffffffUL >> (32 - i))) != 0)
					{
						Memory.Ogg_Free(r);

						return null;
					}
				}
			}

			// Bitreverse the words because our bitwise packer/unpacker is LSb
			// endian
			for (i = 0, count = 0; i < n; i++)
			{
				ogg_uint32_t temp = 0;

				for (c_long j = 0; j < l[i]; j++)
				{
					temp <<= 1;
					temp |= (r[count] >> j) & 1;
				}

				if (sparsecount != 0)
				{
					if (l[i] != 0)
						r[count++] = temp;
				}
				else
					r[count++] = temp;
			}

			return r;
		}



		/********************************************************************/
		/// <summary>
		/// There might be a straightforward one-line way to do the below
		/// that's portable and totally safe against roundoff, but I haven't
		/// thought of it. Therefore, we opt on the side of caution
		/// </summary>
		/********************************************************************/
		public static c_long Book_Maptype1_Quantvals(StaticCodebook b)
		{
			if (b.entries < 1)
				return 0;

			c_long vals = (c_long)Math.Floor(Math.Pow(b.entries, 1.0 / b.dim));

			// The above *should* be reliable, but we'll not assume that FP is
			// ever reliable when bitstream sync is at stake; verify via integer
			// means that vals really is the greatest value of dim for which
			// vals^b->bim <= b->entries
			//
			// Treat the above as an initial guess
			if (vals < 1)
				vals = 1;

			while (true)
			{
				c_long acc = 1;
				c_long acc1 = 1;
				c_int i;

				for (i = 0; i < b.dim; i++)
				{
					if ((b.entries / vals) < acc)
						break;

					acc *= vals;

					if ((c_long.MaxValue / (vals + 1)) < acc1)
						acc1 = c_long.MaxValue;
					else
						acc1 *= vals + 1;
				}

				if ((i >= b.dim) && (acc <= b.entries) && (acc1 > b.entries))
					return vals;
				else
				{
					if ((i < b.dim) || (acc > b.entries))
						vals--;
					else
						vals++;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Unpack the quantized list of values for encode/decode.
		/// We need to deal with two map types: in map type 1, the values are
		/// generated algorithmically (each column of the vector counts
		/// through the values in the quant vector). in map type 2, all the
		/// values came in in an explicit list. Both value lists must be
		/// unpacked
		/// </summary>
		/********************************************************************/
		private static CPointer<c_float> Book_Unquantize(StaticCodebook b, c_int n, CPointer<c_int> sparsemap)
		{
			c_long count = 0;

			if ((b.maptype == 1) || (b.maptype == 2))
			{
				c_float mindel = Float32_Unpack(b.q_min);
				c_float delta = Float32_Unpack(b.q_delta);
				CPointer<c_float> r = Memory.Ogg_CAlloc<c_float>((size_t)(n * b.dim));

				// maptype 1 and 2 both use a quantized value vector, but
				// different sizes
				switch (b.maptype)
				{
					case 1:
					{
						// Most of the time, entries%dimensions == 0, but we need to be
						// well defined. We define that the possible vales at each
						// scalar is values == entries/dim. If entries%dim != 0, we'll
						// have 'too few' values (values*dim<entries), which means that
						// we'll have 'left over' entries; left over entries use zeroed
						// values (and are wasted). So don't generate codebooks like
						// that
						c_int quantvals = Book_Maptype1_Quantvals(b);

						for (c_long j = 0; j < b.entries; j++)
						{
							if ((sparsemap.IsNotNull && (b.lengthlist[j] != 0)) || sparsemap.IsNull)
							{
								c_float last = 0.0f;
								c_int indexdiv = 1;

								for (c_long k = 0; k < b.dim; k++)
								{
									c_int index = (j / indexdiv) % quantvals;
									c_float val = b.quantlist[index];
									val = Math.Abs(val) * delta + mindel + last;

									if (b.q_sequencep != 0)
										last = val;

									if (sparsemap.IsNotNull)
										r[sparsemap[count] * b.dim + k] = val;
									else
										r[count * b.dim + k] = val;

									indexdiv *= quantvals;
								}

								count++;
							}
						}
						break;
					}

					case 2:
					{
						for (c_long j = 0; j < b.entries; j++)
						{
							if ((sparsemap.IsNotNull && (b.lengthlist[j] != 0)) || sparsemap.IsNull)
							{
								c_float last = 0.0f;

								for (c_long k = 0; k < b.dim; k++)
								{
									c_float val = b.quantlist[j * b.dim + k];
									val = Math.Abs(val) * delta + mindel + last;

									if (b.q_sequencep != 0)
										last = val;

									if (sparsemap.IsNotNull)
										r[sparsemap[count] * b.dim + k] = val;
									else
										r[count * b.dim + k] = val;
								}

								count++;
							}
						}
						break;
					}
				}

				return r;
			}

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Vorbis_Staticbook_Destroy(StaticCodebook b)
		{
			if (b.allocedp)
			{
				if (b.quantlist != null)
					b.quantlist = null;

				if (b.lengthlist != null)
					b.lengthlist = null;

				b.Clear();
			}	// Otherwise, it is in static memory
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Vorbis_Book_Clear(Codebook b)
		{
			// Static book is not cleared; we're likely called on the lookup and
			// the static codebook belongs to the info struct
			if (b.valuelist.IsNotNull)
				Memory.Ogg_Free(b.valuelist);

			if (b.codelist != null)
				b.codelist = null;

			if (b.dec_index != null)
				b.dec_index = null;

			if (b.dec_codelengths != null)
				b.dec_codelengths = null;

			if (b.dec_firsttable != null)
				b.dec_firsttable = null;

			b.Clear();
		}



		/********************************************************************/
		/// <summary>
		/// 
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
		public static c_int Vorbis_Book_Init_Decode(Codebook c, StaticCodebook s)
		{
			c_int i, n = 0;

			c.Clear();

			// Count actually used entries and find max length
			for (i = 0; i < s.entries; i++)
			{
				if (s.lengthlist[i] > 0)
					n++;
			}

			c.entries = s.entries;
			c.used_entries = n;
			c.dim = s.dim;

			if (n > 0)
			{
				// Two different remappings go on here.
				//
				// First, we collapse the likely sparse codebook down only to
				// actually represented values/words. This collapsing needs to be
				// indexed as map-valueless books are used to encode original entry
				// positions as integers.
				//
				// Second, we reorder all vectors, including the entry index above,
				// by sorted bitreversed codeword to allow treeless decode

				// Perform sort
				CPointer<ogg_uint32_t> codes = Make_Words(s.lengthlist, s.entries, c.used_entries);
				CPointer<ogg_uint32_t>[] codep = new CPointer<ogg_uint32_t>[n];

				if (codes == null)
					goto ErrOut;

				for (i = 0; i < n; i++)
				{
					codes[i] = Bitreverse(codes[i]);
					codep[i] = codes + i;
				}

				Array.Sort(codep, 0, n, new Sort32a());

				c_int[] sortindex = new c_int[n];
				c.codelist = new ogg_uint32_t[n];

				// The index is a reverse index
				for (i = 0; i < n; i++)
				{
					c_int position = codep[i] - codes;
					sortindex[position] = i;
				}

				for (i = 0; i < n; i++)
					c.codelist[sortindex[i]] = codes[i];

				codes = null;

				c.valuelist = Book_Unquantize(s, n, sortindex);
				c.dec_index = new c_int[n];

				for (n = 0, i = 0; i < s.entries; i++)
				{
					if (s.lengthlist[i] > 0)
						c.dec_index[sortindex[n++]] = i;
				}

				c.dec_codelengths = new byte[n];
				c.dec_maxlength = 0;

				for (n = 0, i = 0; i < s.entries; i++)
				{
					if (s.lengthlist[i] > 0)
					{
						c.dec_codelengths[sortindex[n++]] = s.lengthlist[i];

						if (s.lengthlist[i] > c.dec_maxlength)
							c.dec_maxlength = s.lengthlist[i];
					}
				}

				if ((n == 1) && (c.dec_maxlength == 1))
				{
					// Special case the 'single entry codebook' with a single bit
					// fastpath table (that always returns entry 0) in order to use
					// unmodified decode paths
					c.dec_firsttablen = 1;
					c.dec_firsttable = new ogg_uint32_t[2];
					c.dec_firsttable[0] = c.dec_firsttable[1] = 1;
				}
				else
				{
					c.dec_firsttablen = Ov_ILog((ogg_uint32_t)c.used_entries) - 4;	// This is magic

					if (c.dec_firsttablen < 5)
						c.dec_firsttablen = 5;

					if (c.dec_firsttablen > 8)
						c.dec_firsttablen = 8;

					c_int tabn = 1 << c.dec_firsttablen;
					c.dec_firsttable = new ogg_uint32_t[tabn];

					for (i = 0; i < n; i++)
					{
						if (c.dec_codelengths[i] <= c.dec_firsttablen)
						{
							ogg_uint32_t orig = Bitreverse(c.codelist[i]);

							for (c_int j = 0; j < (1 << (c.dec_firsttablen - c.dec_codelengths[i])); j++)
								c.dec_firsttable[orig | (ogg_uint32_t)(j << c.dec_codelengths[i])] = (ogg_uint32_t)i + 1;
						}
					}

					// Now fill in 'unused' entries in the firsttable with hi/lo search
					// hints for the non-direct-hits
					{
						ogg_uint32_t mask = (ogg_uint32_t)(0xfffffffeUL << (31 - c.dec_firsttablen));
						c_long lo = 0, hi = 0;

						for (i = 0; i < tabn; i++)
						{
							ogg_uint32_t word = ((ogg_uint32_t)i << (32 - c.dec_firsttablen));

							if (c.dec_firsttable[Bitreverse(word)] == 0)
							{
								while (((lo + 1) < n) && (c.codelist[lo + 1] <= word))
									lo++;

								while ((hi < n) && (word >= (c.codelist[hi] & mask)))
									hi++;

								// We only actually have 15 bits per hint to play with here.
								// In order to overflow gracefully (nothing breaks, efficiency
								// just drops), encode as the difference from the extremes
								{
									c_ulong loval = (c_ulong)lo;
									c_ulong hival = (c_ulong)(n - hi);

									if (loval > 0x7fff)
										loval = 0x7fff;

									if (hival > 0x7fff)
										hival = 0x7fff;

									c.dec_firsttable[Bitreverse(word)] = (ogg_uint32_t)(0x80000000UL | (loval << 15) | hival);
								}
							}
						}
					}
				}
			}

			return 0;

			ErrOut:
			Vorbis_Book_Clear(c);

			return -1;
		}
	}
}
