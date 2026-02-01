/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class AvRefStructPool : RefCount, IRefCountData
	{
		/// <summary>
		/// 
		/// </summary>
		internal Type Type;

		/// <summary>
		/// 
		/// </summary>
		internal new AvRefStructOpaque Opaque;

		/// <summary>
		/// 
		/// </summary>
		internal UtilFunc.Init_Cb_Delegate Init_Cb;

		/// <summary>
		/// 
		/// </summary>
		internal UtilFunc.Reset_Cb_Delegate Reset_Cb;

		/// <summary>
		/// 
		/// </summary>
		internal UtilFunc.Free_Entry_Delegate Free_Entry_Cb;

		/// <summary>
		/// 
		/// </summary>
		internal new UtilFunc.Free_Cb_Delegate Free_Cb;

		/// <summary>
		/// 
		/// </summary>
		internal c_int Uninited;

		/// <summary>
		/// 
		/// </summary>
		internal AvRefStructFlag Entry_Flags;

		/// <summary>
		/// 
		/// </summary>
		internal AvRefStructPoolFlag Pool_Flags;

		/// <summary>
		/// The number of outstanding entries not in available_entries
		/// </summary>
		internal c_uint RefCount;

		/// <summary>
		/// This is a linked list of available entries;
		/// the RefCount's opaque pointer is used as next pointer
		/// for available entries.
		/// While the entries are in use, the opaque is a pointer
		/// to the corresponding AVRefStructPool
		/// </summary>
		internal CPointer<RefCount> Available_Entries;

		/// <summary>
		/// 
		/// </summary>
		internal AvMutex Mutex;

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			Type = null;
			Opaque = null;
			Init_Cb = null;
			Reset_Cb = null;
			Free_Entry_Cb = null;
			Free_Cb = null;
			Uninited = 0;
			Entry_Flags = AvRefStructFlag.None;
			Pool_Flags = AvRefStructPoolFlag.None;
			RefCount = 0;
			Available_Entries.SetToNull();
			Mutex = null;
		}
	}
}
