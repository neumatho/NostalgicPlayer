/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// Return values for GetLength()
	/// </summary>
	internal class GetLengthType : IDeepCloneable<GetLengthType>
	{
		/// <summary>
		/// Total time in seconds
		/// </summary>
		public c_double Duration = 0.0;

		/// <summary>
		/// First row to play after module loops (or last parsed row if
		/// target is specified; equal to target if it was found)
		/// </summary>
		public RowIndex RestartRow = Snd_Def.RowIndex_Invalid;

		/// <summary>
		/// Last row played before module loops (UNDEFINED if a target is specified)
		/// </summary>
		public RowIndex EndRow = Snd_Def.RowIndex_Invalid;

		/// <summary>
		/// First row of parsed subsong
		/// </summary>
		public RowIndex StartRow = 0;

		/// <summary>
		/// First row to play after module loops (see restartRow remark)
		/// </summary>
		public OrderIndex RestartOrder = Snd_Def.OrderIndex_Invalid;

		/// <summary>
		/// Last order played before module loops (UNDEFINED if a target is specified)
		/// </summary>
		public OrderIndex EndOrder = Snd_Def.OrderIndex_Invalid;

		/// <summary>
		/// First order of parsed subsong
		/// </summary>
		public OrderIndex StartOrder = 0;

		/// <summary>
		/// True if the specified order/row combination or duration has been
		/// reached while going through the module
		/// </summary>
		public bool TargetReached = false;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public GetLengthType MakeDeepClone()
		{
			return (GetLengthType)MemberwiseClone();
		}
	}
}
