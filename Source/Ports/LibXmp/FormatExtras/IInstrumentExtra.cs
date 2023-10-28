/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.FormatExtras
{
	/// <summary>
	/// 
	/// </summary>
	public interface IInstrumentExtra : IDeepCloneable<IInstrumentExtra>
	{
		/// <summary></summary>
		IInstrumentExtraInfo Instrument_Extras { get; }
	}
}
