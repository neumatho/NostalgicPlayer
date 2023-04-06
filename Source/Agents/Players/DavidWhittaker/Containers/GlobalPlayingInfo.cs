/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.DavidWhittaker.Containers
{
	/// <summary>
	/// Holds global information about the playing state
	/// </summary>
	internal class GlobalPlayingInfo : IDeepCloneable<GlobalPlayingInfo>
	{
		public sbyte Transpose;

		public ushort VolumeFadeSpeed;

		public ushort GlobalVolume;
		public byte GlobalVolumeFadeSpeed;
		public byte GlobalVolumeFadeCounter;

		public ushort SquareChangePosition;
		public bool SquareChangeDirection;

		public byte ExtraCounter;

		public byte DelayCounterSpeed;
		public ushort DelayCounter;

		public ushort Speed;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public GlobalPlayingInfo MakeDeepClone()
		{
			return (GlobalPlayingInfo)MemberwiseClone();
		}
	}
}
