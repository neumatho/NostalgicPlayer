/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Containers
{
	/// <summary>
	/// Holds different information about a sample
	/// </summary>
	public class SampleInformation
	{
		/// <summary>
		/// 
		/// </summary>
		[Flags]
		public enum SampleFlags
		{
			/// <summary></summary>
			None = 0,
			/// <summary></summary>
			Loop = 1,
			/// <summary></summary>
			PingPong = 2,
			/// <summary></summary>
			_16Bit = 4,
			/// <summary></summary>
			Stereo = 8,
		}

		/// <summary>
		/// 
		/// </summary>
		public string Name;

		/// <summary>
		/// 
		/// </summary>
		public uint Length;

		/// <summary>
		/// 
		/// </summary>
		public uint LoopStart;

		/// <summary>
		/// 
		/// </summary>
		public uint LoopLength;

		/// <summary>
		/// 
		/// </summary>
		public Array SampleData;

		/// <summary>
		/// 
		/// </summary>
		public uint SampleOffset;

		/// <summary>
		/// 
		/// </summary>
		public ushort Volume;

		/// <summary>
		/// 
		/// </summary>
		public short Panning;

		/// <summary>
		/// 
		/// </summary>
		public SampleFlags Flags;

		/// <summary>
		/// 
		/// </summary>
		public uint[] NoteFrequencies;
	}
}
