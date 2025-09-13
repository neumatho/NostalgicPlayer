/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Lfo
{
	/// <summary>
	/// 
	/// </summary>
	internal class Lfo : IDeepCloneable<Lfo>
	{
		public c_int Type { get; set; }
		public c_int Rate { get; set; }
		public c_int Depth { get; set; }
		public c_int Phase { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public Lfo MakeDeepClone()
		{
			return (Lfo)MemberwiseClone();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			Type = 0;
			Rate = 0;
			Depth = 0;
			Phase = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void CopyFrom(Lfo other)
		{
			Type = other.Type;
			Rate = other.Rate;
			Depth = other.Depth;
			Phase = other.Phase;
		}
	}
}
