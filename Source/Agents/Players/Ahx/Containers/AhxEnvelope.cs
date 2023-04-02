/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.Ahx.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class AhxEnvelope : IDeepCloneable<AhxEnvelope>
	{
		public int AFrames;
		public int AVolume;

		public int DFrames;
		public int DVolume;

		public int SFrames;

		public int RFrames;
		public int RVolume;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public AhxEnvelope MakeDeepClone()
		{
			return (AhxEnvelope)MemberwiseClone();
		}
	}
}
