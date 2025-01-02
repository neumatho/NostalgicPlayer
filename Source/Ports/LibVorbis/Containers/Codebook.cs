/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.CKit;

namespace Polycode.NostalgicPlayer.Ports.LibVorbis.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class Codebook
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
		/// Populated codebook entries
		/// </summary>
		public c_long used_entries;
		public StaticCodebook c;

		// For encode, the below are entry-ordered, fully populated
		// For decode, the below are ordered by bitreversed codeword and only
		// used entries are populated

		/// <summary>
		/// List of dim*entries actual entry values
		/// </summary>
		public CPointer<c_float> valuelist;

		/// <summary>
		/// List of bitstream codewords for each entry
		/// </summary>
		public ogg_uint32_t[] codelist;

		/// <summary>
		/// Only used if sparseness collapsed
		/// </summary>
		public c_int[] dec_index;
		public byte[] dec_codelengths;
		public ogg_uint32_t[] dec_firsttable;
		public c_int dec_firsttablen;
		public c_int dec_maxlength;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			dim = 0;
			entries = 0;
			used_entries = 0;
			c = null;
			valuelist.SetToNull();
			codelist = null;
			dec_index = null;
			dec_codelengths = null;
			dec_firsttable = null;
			dec_firsttablen = 0;
			dec_maxlength = 0;
		}
	}
}
