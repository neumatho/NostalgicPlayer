/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Loader;

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
		public ref string Name => ref _Name;
		private string _Name = string.Empty;

		/// <summary>
		/// Module format
		/// </summary>
		public ref string Type => ref _Type;
		private string _Type = string.Empty;

		/// <summary>
		/// Author
		/// </summary>
		public string Author { get; internal set; } = string.Empty;

		/// <summary>
		/// Number of patterns
		/// </summary>
		public ref c_int Pat => ref _Pat;
		private c_int _Pat;

		/// <summary>
		/// Number of tracks
		/// </summary>
		public c_int Trk { get; internal set; }

		/// <summary>
		/// Tracks per pattern
		/// </summary>
		public ref c_int Chn => ref _Chn;
		private c_int _Chn;

		/// <summary>
		/// Number of instruments
		/// </summary>
		public ref c_int Ins => ref _Ins;
		private c_int _Ins;

		/// <summary>
		/// Number of samples
		/// </summary>
		public ref c_int Smp => ref _Smp;
		private c_int _Smp;

		/// <summary>
		/// Initial speed
		/// </summary>
		public ref c_int Spd => ref _Spd;
		private c_int _Spd;

		/// <summary>
		/// Initial BPM
		/// </summary>
		public ref c_int Bpm => ref _Bpm;
		private c_int _Bpm;

		/// <summary>
		/// Module length in patterns
		/// </summary>
		public ref c_int Len => ref _Len;
		private c_int _Len;

		/// <summary>
		/// Restart position
		/// </summary>
		public c_int Rst { get; internal set; }

		/// <summary>
		/// Global volume
		/// </summary>
		public c_int Gvl { get; internal set; }

		/// <summary>
		/// Patterns
		/// </summary>
		public Xmp_Pattern[] Xxp { get; internal set; }

		/// <summary>
		/// Tracks
		/// </summary>
		public Xmp_Track[] Xxt { get; internal set; }

		/// <summary>
		/// Instruments
		/// </summary>
		public Xmp_Instrument[] Xxi { get; internal set; }

		/// <summary>
		/// Samples
		/// </summary>
		public ref Xmp_Sample[] Xxs => ref _Xxs;
		private Xmp_Sample[] _Xxs;

		/// <summary>
		/// Channel info
		/// </summary>
		public Xmp_Channel[] Xxc { get; } = ArrayHelper.InitializeArray<Xmp_Channel>(Constants.Xmp_Max_Channels);

		/// <summary>
		/// Orders
		/// </summary>
		public byte[] Xxo { get; } = new byte[Constants.Xmp_Max_Mod_Length];
	}
}
