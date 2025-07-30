/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp
{
	/// <summary>
	/// 
	/// </summary>
	public class Xmp_Module_Info
	{
		/// <summary>
		/// MD5 message digest
		/// </summary>
		public byte[] Md5 { get; } = new byte[16];

		/// <summary>
		/// Volume scale
		/// </summary>
		public c_int Vol_Base { get; internal set; }

		/// <summary>
		/// Pointer to module data
		/// </summary>
		public Xmp_Module Mod { get; internal set; }

		/// <summary>
		/// Comment text, if any
		/// </summary>
		public string Comment { get; internal set; }

		/// <summary>
		/// Number of valid sequences
		/// </summary>
		public c_int Num_Sequences { get; internal set; }

		/// <summary>
		/// Pointer to sequence data
		/// </summary>
		public Xmp_Sequence[] Seq_Data { get; internal set; }

		/// <summary>
		/// C-5 speeds for each sample
		/// </summary>
		public c_double[] C5Speeds { get; internal set; }

		/// <summary>
		/// Different flags
		/// </summary>
		public Xmp_Module_Flags Flags { get; internal set; }
	}
}
