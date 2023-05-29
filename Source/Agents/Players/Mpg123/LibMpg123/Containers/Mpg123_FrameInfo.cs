/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.Mpg123.LibMpg123.Containers
{
	/// <summary>
	/// Data structure for storing information about a frame of MPEG audio
	/// </summary>
	internal class Mpg123_FrameInfo
	{
		public Mpg123_Version Version;	// < The MPEG version (1.0/2.0/2.5)
		public c_int Layer;				// < The MPEG Audio Layer (MP1/MP2/MP3)
		public c_long Rate;				// < The sampling rate in Hz
		public Mpg123_Mode Mode;		// < The audio mode (Mono, Stereo, Joint-stereo, Dual channel)
		public c_int Mode_Ext;			// < The mode extension bit flag
		public c_int FrameSize;			// < The size of the frame (in bytes, including header)
		public Mpg123_Flags Flags;		// < MPEG Audio flag bits
		public c_int Emphasis;			// < The emphasis type
		public c_int BitRate;				// < Bitrate of the frame (kbps)
		public c_int Abr_Rate;			// < The target average bitrate
		public Mpg123_Vbr Vbr;			// < The VBR mode
	}
}
