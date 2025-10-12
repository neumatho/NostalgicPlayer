/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibVorbis.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibVorbis.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class VorbisInfo : IClearable
	{
		/// <summary></summary>
		public c_int version;
		/// <summary></summary>
		public c_int channels;
		/// <summary></summary>
		public c_long rate;

		// The below bitrate declarations are *hints*.
		// Combinations of the three values carry the following implications:
		//
		// all three set to the same value:
		//   implies a fixed rate bitstream
		// only nominal set:
		//   implies a VBR stream that averages the nominal bitrate. No hard
		//   upper/lower limit
		// upper and or lower set:
		//   implies a VBR bitstream that obeys the bitrate limits. nominal
		//   may also be set to give a nominal rate.
		// none set:
		//   the coder does not care to speculate
		/// <summary></summary>
		public c_long bitrate_upper;
		/// <summary></summary>
		public c_long bitrate_nominal;
		/// <summary></summary>
		public c_long bitrate_lower;
		/// <summary></summary>
		public c_long bitrate_window;

		/// <summary></summary>
		public ICodecSetup codec_setup;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			version = 0;
			channels = 0;
			rate = 0;
			bitrate_upper = 0;
			bitrate_nominal = 0;
			bitrate_lower = 0;
			bitrate_window = 0;
			codec_setup = null;
		}
	}
}
