/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.SidPlay2.Containers
{
	/// <summary>
	/// Interface ID
	/// </summary>
	internal struct IId
	{
		private readonly uint d1;
		private readonly ushort d2;
		private readonly ushort d3;
		private readonly ushort d4;
		private readonly ushort d5;
		private readonly uint d6;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public IId(uint d1, ushort d2, ushort d3, ushort d4, ushort d5, uint d6)
		{
			this.d1 = d1;
			this.d2 = d2;
			this.d3 = d3;
			this.d4 = d4;
			this.d5 = d5;
			this.d6 = d6;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool operator ==(IId iid1, IId iid2)
		{
			return (iid1.d1 == iid2.d1) && (iid1.d2 == iid2.d2) && (iid1.d3 == iid2.d3) && (iid1.d4 == iid2.d4) && (iid1.d5 == iid2.d5) && (iid1.d6 == iid2.d6);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static bool operator !=(IId iid1, IId iid2)
		{
			return (iid1.d1 != iid2.d1) || (iid1.d2 != iid2.d2) || (iid1.d3 != iid2.d3) || (iid1.d4 != iid2.d4) || (iid1.d5 != iid2.d5) || (iid1.d6 != iid2.d6);
		}
	}
}
