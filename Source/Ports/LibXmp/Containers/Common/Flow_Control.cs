/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common
{
	/// <summary>
	/// 
	/// </summary>
	internal class Flow_Control : IDeepCloneable<Flow_Control>
	{
		public bool PBreak;
		public c_int Jump;
		public c_int Delay;
		public c_int JumpLine;
		public c_int Loop_Chn;
		public c_int Jump_In_Pat;

		public Pattern_Loop[] Loop;

		public c_int Num_Rows;
		public c_int End_Point;

		/// <summary>
		/// For IT pattern row delay
		/// </summary>
		public RowDelay_Flag RowDelay;
		public RowDelay_Flag RowDelay_Set;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public Flow_Control MakeDeepClone()
		{
			Flow_Control clone = (Flow_Control)MemberwiseClone();

			clone.Loop = ArrayHelper.CloneObjectArray(Loop);

			return clone;
		}
	}
}
