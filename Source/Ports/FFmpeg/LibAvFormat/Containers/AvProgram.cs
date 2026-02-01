/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvFormat.Containers
{
	/// <summary>
	/// New fields can be added to the end with minor version bumps.
	/// Removal, reordering and changes to existing fields require a major
	/// version bump.
	/// sizeof(AVProgram) must not be used outside libav*
	/// </summary>
	public class AvProgram
	{
		/// <summary>
		/// 
		/// </summary>
		public c_int Id;

		/// <summary>
		/// 
		/// </summary>
		public c_int Flags;

		/// <summary>
		/// Selects which program to discard and which to feed to the caller
		/// </summary>
		public AvDiscard Discard;

		/// <summary>
		/// 
		/// </summary>
		public CPointer<c_uint> Stream_Index;

		/// <summary>
		/// 
		/// </summary>
		public c_uint Nb_Stream_Indexes;

		/// <summary>
		/// 
		/// </summary>
		public AvDictionary Metadata;

		/// <summary>
		/// 
		/// </summary>
		public c_int Program_Num;

		/// <summary>
		/// 
		/// </summary>
		public c_int Pmt_Pid;

		/// <summary>
		/// 
		/// </summary>
		public c_int Pcr_Pid;

		/// <summary>
		/// 
		/// </summary>
		public c_int Pmt_Version;

		//****************************************************************
		// All fields below this line are not part of the public API. They
		// may not be used outside of libavformat and can be changed and
		// removed at will.
		// New public fields should be added right above
		//****************************************************************

		/// <summary>
		/// 
		/// </summary>
		public int64_t Start_Time;

		/// <summary>
		/// 
		/// </summary>
		public int64_t End_Time;

		/// <summary>
		/// Reference dts for wrap detection
		/// </summary>
		public int64_t Pts_Wrap_Reference;

		/// <summary>
		/// Behavior on wrap detection
		/// </summary>
		public AvPtsWrap Pts_Wrap_Behaviour;
	}
}
