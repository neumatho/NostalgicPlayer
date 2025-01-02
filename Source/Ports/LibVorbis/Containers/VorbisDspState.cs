/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.CKit;
using Polycode.NostalgicPlayer.Ports.LibVorbis.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibVorbis.Containers
{
	/// <summary>
	/// Vorbis_dsp_state buffers the current vorbis audio
	/// analysis/synthesis state. The DSP state belongs to a specific
	/// logical bitstream
	/// </summary>
	public class VorbisDspState
	{
		/// <summary></summary>
		public bool analysisp;
		/// <summary></summary>
		public VorbisInfo vi;

		/// <summary></summary>
		public CPointer<c_float>[] pcm;
		/// <summary></summary>
		public CPointer<c_float>[] pcmret;
		/// <summary></summary>
		public c_int pcm_storage;
		/// <summary></summary>
		public c_int pcm_current;
		/// <summary></summary>
		public c_int pcm_returned;

		/// <summary></summary>
		public c_int preextrapolate;
		/// <summary></summary>
		public bool eofflag;

		/// <summary></summary>
		public c_long lW;
		/// <summary></summary>
		public c_long W;
		/// <summary></summary>
		public c_long nW;
		/// <summary></summary>
		public c_long centerW;

		/// <summary></summary>
		public ogg_int64_t granulepos;
		/// <summary></summary>
		public ogg_int64_t sequence;

		/// <summary></summary>
		public ogg_int64_t glue_bits;
		/// <summary></summary>
		public ogg_int64_t time_bits;
		/// <summary></summary>
		public ogg_int64_t floor_bits;
		/// <summary></summary>
		public ogg_int64_t res_bits;

		/// <summary></summary>
		public IBackendState backend_state;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			analysisp = false;
			vi = null;
			pcm = null;
			pcmret = null;
			pcm_storage = 0;
			pcm_current = 0;
			pcm_returned = 0;
			preextrapolate = 0;
			eofflag = false;
			lW = 0;
			W = 0;
			nW = 0;
			centerW = 0;
			granulepos = 0;
			sequence = 0;
			glue_bits = 0;
			time_bits = 0;
			floor_bits = 0;
			res_bits = 0;
			backend_state = null;
		}
	}
}
