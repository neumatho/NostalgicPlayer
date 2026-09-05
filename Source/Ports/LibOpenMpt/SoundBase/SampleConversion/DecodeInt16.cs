/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundBase.SampleConversion
{
	/// <summary>
	/// 
	/// </summary>
	internal struct DecodeInt16<TOffset, TOrder> : ISampleConversion<int16> where TOffset : IOffset16 where TOrder : IByteOrder16
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static size_t Input_Inc => 2;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int16 Convert(CPointer<uint8> inBuf)
		{
			return (int16)((inBuf[TOrder.Lo] | (inBuf[TOrder.Hi] << 8)) - TOffset.Offset);
		}
	}
}
