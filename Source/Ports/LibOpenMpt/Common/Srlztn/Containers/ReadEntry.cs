/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.Srlztn.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class ReadEntry : IDeepCloneable<ReadEntry>
	{
		/// <summary>
		/// Index of id start in ID array
		/// </summary>
		public size_t nIdPos = 0;

		/// <summary>
		/// Entry start position
		/// </summary>
		public long RPosStart = 0;

		/// <summary>
		/// Entry size
		/// </summary>
		public size_t nSize = Serialization_Utils.InvalidDataSize;

		/// <summary>
		/// Length of id
		/// </summary>
		public uint16 nIdLength = 0;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public ReadEntry MakeDeepClone()
		{
			return (ReadEntry)MemberwiseClone();
		}
	}
}
