/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.CKit;

namespace Polycode.NostalgicPlayer.Ports.LibVorbis.Containers
{
	/// <summary>
	/// The comments are not part of vorbis_info so that vorbis_info can be
	/// static storage
	/// </summary>
	public class VorbisComment
	{
		/// <summary>
		/// Unlimited user comment fields. libvorbis writes 'libvorbis'
		/// whatever vendor is set to in encode
		/// </summary>
		public CPointer<CPointer<byte>> user_comments;

		/// <summary></summary>
		public CPointer<c_int> comment_lengths;

		/// <summary></summary>
		public c_int comments;

		/// <summary></summary>
		public CPointer<byte> vendor;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			user_comments.SetToNull();
			comment_lengths.SetToNull();
			comments = 0;
			vendor.SetToNull();
		}
	}
}
