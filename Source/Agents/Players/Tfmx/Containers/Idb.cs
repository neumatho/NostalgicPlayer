/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.Tfmx.Containers
{
	/// <summary>
	/// </summary>
	internal class Idb : IDeepCloneable<Idb>
	{
		public ushort[] Cue { get; set; } = new ushort[4];

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public Idb MakeDeepClone()
		{
			Idb clone = (Idb)MemberwiseClone();

			clone.Cue = ArrayHelper.CloneArray(Cue);

			return clone;
		}
	}
}
