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
	internal struct DecodeInt8Delta : ISampleConversion<int8>
	{
		private uint8 delta;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public DecodeInt8Delta()
		{
			delta = 0;
		}



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
			delta += inBuf[0];

			return (int8)delta;
		}
	}
}
