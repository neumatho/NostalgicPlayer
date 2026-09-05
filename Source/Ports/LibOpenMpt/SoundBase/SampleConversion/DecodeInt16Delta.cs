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
	internal struct DecodeInt16Delta<TOrder> : ISampleConversion<int16> where TOrder : IByteOrder16
	{
		private uint16 delta;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public DecodeInt16Delta()
		{
			delta = 0;
		}



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
			delta += (uint16)(inBuf[TOrder.Lo] | (uint16)(inBuf[TOrder.Hi] << 8));

			return (int16)delta;
		}
	}
}
