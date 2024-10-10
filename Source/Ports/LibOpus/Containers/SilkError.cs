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
	public enum SilkError
	{
		/// <summary>
		/// No error
		/// </summary>
		No_Error = 0,

		/// <summary>
		/// 
		/// </summary>
		Error = -1,

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
