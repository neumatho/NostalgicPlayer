/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp
{
	/// <summary>
	/// Current frame information
	/// </summary>
	public class Xmp_Frame_Info
	{
		/// <summary>
		/// Current position
		/// </summary>
		public c_int Pos { get; internal set; }

		/// <summary>
		/// Current pattern
		/// </summary>
		public c_int Pattern { get; internal set; }

		/// <summary>
		/// Current row in pattern
		/// </summary>
		public c_int Row { get; internal set; }

		/// <summary>
		/// Number of rows in current pattern
		/// </summary>
		public c_int Num_Rows { get; internal set; }

		/// <summary>
		/// Current frame
		/// </summary>
		public c_int Frame { get; internal set; }

		/// <summary>
		/// Current replay speed
		/// </summary>
		public c_int Speed { get; internal set; }

		/// <summary>
		/// Current bpm
		/// </summary>
		public c_int Bpm { get; internal set; }

		/// <summary>
		/// Current module time in ms
		/// </summary>
		public c_int Time { get; internal set; }

		/// <summary>
		/// Estimated replay time in ms
		/// </summary>
		public c_int Total_Time { get; internal set; }

		/// <summary>
		/// Frame replay time in us
		/// </summary>
		public c_int Frame_Time { get; internal set; }

		/// <summary>
		/// Pointer to sound buffer
		/// </summary>
		public ref CPointer<int8> Buffer => ref _Buffer;
		private CPointer<int8> _Buffer;

		/// <summary>
		/// Pointer to sound buffer for rear speakers
		/// </summary>
		public ref CPointer<int8> BufferRear => ref _BufferRear;
		private CPointer<int8> _BufferRear;

		/// <summary>
		/// Used buffer size
		/// </summary>
		public c_int Buffer_Size { get; internal set; }

		/// <summary>
		/// Total buffer size
		/// </summary>
		public c_int Total_Size { get; internal set; }

		/// <summary>
		/// Current master volume
		/// </summary>
		public c_int Volume { get; internal set; }

		/// <summary>
		/// Loop counter
		/// </summary>
		public c_int Loop_Count { get; internal set; }

		/// <summary>
		/// Number of virtual channels
		/// </summary>
		public c_int Virt_Channels { get; internal set; }

		/// <summary>
		/// Used virtual channels
		/// </summary>
		public c_int Virt_Used { get; internal set; }

		/// <summary>
		/// Current sequence
		/// </summary>
		public c_int Sequence { get; internal set; }

		/// <summary>
		/// Current channel information
		/// </summary>
		public Xmp_Channel_Info[] Channel_Info { get; } = ArrayHelper.InitializeArray<Xmp_Channel_Info>(Constants.Xmp_Max_Channels);

		/// <summary>
		/// Current playing tracks
		/// </summary>
		public c_int[] Playing_Tracks { get; } = new c_int[Constants.Xmp_Max_Channels];

		/// <summary>
		/// Status of the Amiga filter
		/// </summary>
		public bool Filter { get; internal set; }
	}
}
