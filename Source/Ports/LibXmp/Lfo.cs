/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;

namespace Polycode.NostalgicPlayer.Ports.LibXmp
{
	/// <summary>
	/// 
	/// </summary>
	internal class Lfo
	{
		private const c_int WaveForm_Size = 64;

		private static readonly c_int[] sine_Wave = new c_int[WaveForm_Size]
		{
			   0,  24,  49,  74,  97, 120, 141, 161, 180, 197, 212, 224,
			 235, 244, 250, 253, 255, 253, 250, 244, 235, 224, 212, 197,
			 180, 161, 141, 120,  97,  74,  49,  24,   0, -24, -49, -74,
			 -97,-120,-141,-161,-180,-197,-212,-224,-235,-244,-250,-253,
			-255,-253,-250,-244,-235,-224,-212,-197,-180,-161,-141,-120,
			 -97, -74, -49, -24
		};

		private readonly Xmp_Context ctx;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Lfo(Xmp_Context ctx)
		{
			this.ctx = ctx;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Lfo_Get(Containers.Lfo.Lfo lfo, bool is_Vibrato)
		{
			Module_Data m = ctx.M;

			switch (m.Read_Event_Type)
			{
				case Read_Event.St3:
					return Get_Lfo_St3(lfo);

				case Read_Event.Ft2:
				{
					if (is_Vibrato)
						return Get_Lfo_Ft2(lfo);
					else
						return Get_Lfo_Mod(lfo);
				}

				case Read_Event.It:
					return Get_Lfo_It(lfo);

				default:
					return Get_Lfo_Mod(lfo);
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Lfo_Update(Containers.Lfo.Lfo lfo)
		{
			lfo.Phase += lfo.Rate;
			lfo.Phase %= WaveForm_Size;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Lfo_Set_Phase(Containers.Lfo.Lfo lfo, c_int phase)
		{
			lfo.Phase = phase;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Lfo_Set_Depth(Containers.Lfo.Lfo lfo, c_int depth)
		{
			lfo.Depth = depth;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Lfo_Set_Rate(Containers.Lfo.Lfo lfo, c_int rate)
		{
			lfo.Rate = rate;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Lfo_Set_Waveform(Containers.Lfo.Lfo lfo, c_int type)
		{
			lfo.Type = type;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Lfo_Mod(Containers.Lfo.Lfo lfo)
		{
			if (lfo.Rate == 0)
				return 0;

			c_int val;

			switch (lfo.Type)
			{
				// Sine
				case 0:
				{
					val = sine_Wave[lfo.Phase];
					break;
				}

				// Ramp down
				case 1:
				{
					val = 255 - (lfo.Phase << 3);
					break;
				}

				// Square
				case 2:
				{
					val = lfo.Phase < WaveForm_Size / 2 ? 255 : -255;
					break;
				}

				// Random
				case 3:
				{
					val = (RandomGenerator.GetRandomNumber() & 0x1ff) - 256;
					break;
				}

				// 669 vibrato
				case 669:
				{
					val = lfo.Phase & 1;
					break;
				}

				default:
					return 0;
			}

			return val * lfo.Depth;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Lfo_St3(Containers.Lfo.Lfo lfo)
		{
			if (lfo.Rate == 0)
				return 0;

			// S3M square
			if (lfo.Type == 2)
			{
				c_int val = lfo.Phase < WaveForm_Size / 2 ? 255 : 0;

				return val * lfo.Depth;
			}

			return Get_Lfo_Mod(lfo);
		}



		/********************************************************************/
		/// <summary>
		/// From OpenMPT VibratoWaveforms.xm:
		/// "Generally the vibrato and tremolo tables are identical to those
		/// that ProTracker uses, but the vibrato’s “ramp down” table is
		/// upside down."
		/// </summary>
		/********************************************************************/
		private c_int Get_Lfo_Ft2(Containers.Lfo.Lfo lfo)
		{
			if (lfo.Rate == 0)
				return 0;

			// FT2 ramp
			if (lfo.Type == 1)
			{
				c_int phase = (lfo.Phase + (WaveForm_Size >> 1)) % WaveForm_Size;
				c_int val = (phase << 3) - 255;

				return val * lfo.Depth;
			}

			return Get_Lfo_Mod(lfo);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Get_Lfo_It(Containers.Lfo.Lfo lfo)
		{
			if (lfo.Rate == 0)
				return 0;

			return Get_Lfo_St3(lfo);
		}
		#endregion
	}
}
