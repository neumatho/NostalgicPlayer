/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Ports.LibXmp.FormatExtras;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp
{
	/// <summary>
	/// 
	/// </summary>
	public class Xmp_Instrument
	{
		/// <summary>
		/// Instrument name
		/// </summary>
		public ref string Name => ref _Name;
		private string _Name = string.Empty;

		/// <summary>
		/// Instrument volume
		/// </summary>
		public c_int Vol { get; internal set; }

		/// <summary>
		/// Number of samples
		/// </summary>
		public c_int Nsm { get; internal set; }

		/// <summary>
		/// Release (fadeout)
		/// </summary>
		public c_int Rls { get; internal set; }

		/// <summary>
		/// Amplitude envelope info
		/// </summary>
		public Xmp_Envelope Aei { get; } = new Xmp_Envelope();

		/// <summary>
		/// Pan envelope info
		/// </summary>
		public Xmp_Envelope Pei { get; } = new Xmp_Envelope();

		/// <summary>
		/// Frequency envelope info
		/// </summary>
		public Xmp_Envelope Fei { get; } = new Xmp_Envelope();

		/// <summary>
		/// 
		/// </summary>
		public struct _Map
		{
			/// <summary>
			/// Instrument number for each key
			/// </summary>
			public byte Ins { get; internal set; }

			/// <summary>
			/// Instrument transpose for each key
			/// </summary>
			public sbyte Xpo { get; internal set; }
		}

		/// <summary>
		/// 
		/// </summary>
		public _Map[] Map { get; } = new _Map[Constants.Xmp_Max_Keys];

		/// <summary>
		/// 
		/// </summary>
		public Xmp_SubInstrument[] Sub { get; internal set; }

		/// <summary>
		/// Extra fields
		/// </summary>
		public IInstrumentExtra Extra { get; internal set; }
	}
}
