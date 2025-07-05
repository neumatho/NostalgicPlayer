/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;

namespace Polycode.NostalgicPlayer.Ports.LibSidPlayFp
{
	/// <summary>
	/// 
	/// </summary>
	internal class SimpleMixer
	{
		private const int_least32_t Scale_Factor = 1 << 16;

		private const double Sqrt_2 = 1.41421356237;
		private const double Sqrt_3 = 1.73205080757;

		private static readonly int_least32_t[] scale =
		[
			Scale_Factor,									// 1 chip, no scale
			(int_least32_t)((1.0 / Sqrt_2) * Scale_Factor),	// 2 chips, scale by sqrt(2)
			(int_least32_t)((1.0 / Sqrt_3) * Scale_Factor),	// 3 chips, scale by sqrt(3)
		];

		private delegate int_least32_t MixerFunc();

		private readonly List<short[]> buffers = new List<short[]>();

		private readonly int_least32_t[] iSamples;

		private readonly List<MixerFunc> mix = new List<MixerFunc>();

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SimpleMixer(bool stereo, short[][] buffers, int chips)
		{
			switch (chips)
			{
				case 1:
				{
					mix.Add(stereo ? Stereo_OneChip : () => Mono(1));

					if (stereo)
						mix.Add(Stereo_OneChip);

					break;
				}

				case 2:
				{
					mix.Add(stereo ? Stereo_Ch1_TwoChips : () => Mono(2));

					if (stereo)
						mix.Add(Stereo_Ch2_TwoChips);

					break;
				}

				case 3:
				{
					mix.Add(stereo ? Stereo_Ch1_ThreeChips : () => Mono(3));

					if (stereo)
						mix.Add(Stereo_Ch2_ThreeChips);

					break;
				}
			}

			iSamples = new int_least32_t[chips];

			for (int i = 0; i < chips; i++)
				this.buffers.Add(buffers[i]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint DoMix(short[] buffer, uint samples)
		{
			uint j = 0;

			for (uint i = 0; i < samples; i++)
			{
				for (size_t k = 0; k < (size_t)buffers.Count; k++)
				{
					short[] buf = buffers[(int)k];
					iSamples[(int)k] = buf[i];
				}

				foreach (MixerFunc mix in this.mix)
				{
					int_least32_t tmp = mix();
					buffer[j++] = (short)tmp;
				}
			}

			return j;
		}

		#region Private methods

		//
		// Channel matrix
		//
		//   C1
		// L 1.0
		// R 1.0
		//
		//   C1    C2
		// L 1.0   0.5
		// R 0.5   1.0
		//
		//   C1    C2    C3
		// L 1.0   1.0   0.5
		// R 0.5   1.0   1.0
		//

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private int_least32_t Mono(int chips)
		{
			int_least32_t res = 0;

			for (int i = 0; i < chips; i++)
				res += iSamples[i];

			return res * scale[chips - 1] / Scale_Factor;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private int_least32_t Stereo_OneChip()
		{
			return iSamples[0];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private int_least32_t Stereo_Ch1_TwoChips()
		{
			return (int_least32_t)((iSamples[0] + 0.5 * iSamples[1]) * scale[1] / Scale_Factor);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private int_least32_t Stereo_Ch2_TwoChips()
		{
			return (int_least32_t)((0.5 * iSamples[0] + iSamples[1]) * scale[1] / Scale_Factor);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private int_least32_t Stereo_Ch1_ThreeChips()
		{
			return (int_least32_t)((iSamples[0] + iSamples[1] + 0.5 * iSamples[2]) * scale[2] / Scale_Factor);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private int_least32_t Stereo_Ch2_ThreeChips()
		{
			return (int_least32_t)((0.5 * iSamples[0] + iSamples[1] + iSamples[2]) * scale[2] / Scale_Factor);
		}
		#endregion
	}
}
