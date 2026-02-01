/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvUtil.Containers
{
	/// <summary>
	/// 
	/// </summary>
	[Flags]
	public enum AvRefStructPoolFlag
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		/// <summary>
		/// If this flag is not set, every object in the pool will be zeroed before
		/// the init callback is called or before it is turned over to the user
		/// for the first time if no init callback has been provided
		/// </summary>
		No_Zeroing = AvRefStructFlag.No_Zeroing,

		/// <summary>
		/// If this flag is set and both init_cb and reset_cb callbacks are provided,
		/// then reset_cb will be called if init_cb fails.
		/// The object passed to reset_cb will be in the state left by init_cb
		/// </summary>
		Reset_On_Init_Error = 1 << 16,

		/// <summary>
		/// If this flag is set and both init_cb and free_entry_cb callbacks are
		/// provided, then free_cb will be called if init_cb fails.
		/// 
		/// It will be called after reset_cb in case reset_cb and the
		/// AV_REFSTRUCT_POOL_FLAG_RESET_ON_INIT_ERROR flag are also set.
		/// 
		/// The object passed to free_cb will be in the state left by
		/// the callbacks applied earlier (init_cb potentially followed by reset_cb)
		/// </summary>
		Free_On_Init_Error = 1 << 17,

		/// <summary>
		/// If this flag is set, the entries will be zeroed before
		/// being returned to the user (after the init or reset callbacks
		/// have been called (if provided)). Furthermore, to avoid zeroing twice
		/// it also makes the pool behave as if the AV_REFSTRUCT_POOL_FLAG_NO_ZEROING
		/// flag had been provided
		/// </summary>
		Zero_Every_Time = 1 << 18
	}
}
