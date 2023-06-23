/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibMpg123.Containers
{
	/// <summary>
	/// Data structure for storing information about a frame of MPEG audio
	/// </summary>
	public class Mpg123_FrameInfo
	{
		/// <summary>
		/// The MPEG version (1.0/2.0/2.5)
		/// </summary>
		public Mpg123_Version Version;
		/// <summary>
		/// The MPEG Audio Layer (MP1/MP2/MP3)
		/// </summary>
		public c_int Layer;
		/// <summary>
		/// The sampling rate in Hz
		/// </summary>
		public c_long Rate;
		/// <summary>
		/// The audio mode (Mono, Stereo, Joint-stereo, Dual channel)
		/// </summary>
		public Mpg123_Mode Mode;
		/// <summary>
		/// The mode extension bit flag
		/// </summary>
		public c_int Mode_Ext;
		/// <summary>
		/// The size of the frame (in bytes, including header)
		/// </summary>
		public c_int FrameSize;
		/// <summary>
		/// MPEG Audio flag bits
		/// </summary>
		public Mpg123_Flags Flags;
		/// <summary>
		/// The emphasis type
		/// </summary>
		public c_int Emphasis;
		/// <summary>
		/// Bitrate of the frame (kbps)
		/// </summary>
		public c_int BitRate;
		/// <summary>
		/// The target average bitrate
		/// </summary>
		public c_int Abr_Rate;
		/// <summary>
		/// The VBR mode
		/// </summary>
		public Mpg123_Vbr Vbr;
	}
}
