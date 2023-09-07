/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Lfo
{
	/// <summary>
	/// 
	/// </summary>
	internal class Lfo : IDeepCloneable<Lfo>
	{
		public c_int Type;
		public c_int Rate;
		public c_int Depth;
		public c_int Phase;

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
