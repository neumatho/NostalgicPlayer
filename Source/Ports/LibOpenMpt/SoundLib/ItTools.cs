/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Definition of IT file structures and helper functions
	/// </summary>
	internal static class ItTools
	{
		// Pattern row parsing, the channel data is read to obtain
		// number of channels active in the pattern. These bit masks are
		// to blank out sections of the byte of data being read
		public const ChannelIndex It_BitMask_PatternChanField = 0x7f;
		public const ChannelIndex It_BitMask_PatternChanMask = 0x3f;
		public const ChannelIndex It_BitMask_PatternChanEnabled = 0x80;
		public const ChannelIndex It_BitMask_PatternChanUsed = 0x0f;

		public static readonly int32 SchismTrackerEpoch = new SchismVersionFromDate(2009, 10, 31).Date;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static uint32 DecodeItEditTimer(uint16 cwtv, uint32 editTime)
		{
			if ((cwtv & 0xfff) >= 0x0208)
			{
				editTime ^= 0x4954524b;	// 'ITRK'
				editTime = Bit.Rotr(editTime, 7);
				editTime = ~editTime + 1;
				editTime = Bit.Rotl(editTime, 4);
				editTime ^= 0x4a54484c;	// 'JTHL'
			}

			return editTime;
		}
	}
}
