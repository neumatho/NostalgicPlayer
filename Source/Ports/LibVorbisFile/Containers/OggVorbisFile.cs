/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.LibOgg;
using Polycode.NostalgicPlayer.Ports.LibVorbis.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibVorbisFile.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class OggVorbisFile
	{
		/// <summary>
		/// A FILE *, memory buffer, etc.
		/// </summary>
		public object datasource;

		/// <summary></summary>
		public bool seekable;

		/// <summary></summary>
		public ogg_int64_t offset;

		/// <summary></summary>
		public ogg_int64_t end;

		/// <summary></summary>
		public OggSync oy;

		// If the FILE handle isn't seekable (eg, a pipe), only the current
		// stream appears

		/// <summary></summary>
		public c_int links;

		/// <summary></summary>
		public CPointer<ogg_int64_t> offsets;

		/// <summary></summary>
		public CPointer<ogg_int64_t> dataoffsets;

		/// <summary></summary>
		public CPointer<c_long> serialnos;

		/// <summary>
		/// Overloaded to maintain binary compatibility; x2 size, stores both
		/// beginning and end values
		/// </summary>
		public CPointer<ogg_int64_t> pcmlengths;

		/// <summary></summary>
		public CPointer<VorbisInfo> vi;

		/// <summary></summary>
		public CPointer<VorbisComment> vc;

		// Decoding working state local storage

		/// <summary></summary>
		public ogg_int64_t pcm_offset;

		/// <summary></summary>
		public State ready_state;

		/// <summary></summary>
		public c_long current_serialno;

		/// <summary></summary>
		public c_int current_link;

		/// <summary></summary>
		public c_double bittrack;

		/// <summary></summary>
		public c_double samptrack;

		/// <summary>
		/// Takes physical pages and welds them into a logical stream of
		/// packets
		/// </summary>
		public OggStream os;

		/// <summary>
		/// Central working state for the packet to PCM decoder
		/// </summary>
		public readonly VorbisDspState vd = new VorbisDspState();

		/// <summary>
		/// Local working space for packet to PCM decode
		/// </summary>
		public readonly VorbisBlock vb = new VorbisBlock();

		/// <summary></summary>
		public OvCallbacks callbacks;
	}
}
