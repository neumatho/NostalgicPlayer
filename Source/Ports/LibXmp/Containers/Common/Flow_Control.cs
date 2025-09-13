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
		public bool PBreak { get; set; }
		public c_int Jump { get; set; }
		public c_int Delay { get; set; }
		public c_int JumpLine { get; set; }

		/// <summary>
		/// Pattern loop destination, -1 for none
		/// </summary>
		public c_int Loop_Dest { get; set; }

		/// <summary>
		/// Last loop param for Digital Tracker
		/// </summary>
		public c_int Loop_Param { get; set; }

		/// <summary>
		/// Global loop target for S3M et al.
		/// </summary>
		public ref c_int Loop_Start => ref _Loop_Start;
		private c_int _Loop_Start;

		/// <summary>
		/// Global loop count for S3M et al.
		/// </summary>
		public ref c_int Loop_Count => ref _Loop_Count;
		private c_int _Loop_Count;

		/// <summary>
		/// Number of active loops for scan
		/// </summary>
		public c_int Loop_Active_Num { get; set; }

		public c_int Jump_In_Pat { get; set; }

		public Pattern_Loop[] Loop { get; set; }

		public c_int Num_Rows { get; set; }
		public c_int End_Point { get; set; }

		/// <summary>
		/// For IT pattern row delay
		/// </summary>
		public RowDelay_Flag RowDelay { get; set; }
		public RowDelay_Flag RowDelay_Set { get; set; }

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
