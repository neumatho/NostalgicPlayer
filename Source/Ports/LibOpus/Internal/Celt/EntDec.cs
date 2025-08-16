/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOpus.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Internal.Celt
{
	/// <summary>
	/// A range decoder.
	/// This is an entropy decoder based upon \cite{Mar79}, which is itself a
	/// rediscovery of the FIFO arithmetic code introduced by \cite{Pas76}.
	/// It is very similar to arithmetic encoding, except that encoding is done with
	/// digits in any base, instead of with bits, and so it is faster when using
	/// larger bases (i.e.: a byte).
	/// The author claims an average waste of $\frac{1}{2}\log_b(2b)$ bits, where $b$
	/// is the base, longer than the theoretical optimum, but to my knowledge there
	/// is no published justification for this claim.
	/// This only seems true when using near-infinite precision arithmetic so that
	/// the process is carried out with no rounding errors.
	///
	/// An excellent description of implementation details is available at
	/// http://www.arturocampos.com/ac_range.html
	/// A recent work \cite{MNW98} which proposes several changes to arithmetic
	/// encoding for efficiency actually re-discovers many of the principles
	/// behind range encoding, and presents a good theoretical analysis of them.
	///
	/// End of stream is handled by writing out the smallest number of bits that
	/// ensures that the stream will be correctly decoded regardless of the value of
	/// any subsequent bits.
	/// ec_tell() can be used to determine how many bits were needed to decode
	/// all the symbols thus far; other data can be packed in the remaining bits of
	/// the input buffer.
	///
	/// @PHDTHESIS{Pas76,
	/// author="Richard Clark Pasco",
	/// title="Source coding algorithms for fast data compression",
	/// school="Dept. of Electrical Engineering, Stanford University",
	/// address="Stanford, CA",
	/// month=May,
	/// year=1976
	/// }
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
	/// URL="http://www.stanford.edu/class/ee398a/handouts/papers/Moffat98ArithmCoding.pdf"
	/// </summary>
	internal static class EntDec
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Ec_Read_Byte(Ec_Dec _this)
		{
			return _this.offs < _this.storage ? _this.buf[_this.offs++] : 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static c_int Ec_Read_Byte_From_End(Ec_Dec _this)
		{
			return _this.end_offs < _this.storage ? _this.buf[_this.storage - ++(_this.end_offs)] : 0;
		}



		/********************************************************************/
		/// <summary>
		/// Normalizes the contents of val and rng so that rng lies entirely
		/// in the high-order symbol
		/// </summary>
		/********************************************************************/
		private static void Ec_Dec_Normalize(Ec_Dec _this)
		{
			// If the range is too small, rescale it and input some bits
			while (_this.rng <= Constants.Ec_Code_Bot)
			{
				_this.nbits_total += Constants.Ec_Sym_Bits;
				_this.rng <<= Constants.Ec_Sym_Bits;

				// Use up the remaining bits from our last symbol
				c_int sym = _this.rem;

				// Read the next value from the input
				_this.rem = Ec_Read_Byte(_this);

				// Take the rest of the bits we need from this new symbol
				sym = (sym << Constants.Ec_Sym_Bits | _this.rem) >> (Constants.Ec_Sym_Bits - Constants.Ec_Code_Extra);

				// And subtract them from val, capped to be less than EC_CODE_TOP
				_this.val = (opus_uint32)(((_this.val << Constants.Ec_Sym_Bits) + (Constants.Ec_Sym_Max & ~sym)) & (Constants.Ec_Code_Top - 1));
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Ec_Dec_Init(out Ec_Dec _this, CPointer<byte> _buf, opus_uint32 _storage)
		{
			_this = new Ec_Dec();

			_this.buf = _buf;
			_this.storage = _storage;
			_this.end_offs = 0;
			_this.end_window = 0;
			_this.nend_bits = 0;

			// This is the offset from which ec_tell() will subtract partial bits.
			// The final value after the ec_dec_normalize() call will be the same as in
			// the encoder, but we have to compensate for the bits that are added there
			_this.nbits_total = Constants.Ec_Code_Bits + 1 - ((Constants.Ec_Code_Bits - Constants.Ec_Code_Extra) / Constants.Ec_Sym_Bits) * Constants.Ec_Sym_Bits;
			_this.offs = 0;
			_this.rng = 1U << Constants.Ec_Code_Extra;
			_this.rem = Ec_Read_Byte(_this);
			_this.val = (opus_uint32)(_this.rng - 1 - (_this.rem >> (Constants.Ec_Sym_Bits - Constants.Ec_Code_Extra)));
			_this.error = false;

			// Normalize the interval
			Ec_Dec_Normalize(_this);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_uint Ec_Decode(Ec_Dec _this, c_uint _ft)
		{
			_this.ext = EntCode.Celt_UDiv(_this.rng, _ft);
			c_uint s = _this.val / _this.ext;

			return _ft - Ec_Mini(s + 1, _ft);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_uint Ec_Decode_Bin(Ec_Dec _this, c_uint _bits)
		{
			_this.ext = _this.rng >> (int)_bits;
			c_uint s = _this.val / _this.ext;

			return (1U << (int)_bits) - Ec_Mini(s + 1, 1U << (int)_bits);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static void Ec_Dec_Update(Ec_Dec _this, c_uint _fl, c_uint _fh, c_uint _ft)
		{
			opus_uint32 s = Arch.IMUL32(_this.ext, _ft - _fh);
			_this.val -= s;
			_this.rng = _fl > 0 ? Arch.IMUL32(_this.ext, _fh - _fl) : _this.rng - s;

			Ec_Dec_Normalize(_this);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool Ec_Dec_Bit_Logp(Ec_Dec _this, c_uint _logp)
		{
			opus_uint32 r = _this.rng;
			opus_uint32 d = _this.val;
			opus_uint32 s = r >> (int)_logp;
			bool ret = d < s;

			if (!ret)
				_this.val = d - s;

			_this.rng = ret ? s : r - s;

			Ec_Dec_Normalize(_this);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static c_int Ec_Dec_Icdf(Ec_Dec _this, CPointer<byte> _icdf, c_uint _ftb)
		{
			opus_uint32 s = _this.rng;
			opus_uint32 d = _this.val;
			opus_uint32 r = s >> (int)_ftb;
			c_int ret = -1;

			opus_uint32 t;

			do
			{
				t = s;
				s = Arch.IMUL32(r, _icdf[++ret]);
			}
			while (d < s);

			_this.val = d - s;
			_this.rng = t - s;

			Ec_Dec_Normalize(_this);

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static opus_uint32 Ec_Dec_UInt(Ec_Dec _this, opus_uint32 _ft)
		{
			_ft--;
			c_int ftb = EntCode.Ec_Ilog(_ft);

			if (ftb > Constants.Ec_UInt_Bits)
			{
				ftb -= Constants.Ec_UInt_Bits;
				c_uint ft = (_ft >> ftb) + 1;

				c_uint s = Ec_Decode(_this, ft);
				Ec_Dec_Update(_this, s, s + 1, ft);

				opus_uint32 t = s << ftb | Ec_Dec_Bits(_this, (c_uint)ftb);
				if (t <= _ft)
					return t;

				_this.error = true;

				return ft;
			}
			else
			{
				_ft++;

				c_uint s = Ec_Decode(_this, _ft);
				Ec_Dec_Update(_this, s, s + 1, _ft);

				return s;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static opus_uint32 Ec_Dec_Bits(Ec_Dec _this, c_uint _bits)
		{
			ec_window window = _this.end_window;
			c_int available = _this.nend_bits;

			if (available < _bits)
			{
				do
				{
					window |= (ec_window)Ec_Read_Byte_From_End(_this) << available;
					available += Constants.Ec_Sym_Bits;
				}
				while (available <= (Constants.Ec_Window_Size - Constants.Ec_Sym_Bits));
			}

			opus_uint32 ret = window & (((opus_uint32)1 << (int)_bits) - 1U);
			window >>= (int)_bits;
			available -= (c_int)_bits;

			_this.end_window = window;
			_this.nend_bits = available;
			_this.nbits_total += (c_int)_bits;

			return ret;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static c_uint Ec_Mini(c_uint a, c_uint b)
		{
			return (c_uint)(a + ((b - a) & -(b < a ? 1 : 0)));
		}
	}
}
