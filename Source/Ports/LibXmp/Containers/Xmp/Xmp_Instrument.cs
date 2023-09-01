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
	public class Xmp_Instrument
	{
		/// <summary>
		/// Instrument name
		/// </summary>
		public string Name = string.Empty;

		/// <summary>
		/// Instrument volume
		/// </summary>
		public c_int Vol;

		/// <summary>
		/// Number of samples
		/// </summary>
		public c_int Nsm;

		/// <summary>
		/// Release (fadeout)
		/// </summary>
		public c_int Rls;

		/// <summary>
		/// Amplitude envelope info
		/// </summary>
		public Xmp_Envelope Aei = new Xmp_Envelope();

		/// <summary>
		/// Pan envelope info
		/// </summary>
		public Xmp_Envelope Pei = new Xmp_Envelope();

		/// <summary>
		/// Frequency envelope info
		/// </summary>
		public Xmp_Envelope Fei = new Xmp_Envelope();

		/// <summary>
		/// 
		/// </summary>
		public struct _Map
		{
			/// <summary>
			/// Instrument number for each key
			/// </summary>
			public byte Ins;

			/// <summary>
			/// Instrument transpose for each key
			/// </summary>
			public sbyte Xpo;
		}

		/// <summary>
		/// 
		/// </summary>
		public _Map[] Map = new _Map[Constants.Xmp_Max_Keys];

		/// <summary>
		/// 
		/// </summary>
		public Xmp_SubInstrument[] Sub;

		/// <summary>
		/// Extra fields
		/// </summary>
		public object Extra;
	}
}
