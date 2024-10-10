/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Celt
{
	/// <summary>
	/// A range encoder.
	/// See entdec.c and the references for implementation details \cite{Mar79,MNW98}.
	///
	/// @INPROCEEDINGS{Mar79,
	/// author="Martin, G.N.N.",
	/// title="Range encoding: an algorithm for removing redundancy from a digitised
	/// message",
	/// booktitle="Video and Data Recording Conference",
	/// year=1979,
	/// address="Southampton",
	/// month=Jul
	/// }
	/// @ARTICLE{MNW98,
	/// author="Alistair Moffat and Radford Neal and Ian H. Witten",
	/// title="Arithmetic Coding Revisited",
	/// journal="{ACM} Transactions on Information Systems",
	/// year=1998,
	/// volume=16,
	/// number=3,
	/// pages="256--294",
	/// month=Jul,
	/// URL="http://www.stanford.edu/class/ee398/handouts/papers/Moffat98ArithmCoding.pdf"
	/// </summary>
	internal static class EntEnc
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static bool Ec_Write_Byte(Ec_Enc _this, c_uint _value)
		{
			if ((_this.offs + _this.end_offs) >= _this.storage)
				return true;

			_this.buf[_this.offs++] = (byte)_value;

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static bool Ec_Write_Byte_At_End(Ec_Enc _this, c_uint _value)
		{
			if ((_this.offs + _this.end_offs) >= _this.storage)
				return true;

			_this.buf[_this.storage - ++(_this.end_offs)] = (byte)_value;

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Outputs a symbol, with a carry bit.
		/// If there is a potential to propagate a carry over several
		/// symbols, they are buffered until it can be determined whether or
		/// not an actual carry will occur.
		/// If the counter for the buffered symbols overflows, then the
		/// stream becomes undecodable.
		/// This gives a theoretical limit of a few billion symbols in a
		/// single packet on 32-bit systems.
		/// The alternative is to truncate the range in order to force a
		/// carry, but requires similar carry tracking in the decoder,
		/// needlessly slowing it down
		/// </summary>
		/********************************************************************/
		private static void Ec_Enc_Carry_Out(Ec_Enc _this, c_int _c)
		{
			if (_c != Constants.Ec_Sym_Max)
			{
				// No further carry propagation possible, flush buffer
				c_int carry = _c >> Constants.Ec_Sym_Bits;

				// Don't output a byte on the first write.
				// This compare should be taken care of by branch-prediction thereafter
				if (_this.rem >= 0)
					_this.error |= Ec_Write_Byte(_this, (c_uint)(_this.rem + carry));

				if (_this.ext > 0)
				{
					c_uint sym = (c_uint)((Constants.Ec_Sym_Max + carry) & Constants.Ec_Sym_Max);

					do
					{
						_this.error |= Ec_Write_Byte(_this, sym);
					}
					while (--(_this.ext) > 0);
				}

				_this.rem = (c_int)(_c & Constants.Ec_Sym_Max);
			}
			else
				_this.ext++;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Ec_Enc_Normalize(Ec_Enc _this)
		{
			// If the range is too small, output some bits and rescale it
			while (_this.rng <= Constants.Ec_Code_Bot)
			{
				Ec_Enc_Carry_Out(_this, (c_int)(_this.val >> Constants.Ec_Code_Shift));

				// Move the next-to-high-order symbol into the high-order position
				_this.val = (_this.val << Constants.Ec_Sym_Bits) & (Constants.Ec_Code_Top - 1);
				_this.rng <<= Constants.Ec_Sym_Bits;
				_this.nbits_total += Constants.Ec_Sym_Bits;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Ec_Enc_Init(out Ec_Enc _this, Pointer<byte> _buf, opus_uint32 _size)
		{
			_this = new Ec_Enc();

			_this.buf = _buf;
			_this.end_offs = 0;
			_this.end_window = 0;
			_this.nend_bits = 0;

			// This is the offset from which ec_tell() will subtract partial bits
			_this.nbits_total = Constants.Ec_Code_Bits + 1;
			_this.offs = 0;
			_this.rng = Constants.Ec_Code_Top;
			_this.rem = -1;
			_this.val = 0;
			_this.ext = 0;
			_this.storage = _size;
			_this.error = false;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Ec_Encode(Ec_Enc _this, c_uint _fl, c_uint _fh, c_uint _ft)
		{
			opus_uint32 r = EntCode.Celt_UDiv(_this.rng, _ft);

			if (_fl > 0)
			{
				_this.val += _this.rng - Arch.IMUL32(r, _ft - _fl);
				_this.rng = Arch.IMUL32(r, _fh - _fl);
			}
			else
				_this.rng -= Arch.IMUL32(r, _ft - _fh);

			Ec_Enc_Normalize(_this);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Ec_Encode_Bin(Ec_Enc _this, c_uint _fl, c_uint _fh, c_uint _bits)
		{
			opus_uint32 r = _this.rng >> (int)_bits;

			if (_fl > 0)
			{
				_this.val += _this.rng - Arch.IMUL32(r, (1U << (int)_bits) - _fl);
				_this.rng = Arch.IMUL32(r, _fh - _fl);
			}
			else
				_this.rng -= Arch.IMUL32(r, (1U << (int)_bits) - _fh);

			Ec_Enc_Normalize(_this);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Ec_Enc_Bit_Logp(Ec_Enc _this, bool _val, c_uint _logp)
		{
			opus_uint32 r = _this.rng;
			opus_uint32 l = _this.val;
			opus_uint32 s = r >> (int)_logp;
			r -= s;

			if (_val)
				_this.val = l + r;

			_this.rng = _val ? s : r;

			Ec_Enc_Normalize(_this);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Ec_Enc_Icdf(Ec_Enc _this, c_int _s, Pointer<byte> _icdf, c_uint _ftb)
		{
			opus_uint32 r = _this.rng >> (int)_ftb;

			if (_s > 0)
			{
				_this.val += _this.rng - Arch.IMUL32(r, _icdf[_s - 1]);
				_this.rng = Arch.IMUL32(r, (c_uint)_icdf[_s - 1] - _icdf[_s]);
			}
			else
				_this.rng -= Arch.IMUL32(r, _icdf[_s]);

			Ec_Enc_Normalize(_this);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Ec_Enc_UInt(Ec_Enc _this, opus_uint32 _fl, opus_uint32 _ft)
		{
			_ft--;
			c_int ftb = EntCode.Ec_Ilog(_ft);

			if (ftb > Constants.Ec_UInt_Bits)
			{
				ftb -= Constants.Ec_UInt_Bits;
				c_uint ft = (_ft >> ftb) + 1;
				c_uint fl = _fl >> ftb;

				Ec_Encode(_this, fl, fl + 1, ft);
				Ec_Enc_Bits(_this, (opus_uint32)(_fl & ((1 << ftb) - 1)), (c_uint)ftb);
			}
			else
				Ec_Encode(_this, _fl, _fl + 1, _ft + 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Ec_Enc_Bits(Ec_Enc _this, opus_uint32 _fl, c_uint _bits)
		{
			ec_window window = _this.end_window;
			c_int used = _this.nend_bits;

			if ((used + _bits) > Constants.Ec_Window_Size)
			{
				do
				{
					_this.error |= Ec_Write_Byte_At_End(_this, window & Constants.Ec_Sym_Max);
					window >>= Constants.Ec_Sym_Bits;
					used -= Constants.Ec_Sym_Bits;
				}
				while (used >= Constants.Ec_Sym_Bits);
			}

			window |= _fl << used;
			used = (c_int)(used + _bits);

			_this.end_window = window;
			_this.nend_bits = used;
			_this.nbits_total = (c_int)(_this.nbits_total + _bits);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Ec_Enc_Patch_Initial_Bits(Ec_Enc _this, c_uint _val, c_uint _nbits)
		{
			c_int shift = (c_int)(Constants.Ec_Sym_Bits - _nbits);
			c_uint mask = ((1U << (int)_nbits) - 1) << shift;

			if (_this.offs > 0)
			{
				// The first byte has been finalized
				_this.buf[0] = (byte)((_this.buf[0] & ~mask) | _val << shift);
			}
			else if (_this.rem >= 0)
			{
				// The first byte is still awaiting carry propagation
				_this.rem = (c_int)((_this.rem & ~mask) | _val << shift);
			}
			else if (_this.rng <= (Constants.Ec_Code_Top >> (int)_nbits))
			{
				// The renormalization loop has never been run
				_this.val = (_this.val & ~((opus_uint32)mask << Constants.Ec_Code_Shift)) | _val << (Constants.Ec_Code_Shift + shift);
			}
			// The encoder hasn't even encoded _nbits of data yet
			else
				_this.error = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Ec_Enc_Done(Ec_Enc _this)
		{
			// We output the minimum number of bits that ensures that the symbols encoded
			// thus far will be decoded correctly regardless of the bits that follow
			c_int l = Constants.Ec_Code_Bits - EntCode.Ec_Ilog(_this.rng);
			opus_uint32 msk = (Constants.Ec_Code_Top - 1) >> l;
			opus_uint32 end = (_this.val + msk) & ~msk;

			if ((end | msk) >= (_this.val + _this.rng))
			{
				l++;
				msk >>= 1;
				end = (_this.val + msk) & ~msk;
			}

			while (l > 0)
			{
				Ec_Enc_Carry_Out(_this, (c_int)(end >> Constants.Ec_Code_Shift));
				end = (end << Constants.Ec_Sym_Bits) & (Constants.Ec_Code_Top - 1);
				l -= Constants.Ec_Sym_Bits;
			}

			// If we have a buffered byte flush it into the output buffer
			if ((_this.rem >= 0) || (_this.ext > 0))
				Ec_Enc_Carry_Out(_this, 0);

			// If we have buffered extra bits, flush them as well
			ec_window window = _this.end_window;
			c_int used = _this.nend_bits;

			while (used >= Constants.Ec_Sym_Bits)
			{
				_this.error |= Ec_Write_Byte_At_End(_this, window & Constants.Ec_Sym_Max);
				window >>= Constants.Ec_Sym_Bits;
				used -= Constants.Ec_Sym_Bits;
			}

			// Clear any excess space and add any remaining extra bits to the last byte
			if (!_this.error)
			{
				Memory.Opus_Clear(_this.buf + _this.offs, (int)(_this.storage - _this.offs - _this.end_offs));

				if (used > 0)
				{
					// If there's no range coder data at all, give up
					if (_this.end_offs >= _this.storage)
						_this.error = true;
					else
					{
						l = -l;

						// If we've busted, don't add too many bits to the last byte; it
						// would corrupt the range coder data, and that's more important
						if (((_this.offs + _this.end_offs) >= _this.storage) && (l < used))
						{
							window &= (1U << l) - 1;
							_this.error = true;
						}

						_this.buf[_this.storage - _this.end_offs - 1] |= (byte)window;
					}
				}
			}
		}
	}
}
