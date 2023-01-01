/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64.Vic_II
{
	/// <summary>
	/// Sprites handling
	/// </summary>
	internal class Sprites
	{
		private const int SPRITES = 8;

		private const int enable = 0x15;
		private const int y_expansion = 0x17;

		private readonly uint8_t[] regs;

		private uint8_t exp_flop;
		private uint8_t dma;
		private uint8_t[] mc_base = new uint8_t[SPRITES];
		private uint8_t[] mc = new uint8_t[SPRITES];

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Sprites(uint8_t[] regs)
		{
			this.regs = regs;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			exp_flop = 0xff;
			dma = 0;

			Array.Clear(mc_base, 0, mc_base.Length);
			Array.Clear(mc, 0, mc.Length);
		}



		/********************************************************************/
		/// <summary>
		/// Update mc values in one pass after the DMA has been processed
		/// </summary>
		/********************************************************************/
		public void UpdateMc()
		{
			uint8_t mask = 1;

			for (uint i = 0; i < SPRITES; i++, mask <<= 1)
			{
				if ((dma & mask) != 0)
					mc[i] = (uint8_t)((mc[i] + 3) & 0x3f);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Update mc base value
		/// </summary>
		/********************************************************************/
		public void UpdateMcBase()
		{
			uint8_t mask = 1;

			for (uint i = 0; i < SPRITES; i++, mask <<= 1)
			{
				if ((exp_flop & mask) != 0)
				{
					mc_base[i] = mc[i];
					if (mc_base[i] == 0x3f)
						dma &= (uint8_t)~mask;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Calculate sprite expansion
		/// </summary>
		/********************************************************************/
		public void CheckExp()
		{
			exp_flop ^= (uint8_t)(dma & regs[y_expansion]);
		}



		/********************************************************************/
		/// <summary>
		/// Check if sprite is displayed
		/// </summary>
		/********************************************************************/
		public void CheckDisplay()
		{
			for (uint i = 0; i < SPRITES; i++)
				mc[i] = mc_base[i];
		}



		/********************************************************************/
		/// <summary>
		/// Calculate sprite DMA
		/// </summary>
		/********************************************************************/
		public void CheckDma(uint rasterY, uint8_t[] regs)
		{
			uint8_t y = (uint8_t)(rasterY & 0xff);
			uint8_t mask = 1;

			for (uint i = 0; i < SPRITES; i++, mask <<= 1)
			{
				if (((regs[enable] & mask) != 0) && (y == regs[(i << 1) + 1]) && ((dma & mask) == 0))
				{
					dma |= mask;
					mc_base[i] = 0;
					exp_flop |= mask;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Calculate line crunch
		/// </summary>
		/********************************************************************/
		public void LineCrunch(uint8_t data, uint lineCycle)
		{
			uint8_t mask = 1;

			for (uint i = 0; i < SPRITES; i++, mask <<= 1)
			{
				if (((data & mask) == 0) && ((exp_flop & mask) == 0))
				{
					// Sprite crunch
					if (lineCycle == 14)
					{
						uint8_t mc_i = mc[i];
						uint8_t mcBase_i = mc_base[i];

						mc[i] = (uint8_t)((0x2a & (mcBase_i & mc_i)) | (0x15 & (mcBase_i | mc_i)));

						// mcbase will be set from mc on the following clock call
					}

					exp_flop |= mask;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Check if DMA is active for sprites
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsDma(uint val)
		{
			return (dma & val) != 0;
		}
	}
}
