/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64.Vic_II
{
	/// <summary>
	/// Lightpen emulation.
	/// Does not reflect model differences
	/// </summary>
	internal class Lightpen
	{
		/// <summary>
		/// Last VIC raster line
		/// </summary>
		private uint lastLine;

		/// <summary>
		/// VIC cycles per line
		/// </summary>
		private uint cyclesPerLine;

		/// <summary>
		/// X coordinate
		/// </summary>
		private uint lpX;

		/// <summary>
		/// Y coordinate
		/// </summary>
		private uint lpY;

		/// <summary>
		/// Has lightpen IRQ been triggered in this frame already?
		/// </summary>
		private bool isTriggered;

		/********************************************************************/
		/// <summary>
		/// Set VIC screen size
		/// </summary>
		/********************************************************************/
		public void SetScreenSize(uint height, uint width)
		{
			lastLine = height - 1;
			cyclesPerLine = width;
		}



		/********************************************************************/
		/// <summary>
		/// Reset the lightpen
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			lpX = 0;
			lpY = 0;
			isTriggered = false;
		}



		/********************************************************************/
		/// <summary>
		/// Return the low byte of x coordinate
		/// </summary>
		/********************************************************************/
		public uint8_t GetX()
		{
			return (uint8_t)lpX;
		}



		/********************************************************************/
		/// <summary>
		/// Return the low byte of y coordinate
		/// </summary>
		/********************************************************************/
		public uint8_t GetY()
		{
			return (uint8_t)lpY;
		}



		/********************************************************************/
		/// <summary>
		/// Retrigger lightpen on vertical blank
		/// </summary>
		/********************************************************************/
		public bool Retrigger()
		{
			if (isTriggered)
				return false;

			isTriggered = true;

			switch (cyclesPerLine)
			{
				default:
				case 63:
				{
					lpX = 0xd1;
					break;
				}

				case 65:
				{
					lpX = 0xd5;
					break;
				}
			}

			lpY = 0;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Trigger lightpen from CIA
		/// </summary>
		/********************************************************************/
		public bool Trigger(uint lineCycle, uint rasterY)
		{
			if (isTriggered)
				return false;

			isTriggered = true;

			// Don't latch on the last line, except on the first cycle
			if ((rasterY == lastLine) && (lineCycle > 0))
				return false;

			// Latch current coordinates
			lpX = (uint)GetXPos(lineCycle) + 2;	// + 1 for MOS 85XX
			lpY = rasterY;

			// On 6569R1 and 6567R56A the interrupt is triggered only
			// when the line is low on the first cycle of the frame
			return true;	// false for old chip revisions
		}



		/********************************************************************/
		/// <summary>
		/// Untrigger lightpen from CIA
		/// </summary>
		/********************************************************************/
		public void Untrigger()
		{
			isTriggered = false;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Transform line cycle into x coordinate
		/// </summary>
		/********************************************************************/
		private uint8_t GetXPos(uint lineCycle)
		{
			// FIXME: on NTSC the xpos is not incremented at lineCycle 61
			if (lineCycle < 13)
				lineCycle += cyclesPerLine;

			lineCycle -= 13;

			// On NTSC the xpos is not incremented at lineCycle 61
			if ((cyclesPerLine == 65) && (lineCycle > (61 - 13)))
				lineCycle--;

			return (uint8_t)(lineCycle << 2);
		}
		#endregion
	}
}
