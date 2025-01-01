/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibVorbis
{
	/// <summary>
	/// 
	/// </summary>
	public enum VorbisError
	{
		/// <summary></summary>
		Ok = 0,

		/// <summary></summary>
		False = -1,
		/// <summary></summary>
		Eof = -2,
		/// <summary></summary>
		Hole = -3,

		/// <summary></summary>
		Read = -128,
		/// <summary></summary>
		Fault = -129,
		/// <summary></summary>
		Impl = -130,
		/// <summary></summary>
		Inval = -131,
		/// <summary></summary>
		NotVorbis = -132,
		/// <summary></summary>
		BadHeader = -133,
		/// <summary></summary>
		Version = -134,
		/// <summary></summary>
		NotAudio = -135,
		/// <summary></summary>
		BadPacket = -136,
		/// <summary></summary>
		BadLink = -137,
		/// <summary></summary>
		NoSeek = -138
	}
}
