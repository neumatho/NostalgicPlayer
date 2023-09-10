/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
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
		public c_int Pos;

		/// <summary>
		/// Current pattern
		/// </summary>
		public c_int Pattern;

		/// <summary>
		/// Current row in pattern
		/// </summary>
		public c_int Row;

		/// <summary>
		/// Number of rows in current pattern
		/// </summary>
		public c_int Num_Rows;

		/// <summary>
		/// Current frame
		/// </summary>
		public c_int Frame;

		/// <summary>
		/// Current replay speed
		/// </summary>
		public c_int Speed;

		/// <summary>
		/// Current bpm
		/// </summary>
		public c_int Bpm;

		/// <summary>
		/// Current module time in ms
		/// </summary>
		public c_int Time;

		/// <summary>
		/// Estimated replay time in ms
		/// </summary>
		public c_int Total_Time;

		/// <summary>
		/// Frame replay time in us
		/// </summary>
		public c_int Frame_Time;

		/// <summary>
		/// Pointer to sound buffer
		/// </summary>
		public uint8[] Buffer;

		/// <summary>
		/// Used buffer size
		/// </summary>
		public c_int Buffer_Size;

		/// <summary>
		/// Total buffer size
		/// </summary>
		public c_int Total_Size;

		/// <summary>
		/// Current master volume
		/// </summary>
		public c_int Volume;

		/// <summary>
		/// Loop counter
		/// </summary>
		public c_int Loop_Count;

		/// <summary>
		/// Number of virtual channels
		/// </summary>
		public c_int Virt_Channels;

		/// <summary>
		/// Used virtual channels
		/// </summary>
		public c_int Virt_Used;

		/// <summary>
		/// Current sequence
		/// </summary>
		public c_int Sequence;

		/// <summary>
		/// Current channel information
		/// </summary>
		public Xmp_Channel_Info[] Channel_Info = ArrayHelper.InitializeArray<Xmp_Channel_Info>(Constants.Xmp_Max_Channels);

		/// <summary>
		/// Status of the Amiga filter
		/// </summary>
		public bool Filter;
	}
}
