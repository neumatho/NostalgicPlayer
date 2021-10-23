/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Runtime.CompilerServices;

namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.LibSidPlayFp.C64.Cpu
{
	/// <summary>
	/// Processor Status Register
	/// </summary>
	internal class Flags
	{
		private bool c;		// Carry
		private bool z;		// Zero
		private bool i;		// Interrupt disabled
		private bool d;		// Decimal
		private bool v;		// Overflow
		private bool n;		// Negative

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reset()
		{
			c = z = i = d = v = n = false;
		}



		/********************************************************************/
		/// <summary>
		/// Set N and Z flag values
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetNZ(uint8_t value)
		{
			z = value == 0;
			n = (value & 0x80) != 0;
		}



		/********************************************************************/
		/// <summary>
		/// Get status register value
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint8_t Get()
		{
			uint8_t sr = 0;

			if (c)
				sr |= 0x01;

			if (z)
				sr |= 0x02;

			if (i)
				sr |= 0x04;

			if (d)
				sr |= 0x08;

			if (v)
				sr |= 0x40;

			if (n)
				sr |= 0x80;

			return sr;
		}



		/********************************************************************/
		/// <summary>
		/// Set status register value
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(uint8_t sr)
		{
			c = (sr & 0x01) != 0;
			z = (sr & 0x02) != 0;
			i = (sr & 0x04) != 0;
			d = (sr & 0x08) != 0;
			v = (sr & 0x40) != 0;
			n = (sr & 0x80) != 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool GetN()
		{
			return n;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool GetC()
		{
			return c;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool GetD()
		{
			return d;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool GetZ()
		{
			return z;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool GetV()
		{
			return v;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool GetI()
		{
			return i;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetN(bool f)
		{
			n = f;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetC(bool f)
		{
			c = f;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetD(bool f)
		{
			d = f;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetZ(bool f)
		{
			z = f;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetV(bool f)
		{
			v = f;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetI(bool f)
		{
			i = f;
		}
	}
}
