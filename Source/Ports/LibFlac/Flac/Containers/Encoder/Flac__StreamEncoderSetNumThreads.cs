/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibFlac.Flac.Containers.Encoder
{
	/// <summary>
	/// 
	/// </summary>
	public enum Flac__StreamEncoderSetNumThreads
	{
		/// <summary></summary>
		Ok = 0,

		/// <summary></summary>
		Not_Compiled_With_Multithreading_Enabled,

		/// <summary></summary>
		Already_Initialized,

		/// <summary></summary>
		Too_Many_Threads
	}
}
