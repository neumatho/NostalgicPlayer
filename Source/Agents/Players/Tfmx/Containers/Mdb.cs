/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.Tfmx.Containers
{
	/// <summary>
	/// </summary>
	internal class Mdb : IDeepCloneable<Mdb>
	{
		public bool PlayerEnable;
		public bool EndFlag;
		public ushort SpeedCnt;
		public ushort CiaSave;
		public bool PlayPattFlag;
		public sbyte MasterVol;
		public sbyte FadeDest;
		public sbyte FadeTime;
		public sbyte FadeReset;
		public sbyte FadeSlope;
		public short TrackLoop;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public Mdb MakeDeepClone()
		{
			return (Mdb)MemberwiseClone();
		}
	}
}
