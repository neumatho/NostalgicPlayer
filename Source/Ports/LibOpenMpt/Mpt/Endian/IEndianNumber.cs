/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Numerics;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Endian
{
	/// <summary>
	/// Common interface for numbers with a defined endianness
	/// </summary>
	internal interface IEndianNumber<TValue> where TValue : unmanaged, INumberBase<TValue>
	{
		TValue Value { get; }
	}
}
