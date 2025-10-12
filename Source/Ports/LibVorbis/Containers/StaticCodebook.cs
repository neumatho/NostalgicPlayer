/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibVorbis.Containers
{
	/// <summary>
	/// This structure encapsulates huffman and VQ style encoding books; it
	/// doesn't do anything specific to either.
	///
	/// valuelist/quantlist are nonNULL (and q_* significant) only if
	/// there's entry->value mapping to be done.
	///
	/// If encode-side mapping must be done (and thus the entry needs to be
	/// hunted), the auxiliary encode pointer will point to a decision
	/// tree. This is true of both VQ and huffman, but is mostly useful
	/// with VQ
	/// </summary>
	internal class StaticCodebook : IClearable
	{
		/// <summary>
		/// Codebook dimensions (elements per vector)
		/// </summary>
		public c_long dim;

		/// <summary>
		/// Codebook entries
		/// </summary>
		public c_long entries;

		/// <summary>
		/// Codeword lengths in bits
		/// </summary>
		public byte[] lengthlist;

		//
		// Mapping
		//

		public c_int maptype;

		// The below does a linear, single monotonic sequence mapping

		/// <summary>
		/// Packed 32 bit float; quant value 0 maps to minval
		/// </summary>
		public c_long q_min;

		/// <summary>
		/// Packed 32 bit float; val 1 - val 0 == delta
		/// </summary>
		public c_long q_delta;

		// <summary>
		// Bits: 0 < quant <= 16
		// </summary>
		public c_int q_quant;

		/// <summary>
		/// Bitflag
		/// </summary>
		public c_int q_sequencep;

		/// <summary>
		/// map == 1: (int)(entries^(1/dim)) element column map
		/// map == 2: list of dim*entries quantized entry vals
		/// </summary>
		public c_long[] quantlist;

		/// <summary>
		/// 
		/// </summary>
		public bool allocedp;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			dim = 0;
			entries = 0;
			lengthlist = null;
			maptype = 0;
			q_min = 0;
			q_delta = 0;
			q_quant = 0;
			q_sequencep = 0;
			quantlist = null;
			allocedp = false;
		}
	}
}
