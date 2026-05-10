/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibReSidFp.Array
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
		public Matrix(uint new_x, uint new_y)
		{
			data = new T[new_x][];

			for (uint i = 0; i < new_x; i++)
				data[i] = new T[new_y];

			x = new_x;
			y = new_y;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public T[] this[uint a] => data[a];
	}
}
