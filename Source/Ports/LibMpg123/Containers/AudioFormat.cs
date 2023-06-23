/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibMpg123.Containers
{
	/// <summary>
	/// 
	/// </summary>
	internal class AudioFormat
	{
		public Mpg123_Enc_Enum Encoding;// Final encoding, after post-processing
		public c_int EncSize;			// Size of one sample in bytes, plain int should be fine here...
		public Mpg123_Enc_Enum Dec_Enc;	// Encoding of decoder synth
		public c_int Dec_EncSize;		// Size of one decoder sample
		public c_int Channels;
		public c_long Rate;
	}
}
