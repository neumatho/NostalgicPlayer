/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundBase.SampleConversion
{
	/// <summary>
	/// 
	/// </summary>
	internal readonly struct ClipFixed<TFractionalBits, TClipOutput> : ISampleClip<int32> where TFractionalBits : IFractionalBits where TClipOutput : IClipOutput
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int32 Clip(int32 val)
		{
			if (TClipOutput.ClipOutput)
			{
				int32 clip_Max = (1 << TFractionalBits.FractionalBits) - 1;
				int32 clip_Min = 0 - (1 << TFractionalBits.FractionalBits);

				if (val < clip_Min)
					val = clip_Min;

				if (val > clip_Max)
					val = clip_Max;

				return val;
			}
			else
				return val;
		}
	}
}
