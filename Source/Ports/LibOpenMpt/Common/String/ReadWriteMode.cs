/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.String
{
	/// <summary>
	/// 
	/// </summary>
	internal enum ReadWriteMode : uint8
	{
		/// <summary>
		/// Reading / Writing: Standard null-terminated string handling
		/// </summary>
		NullTerminated = 1,

		/// <summary>
		/// Reading: Source string is not guaranteed to be null-terminated (if it fills the whole char array).
		/// Writing: Destination string is not guaranteed to be null-terminated (if it fills the whole char array).
		/// </summary>
		MaybeNullTerminated = 2,

		/// <summary>
		/// Reading: String may contain null characters anywhere. They should be treated as spaces.
		/// Writing: A space-padded string is written.
		/// </summary>
		SpacePadded = 3,

		/// <summary>
		/// Reading: String may contain null characters anywhere. The last character is ignored (it is supposed to be 0).
		/// Writing: A space-padded string with a trailing null is written.
		/// </summary>
		SpacePaddedNull = 4
	}
}
