/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Player;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common
{
	/// <summary>
	/// 
	/// </summary>
	internal class Player_Data
	{
		public c_int Ord;
		public c_int Pos;
		public c_int Row;
		public c_int Frame;
		public c_int Speed;
		public c_int Bpm;
		public Xmp_Mode Mode;
		public Xmp_Flags Player_Flags;
		public Xmp_Flags Flags;

		public c_double Current_Time;
		public c_double Frame_Time;

		public c_int Loop_Count;
		public c_int Sequence;
		public byte[] Sequence_Control = new byte[Constants.Xmp_Max_Mod_Length];

		/// <summary>
		/// Music volume
		/// </summary>
		public c_int Master_Vol;
		public c_int GVol;

		public Flow_Control Flow = new Flow_Control();

		public Scan_Data[] Scan;

		public Channel_Data[] Xc_Data;

		public c_int[] Channel_Vol = new c_int[Constants.Xmp_Max_Channels];
		public bool[] Channel_Mute = new bool[Constants.Xmp_Max_Channels];

		public Virt_Control Virt = new Virt_Control();

		public Xmp_Event[] Inject_Event = ArrayHelper.InitializeArray<Xmp_Event>(Constants.Xmp_Max_Channels);

		public (
			c_int Consumed,
			c_int In_Size,
			sbyte[] In_Buffer
		) Buffer_Data;

		/// <summary>
		/// For IceTracker speed effect
		/// </summary>
		public c_int St26_Speed;

		/// <summary>
		/// Amiga led filter
		/// </summary>
		public bool Filter;

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public Player_Data MakeDeepClone()
		{
			Player_Data clone = (Player_Data)MemberwiseClone();

			clone.Flow = Flow.MakeDeepClone();
			clone.Virt = Virt.MakeDeepClone();

			clone.Sequence_Control = ArrayHelper.CloneArray(Sequence_Control);
			clone.Channel_Vol = ArrayHelper.CloneArray(Channel_Vol);
			clone.Channel_Mute = ArrayHelper.CloneArray(Channel_Mute);

			clone.Xc_Data = ArrayHelper.CloneObjectArray(Xc_Data);
			clone.Inject_Event = ArrayHelper.CloneObjectArray(Inject_Event);

			return clone;
		}
	}
}
