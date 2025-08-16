/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibOpus.Containers
{
	/// <summary>
	/// The entropy encoder/decoder context.
	/// We use the same structure for both, so that common functions like ec_tell()
	/// can be used on either one
	/// </summary>
	internal class Ec_Ctx : IDeepCloneable<Ec_Ctx>
	{
		/// <summary>
		/// Buffered input/output
		/// </summary>
		public CPointer<byte> buf;

		/// <summary>
		/// The size of the buffer
		/// </summary>
		public opus_uint32 storage;

		/// <summary>
		/// The offset at which the last byte containing raw bits was read/written
		/// </summary>
		public opus_uint32 end_offs;

		/// <summary>
		/// Bits that will be read from/written at the end
		/// </summary>
		public ec_window end_window;

		/// <summary>
		/// Number of valid bits in end_window
		/// </summary>
		public c_int nend_bits;

		/// <summary>
		/// The total number of whole bits read/written.
		/// This does not include partial bits currently in the range coder
		/// </summary>
		public c_int nbits_total;

		/// <summary>
		/// The offset at which the next range coder byte will be read/written
		/// </summary>
		public opus_uint32 offs;

		/// <summary>
		/// The number of values in the current range
		/// </summary>
		public opus_uint32 rng;

		/// <summary>
		/// In the decoder: the difference between the top of the current range and
		/// the input value, minus one.
		///
		/// In the encoder: the low end of the current range
		/// </summary>
		public opus_uint32 val;

		/// <summary>
		/// In the decoder: the saved normalization factor from ec_decode().
		///
		/// In the encoder: the number of outstanding carry propagating symbols
		/// </summary>
		public opus_uint32 ext;

		/// <summary>
		/// A buffered input/output symbol, awaiting carry propagation
		/// </summary>
		public c_int rem;

		/// <summary>
		/// True if an error occurred
		/// </summary>
		public bool error;

		/********************************************************************/
		/// <summary>
		/// Clone the current object into a new one
		/// </summary>
		/********************************************************************/
		public Ec_Ctx MakeDeepClone()
		{
			return new Ec_Ctx
			{
				buf = buf,
				storage = storage,
				end_offs = end_offs,
				end_window = end_window,
				nend_bits = nend_bits,
				nbits_total = nbits_total,
				offs = offs,
				rng = rng,
				val = val,
				ext = ext,
				rem = rem,
				error = error
			};
		}
	}
}
