/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpus.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public enum Bandwidth
	{
		/// <summary></summary>
		None = 0,
		/// <summary></summary>
		Narrowband = 1101,	// < 4 kHz bandpass
		/// <summary></summary>
		Mediumband,			// < 6 kHz bandpass
		/// <summary></summary>
		Wideband,			// < 8 kHz bandpass
		/// <summary></summary>
		Superwideband,		// < 12 kHz bandpass
		/// <summary></summary>
		Fullband			// < 20 kHz bandpass
	}
}
