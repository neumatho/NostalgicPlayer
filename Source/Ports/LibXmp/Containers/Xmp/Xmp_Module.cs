/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp
{
	/// <summary>
	/// 
	/// </summary>
	public class Xmp_Module
	{
		/// <summary>
		/// Module title
		/// </summary>
		public string Name = string.Empty;

		/// <summary>
		/// Module format
		/// </summary>
		public string Type = string.Empty;

		/// <summary>
		/// Author
		/// </summary>
		public string Author = string.Empty;

		/// <summary>
		/// Number of patterns
		/// </summary>
		public c_int Pat;

		/// <summary>
		/// Number of tracks
		/// </summary>
		public c_int Trk;

		/// <summary>
		/// Tracks per pattern
		/// </summary>
		public c_int Chn;

		/// <summary>
		/// Number of instruments
		/// </summary>
		public c_int Ins;

		/// <summary>
		/// Number of samples
		/// </summary>
		public c_int Smp;

		/// <summary>
		/// Initial speed
		/// </summary>
		public c_int Spd;

		/// <summary>
		/// Initial BPM
		/// </summary>
		public c_int Bpm;

		/// <summary>
		/// Module length in patterns
		/// </summary>
		public c_int Len;

		/// <summary>
		/// Restart position
		/// </summary>
		public c_int Rst;

		/// <summary>
		/// Global volume
		/// </summary>
		public c_int Gvl;

		/// <summary>
		/// Patterns
		/// </summary>
		public Xmp_Pattern[] Xxp;

		/// <summary>
		/// Tracks
		/// </summary>
		public Xmp_Track[] Xxt;

		/// <summary>
		/// Instruments
		/// </summary>
		public Xmp_Instrument[] Xxi;

		/// <summary>
		/// Samples
		/// </summary>
		public Xmp_Sample[] Xxs;

		/// <summary>
		/// Channel info
		/// </summary>
		public readonly Xmp_Channel[] Xxc = ArrayHelper.InitializeArray<Xmp_Channel>(Constants.Xmp_Max_Channels);

		/// <summary>
		/// Orders
		/// </summary>
		public readonly byte[] Xxo = new byte[Constants.Xmp_Max_Mod_Length];
	}
}
