/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// Known language codespaces
	/// </summary>
	public enum AvLangCodespace
	{
		/// <summary>
		/// 3-char bibliographic language codes as per ISO-IEC 639-2
		/// </summary>
		Iso639_2_Bibl,

		/// <summary>
		/// 3-char terminological language codes as per ISO-IEC 639-2
		/// </summary>
		Iso639_2_Term,

		/// <summary>
		/// 2-char code of language as per ISO/IEC 639-1
		/// </summary>
		Iso639_1
	}
}
