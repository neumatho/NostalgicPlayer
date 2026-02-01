/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class AvSubtitleRect
	{
		/// <summary>
		/// Top left corner of pict, undefined when pict is not set
		/// </summary>
		public c_int X;

		/// <summary>
		/// Top left corner of pict, undefined when pict is not set
		/// </summary>
		public c_int Y;

		/// <summary>
		/// Width of pict, undefined when pict is not set
		/// </summary>
		public c_int W;

		/// <summary>
		/// Height of pict, undefined when pict is not set
		/// </summary>
		public c_int H;

		/// <summary>
		/// Number of colors in pict, undefined when pict is not set
		/// </summary>
		public c_int Nb_Colors;

		/// <summary>
		/// data+linesize for the bitmap of this subtitle.
		/// Can be set for text/ass as well once they are rendered
		/// </summary>
		public readonly CPointer<uint8_t>[] Data = new CPointer<uint8_t>[4];

		/// <summary>
		/// 
		/// </summary>
		public readonly c_int[] LineSize = new c_int[4];

		/// <summary>
		/// 
		/// </summary>
		public c_int Flags;

		/// <summary>
		/// 
		/// </summary>
		public AvSubtitleType Type;

		/// <summary>
		/// 0 terminated plain UTF-8 text
		/// </summary>
		public CPointer<char> Text;

		/// <summary>
		/// 0 terminated ASS/SSA compatible event line.
		/// The presentation of this is unaffected by the other values in this
		/// struct
		/// </summary>
		public CPointer<char> Ass;
	}
}
