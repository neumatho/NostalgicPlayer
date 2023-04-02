/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.Tfmx.Containers
{
	/// <summary>
	/// </summary>
	internal class PdBlk : IDeepCloneable<PdBlk>
	{
		public ushort FirstPos;
		public ushort LastPos;
		public ushort CurrPos;
		public ushort PreScale;
		public Pdb[] p = ArrayHelper.InitializeArray<Pdb>(8);

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public PdBlk MakeDeepClone()
		{
			PdBlk clone = (PdBlk)MemberwiseClone();

			clone.p = ArrayHelper.CloneObjectArray(p);

			return clone;
		}
	}
}
