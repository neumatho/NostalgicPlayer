/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
global using VlcBaseType = System.Int16;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// Union class
	/// </summary>
	public class VlcElem : IClearable
	{
		/// <summary>
		/// The struct is for use as ordinary VLC (with get_vlc2())
		/// </summary>
		public (
			VlcBaseType Sym,
			VlcBaseType Len
		) U1;

		/// <summary>
		/// This struct is for use as run-length VLC (with GET_RL_VLC)
		/// </summary>
		public (
			int16_t Level,
			int8_t Len8,
			uint8_t Run
		) U2;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			U1.Sym = 0;
			U1.Len = 0;

			U2.Level = 0;
			U2.Len8 = 0;
			U2.Run = 0;
		}
	}
}
