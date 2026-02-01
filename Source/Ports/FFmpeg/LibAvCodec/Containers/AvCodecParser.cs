/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.FFmpeg.LibAvCodec.Containers
{
	/// <summary>
	/// 
	/// </summary>
	public class AvCodecParser
	{
		/// <summary>
		/// 
		/// </summary>
		public readonly AvCodecId[] Codec_Ids = new AvCodecId[7];

		/// <summary>
		/// 
		/// </summary>
//		public c_int Priv_Data_Size;
		public CodecFunc.Private_Data_Alloc_Delegate Priv_Data_Alloc;

		/// <summary>
		/// 
		/// </summary>
		public CodecFunc.Parser_Init_Delegate Parser_Init;

		/// <summary>
		/// 
		/// </summary>
		public CodecFunc.Parser_Parse_Delegate Parser_Parse;

		/// <summary>
		/// 
		/// </summary>
		public CodecFunc.Parser_Close_Delegate Parser_Close;
	}
}
