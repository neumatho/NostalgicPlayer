/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpus.Containers
{
	/// <summary>
	/// Opus error codes
	/// </summary>
	public enum OpusError
	{
		/// <summary>
		/// No error
		/// </summary>
		Ok = 0,

		/// <summary>
		/// One or more invalid/out of range arguments
		/// </summary>
		Bad_Arg = -1,

		/// <summary>
		/// Not enough bytes allocated in the buffer
		/// </summary>
		Buffer_Too_Small = -2,

		/// <summary>
		/// An internal error was detected
		/// </summary>
		Internal_Error = -3,

		/// <summary>
		/// The compressed data passed is corrupted
		/// </summary>
		Invalid_Packet = -4,

		/// <summary>
		/// Invalid/unsupported request number
		/// </summary>
		Unimplemented = -5,

		/// <summary>
		/// An encoder or decoder structure is invalid or already freed
		/// </summary>
		Invalid_State = -6,

		/// <summary>
		/// Memory allocation has failed
		/// </summary>
		Alloc_Fail = -7,

		/// <summary>
		/// Output sampling frequency lower than internal decoded sampling frequency
		/// </summary>
		Invalid_Sampling_Frequency = -200,

		/// <summary>
		/// Payload has bit errors
		/// </summary>
		Invalid_Frame_Size = -203
	}
}
