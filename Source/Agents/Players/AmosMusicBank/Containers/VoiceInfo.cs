/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.AmosMusicBank.Containers
{
	/// <summary>
	/// Holds playing information for a single channel
	/// </summary>
	internal class VoiceInfo : IDeepCloneable<VoiceInfo>
	{
		public delegate void EffectFunc(VoiceInfo voiceInfo, IChannel channel);

		public int ChannelNumber { get; set; }

		public ushort[] VoiAdr { get; set; }	// Track data
		public int VoiAdrIndex { get; set; }
		public int VoiDeb { get; set; }
		public Sample VoiInst { get; set; }
		public short VoiInstNumber { get; set; }
		public int VoiPatDIndex { get; set; }
		public PositionList VoiPat { get; set; }
		public int VoiPatIndex { get; set; }
		public ushort VoiCpt { get; set; }
		public ushort VoiRep { get; set; }
		public ushort VoiNote { get; set; }
		public ushort VoiDVol { get; set; }
		public ushort VoiVol { get; set; }
		public EffectFunc VoiEffect { get; set; }
		public short VoiValue { get; set; }
		public ushort VoiPToTo { get; set; }
		public bool VoiPTone { get; set; }
		public sbyte VoiVib { get; set; }

		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public VoiceInfo MakeDeepClone()
		{
			return (VoiceInfo)MemberwiseClone();
		}
	}
}
