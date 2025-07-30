/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp
{
	/// <summary>
	/// 
	/// </summary>
	public class Xmp_Event : IDeepCloneable<Xmp_Event>
	{
		/// <summary>
		/// Note number (0 means no note)
		/// </summary>
		public byte Note { get; internal set; }

		/// <summary>
		/// Patch number
		/// </summary>
		public byte Ins { get; internal set; }

		/// <summary>
		/// Volume (0 to basevol)
		/// </summary>
		public byte Vol { get; internal set; }

		/// <summary>
		/// Effect type
		/// </summary>
		public ref byte FxT => ref _FxT;
		private byte _FxT;

		/// <summary>
		/// Effect parameter
		/// </summary>
		public ref byte FxP => ref _FxP;
		private byte _FxP;

		/// <summary>
		/// Secondary effect type
		/// </summary>
		public ref byte F2T => ref _F2T;
		private byte _F2T;

		/// <summary>
		/// Secondary effect parameter
		/// </summary>
		public ref byte F2P => ref _F2P;
		private byte _F2P;

		/// <summary>
		/// Internal (reserved) flags
		/// </summary>
		internal byte _Flag { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public Xmp_Event MakeDeepClone()
		{
			return (Xmp_Event)MemberwiseClone();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			Note = 0;
			Ins = 0;
			Vol = 0;
			FxT = 0;
			FxP = 0;
			F2T = 0;
			F2P = 0;
			_Flag = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void CopyFrom(Xmp_Event other)
		{
			Note = other.Note;
			Ins = other.Ins;
			Vol = other.Vol;
			FxT = other.FxT;
			FxP = other.FxP;
			F2T = other.F2T;
			F2P = other.F2P;
			_Flag = other._Flag;
		}
	}
}
