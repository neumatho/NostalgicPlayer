/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibOgg;
using Polycode.NostalgicPlayer.Ports.LibVorbis.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibVorbis.Containers
{
	/// <summary>
	/// Vorbis_block is a single block of data to be processed as part of
	/// the analysis/synthesis stream; it belongs to a specific logical
	/// bitstream, but is independent from other vorbis_blocks belonging to
	/// that logical bitstream
	/// </summary>
	public class VorbisBlock
	{
		// Necessary stream state for linking to the framing abstraction

		/// <summary>
		/// This is a pointer into local storage
		/// </summary>
		public Pointer<c_float>[] pcm;
		/// <summary></summary>
		public OggPack opb;

		/// <summary></summary>
		public c_long lW;
		/// <summary></summary>
		public c_long W;
		/// <summary></summary>
		public c_long nW;
		/// <summary></summary>
		public c_int pcmend;
		/// <summary></summary>
		public c_int mode;

		/// <summary></summary>
		public bool eofflag;
		/// <summary></summary>
		public ogg_int64_t granulepos;
		/// <summary></summary>
		public ogg_int64_t sequence;

		/// <summary>
		/// For read-only access of configuration
		/// </summary>
		public VorbisDspState vd;

		// Local storage to avoid remallocing; it's up the the mapping to
		// structure it
		/// <summary></summary>
		public Pointer<byte> localstore;
		/// <summary></summary>
		public c_long localtop;
		/// <summary></summary>
		public c_long localalloc;
		/// <summary></summary>
		public c_long totaluse;
		/// <summary></summary>
		public AllocChain reap;

		// Bitmetrics for the frame
		/// <summary></summary>
		public c_long glue_bits;
		/// <summary></summary>
		public c_long time_bits;
		/// <summary></summary>
		public c_long floor_bits;
		/// <summary></summary>
		public c_long res_bits;

		/// <summary></summary>
		public IVorbisBlockInternal @internal;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			pcm = null;
			opb = null;
			lW = 0;
			W = 0;
			nW = 0;
			pcmend = 0;
			mode = 0;
			eofflag = false;
			granulepos = 0;
			sequence = 0;
			vd = null;
			localstore.SetToNull();
			localtop = 0;
			localalloc = 0;
			totaluse = 0;
			reap = null;
			glue_bits = 0;
			time_bits = 0;
			floor_bits = 0;
			res_bits = 0;
			@internal = null;
		}
	}
}
