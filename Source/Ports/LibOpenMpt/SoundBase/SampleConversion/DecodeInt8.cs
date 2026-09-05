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
	internal struct DecodeInt8 : ISampleConversion<int8>
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static size_t Input_Inc => 1;



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int8 Convert(CPointer<uint8> inBuf)
		{
			return (int8)inBuf[0];
		}
	}
}
