/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Player.AmosMusicBank.Containers
{
	/// <summary>
	/// Holds playing information for a single channel
	/// </summary>
	internal class VoiceInfo : IDeepCloneable<VoiceInfo>
	{
		public delegate void EffectFunc(VoiceInfo voiceInfo, IChannel channel);

		public int ChannelNumber;

		public ushort[] VoiAdr;		// Track data
		public int VoiAdrIndex;
		public int VoiDeb;
		public Sample VoiInst;
		public short VoiInstNumber;
		public int VoiPatDIndex;
		public PositionList VoiPat;
		public int VoiPatIndex;
		public ushort VoiCpt;
		public ushort VoiRep;
		public ushort VoiNote;
		public ushort VoiDVol;
		public ushort VoiVol;
		public EffectFunc VoiEffect;
		public short VoiValue;
		public ushort VoiPToTo;
		public bool VoiPTone;
		public sbyte VoiVib;

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
