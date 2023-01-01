/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation
{
	/// <summary>
	/// Make sure that the variable is between given range
	/// </summary>
	internal class LimVar<T> where T : IComparable
	{
		private readonly T minVal;
		private readonly T maxVal;

		private T val;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public LimVar(T min, T max)
		{
			minVal = min;
			maxVal = max;

			Value = default;
		}



		/********************************************************************/
		/// <summary>
		/// Assign to a new value
		/// </summary>
		/********************************************************************/
		public T Value
		{
			get => val;

			set
			{
				if (value.CompareTo(minVal) < 0)
					val = minVal;
				else if (value.CompareTo(maxVal) > 0)
					val = maxVal;
				else
					val = value;
			}
		}
	}
}
