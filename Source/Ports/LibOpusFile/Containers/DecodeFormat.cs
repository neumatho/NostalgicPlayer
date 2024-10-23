/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.OpusFile.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public enum DecodeFormat
	{
		/// <summary>
		/// Indicates that the decoding callback should produce signed 16-bit
		/// native-endian output samples
		/// </summary>
		Short = 7008,

		/// <summary>
		/// Indicates that the decoding callback should produce 32-bit
		/// native-endian float samples
		/// </summary>
		Float = 7040
	}
}
