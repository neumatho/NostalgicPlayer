/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Player;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common
{
	/// <summary>
	/// 
	/// </summary>
	internal class Player_Data : IDeepCloneable<Player_Data>
	{
		public c_int Ord { get; set; }
		public c_int Pos { get; set; }
		public c_int Row { get; set; }
		public c_int Frame { get; set; }
		public c_int Speed { get; set; }
		public ref c_int Bpm => ref _Bpm;
		private c_int _Bpm;
		public Xmp_Mode Mode { get; set; }
		public Xmp_Flags Player_Flags { get; set; }
		public Xmp_Flags Flags { get; set; }

		public c_double Scan_Time_Factor { get; set; }		// m->time_factor for most recent scan
		public c_double Current_Time { get; set; }			// Current time based on scan time factor

		public c_int Loop_Count { get; set; }
		public c_int Sequence { get; set; }
		public byte[] Sequence_Control { get; set; } = new byte[Constants.Xmp_Max_Mod_Length];

		/// <summary>
		/// Music volume
		/// </summary>
		public c_int Master_Vol { get; set; }
		public ref c_int GVol => ref _GVol;
		private c_int _GVol;

		public Flow_Control Flow { get; set; } = new Flow_Control();

		public Scan_Data[] Scan { get; set; }

		public Channel_Data[] Xc_Data { get; set; }

		public c_int[] Channel_Vol { get; set; } = new c_int[Constants.Xmp_Max_Channels];
		public bool[] Channel_Mute { get; set; } = new bool[Constants.Xmp_Max_Channels];

		public Virt_Control Virt { get; set; } = new Virt_Control();

		public Xmp_Event[] Inject_Event { get; set; } = ArrayHelper.InitializeArray<Xmp_Event>(Constants.Xmp_Max_Channels);

		// ReSharper disable once InconsistentNaming
		public (
			c_int Consumed,
			c_int In_Size,
			CPointer<sbyte> In_Buffer
		) Buffer_Data;

		/// <summary>
		/// For IceTracker speed effect
		/// </summary>
		public c_int St26_Speed { get; set; }

		/// <summary>
		/// Amiga led filter
		/// </summary>
		public bool Filter { get; set; }

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
