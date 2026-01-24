/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

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

		/// <summary>
		/// Pattern loop destination, -1 for none
		/// </summary>
		public c_int Loop_Dest;

		/// <summary>
		/// Last loop param for Digital Tracker
		/// </summary>
		public c_int Loop_Param;

		/// <summary>
		/// Global loop target for S3M et al.
		/// </summary>
		public c_int Loop_Start;

		/// <summary>
		/// Global loop count for S3M et al.
		/// </summary>
		public c_int Loop_Count;

		/// <summary>
		/// Number of active loops for scan
		/// </summary>
		public c_int Loop_Active_Num;

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
