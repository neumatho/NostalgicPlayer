/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibMpg123.Containers
{
	/// <summary>
	/// Separate frame header structure for safe decoding of headers without
	/// modifying the main frame struct before we are sure that we can read a
	/// frame into it
	/// </summary>
	internal struct Frame_Header
	{
		public c_int Lay;

		// Lots of flags that could share storage, should reform that
		public c_int Lsf;				// 0: MPEG 1.0; 1: MPEG 2.0/2.5 -- both used as bool and array index!
		public bool Mpeg25;
		public bool Error_Protection;
		public c_int BitRate_Index;
		public c_int Sampling_Frequency;
		public c_int Padding;
		public bool Extension;
		public Mode Mode;
		public c_int Mode_Ext;
		public bool Copyright;
		public bool Original;
		public c_int Emphasis;

		// Even 16 bit int is enough for MAXFRAMESIZE
		public c_int FrameSize;			// Computed frame size
		public bool FreeFormat;
		public c_int FreeFormat_FrameSize;

		// Derived from header and checked against the above
		public c_int SSize;
	}
}
