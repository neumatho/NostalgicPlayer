/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSidFp.Array
{
	/// <summary>
	/// 
	/// </summary>
	internal class Matrix<T>
	{
		private readonly T[][] data;
		private readonly uint x, y;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Matrix(uint x, uint y)
		{
			data = new T[x][];

			for (uint i = 0; i < x; i++)
				data[i] = new T[y];

			this.x = x;
			this.y = y;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public T[] this[uint a] => data[a];
	}
}
