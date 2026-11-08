/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using OplRegister = System.UInt16;
using OplValue = System.Byte;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Translate data coming from OpenMPT's mixer into OPL commands to be sent to the Opal emulator
	/// </summary>
	internal class Opl
	{
		#region OplRegisters

		#region Operators (combine with result of OperatorToRegister)
		/// <summary>
		/// AM / VIB / EG / KSR / Multiple (0x20 to 0x35)
		/// </summary>
		private const uint8 Am_Vib = 0x20;

		/// <summary>
		/// KSL / Total level (0x40 to 0x55)
		/// </summary>
		private const uint8 Ksl_Level = 0x40;

		/// <summary>
		/// Attack rate / Decay rate (0x60 to 0x75)
		/// </summary>
		private const uint8 Attack_Decay = 0x60;

		/// <summary>
		/// Sustain level / Release rate (0x80 to 0x95)
		/// </summary>
		private const uint8 Sustain_Release = 0x80;

		/// <summary>
		/// Wave select (0xE0 to 0xF5)
		/// </summary>
		private const uint8 Wave_Select = 0xe0;
		
		#endregion

		#region Channels (combine with result of ChannelToRegister)
		/// <summary>
		/// F-number low bits (0xA0 to 0xA8)
		/// </summary>
		private const uint8 FNum_Low = 0xa0;

		/// <summary>
		/// F-number high bits / Key on / Block (octave) (0xB0 to 0xB8)
		/// </summary>
		private const uint8 KeyOn_Block = 0xb0;

		/// <summary>
		/// Feedback / Connection (0xC0 to 0xC8)
		/// </summary>
		private const uint8 Feedback_Connection = 0xc0;

		/// <summary>
		/// Tremolo Depth / Vibrato Depth / Percussion Mode / BD/SD/TT/CY/HH Key-On
		/// </summary>
		private const uint8 Tremolo_Vibrato_Depth = 0xbd;
		#endregion

		#endregion

		#region OplValues

		#region AM VIB
		private const uint8 Tremolo_On = 0x80;
		private const uint8 Vibrato_On = 0x40;
		private const uint8 Sustain_On = 0x20;

		/// <summary>
		/// Key scaling rate
		/// </summary>
		private const uint8 Ksr = 0x10;

		/// <summary>
		/// Frequency multiplier
		/// </summary>
		private const uint8 Multiple_Mask = 0x0f;
		#endregion

		#region KSL Level
		/// <summary>
		/// Envelope scaling bits
		/// </summary>
		private const uint8 Ksl_Mask = 0xc0;

		/// <summary>
		/// Strength (volume) of OP
		/// </summary>
		private const uint8 Total_Level_Mask = 0x3f;
		#endregion

		#region Attack Decay
		private const uint8 Attack_Mask = 0xf0;
		private const uint8 Decay_Mask = 0x0f;
		#endregion

		#region Sustain Release
		private const uint8 Sustain_Mask = 0xf0;
		private const uint8 Release_Mask = 0x0f;
		#endregion

		#region KeyOn Block
		private const uint8 KeyOn_Bit = 0x20;
		#endregion

		#region Feedback Connection
		/// <summary>
		/// Valid just for first OP of a voice
		/// </summary>
		private const uint8 Feedback_Mask = 0x0e;
		private const uint8 Connection_Bit = 0x01;
		private const uint8 Voice_To_Left = 0x10;
		private const uint8 Voice_To_Right = 0x20;
		private const uint8 Stereo_Bits = Voice_To_Left | Voice_To_Right;
		#endregion

		#endregion

		/// <summary>
		/// 9 for OPL2 or 18 for OPL3
		/// </summary>
		private const size_t Opl_Channels = 18;

		/// <summary>
		/// Indicates that the channel has been cut and used as a hint to re-use the channel for the same tracker channel if possible
		/// </summary>
		private const uint8 Opl_Channel_Cut = 0x80;
		private const uint8 Opl_Channel_Mask = 0x7f;
		private const uint8 Opl_Channel_Invalid = 0xff;
		private const int Opl_BaseRate = 49716;

		private Opal.Opal m_Opl;

		private readonly array<uint8> m_KeyOnBlock = new array<uint8>(Opl_Channels);
		private readonly array<ChannelIndex> m_OplToChan = new array<ChannelIndex>(Opl_Channels);
		private readonly array<uint8> m_ChanToOpl = new array<uint8>(Snd_Def.Max_Channels);
		private readonly array<OplPatch> m_Patches = new array<OplPatch>(Opl_Channels);

		private bool m_IsActive = false;

		private static readonly uint8[] oplChannelToOperator = [ 0, 1, 2, 8, 9, 10, 16, 17, 18 ];

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Opl(uint32 sampleRate)
		{
			Initialize(sampleRate);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsActive(ChannelIndex c)
		{
			return GetVoice(c) != Opl_Channel_Invalid;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Initialize(uint32 sampleRate)
		{
			if (m_Opl == null)
				m_Opl = new Opal.Opal((c_int)sampleRate);
			else
				m_Opl.SetSampleRate((c_int)sampleRate);

			Reset();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Mix(CPointer<int32> target, size_t count, uint32 volumeFactorQ16)
		{
			if (!m_IsActive)
				return;

			// This factor causes a sample voice to be more or less as loud as an OPL voice
			int32 factor = (int32)Util.MulDiv_Unsigned(volumeFactorQ16, 6169, 1 << 16);

			while (count-- != 0)
			{
				m_Opl.Sample(out int16 l, out int16 r);

				target[0] += l * factor;
				target[1] += r * factor;
				target += 2;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void MoveChannel(ChannelIndex from, ChannelIndex to)
		{
			uint8 oplCh = GetVoice(from);

			if (oplCh == Opl_Channel_Invalid)
				return;

			m_OplToChan[oplCh] = to;
			m_ChanToOpl[from] = Opl_Channel_Invalid;
			m_ChanToOpl[to] = oplCh;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void NoteOff(ChannelIndex c)
		{
			uint8 oplCh = GetVoice(c);

			if ((oplCh == Opl_Channel_Invalid) || (m_Opl == null))
				return;

			if ((m_KeyOnBlock[oplCh] & KeyOn_Bit) == 0)
				return;

			m_KeyOnBlock[oplCh] &= unchecked((uint8)(~KeyOn_Bit));
			Port(c, (OplRegister)(KeyOn_Block | ChannelToRegister(oplCh)), m_KeyOnBlock[oplCh]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void NoteCut(ChannelIndex c, bool unassign = true)
		{
			uint8 oplCh = GetVoice(c);

			if (oplCh == Opl_Channel_Invalid)
				return;

			NoteOff(c);
			Volume(c, 0, false);	// Note that a volume of 0 is not complete silence; the release portion of the sound will still be heard at -48dB

			if (unassign)
			{
				m_OplToChan[oplCh] = Snd_Def.ChannelIndex_Invalid;
				m_ChanToOpl[c] |= Opl_Channel_Cut;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Frequency(ChannelIndex c, uint32 milliHertz, bool keyOff, bool beatingOscillators)
		{
			uint8 oplCh = GetVoice(c);

			if ((oplCh == Opl_Channel_Invalid) || (m_Opl == null))
				return;

			uint16 fNum = 1023;
			uint8 block = 7;

			if (milliHertz <= 6208431)
			{
				if (milliHertz > 3104215)
					block = 7;
				else if (milliHertz > 1552107)
					block = 6;
				else if (milliHertz > 776053)
					block = 5;
				else if (milliHertz > 388026)
					block = 4;
				else if (milliHertz > 194013)
					block = 3;
				else if (milliHertz > 97006)
					block = 2;
				else if (milliHertz > 48503)
					block = 1;
				else
					block = 0;

				fNum = (uint16)(Util.MulDivR_Unsigned(milliHertz, (uint32)(1 << (20 - block)), Opl_BaseRate * 1000));
			}

			// Evil CDFM hack! Composer 670 slightly detunes each note based on the OPL channel number modulo 4.
			// We allocate our OPL channels dynamically, which would result in slightly different beating characteristics,
			// but we can just take the pattern channel number instead, as the pattern channel layout is always identical
			if (beatingOscillators)
				fNum = Math.Min((uint16)(fNum + (c & 3)), (uint16)1023);

			fNum |= (uint16)(block << 10);

			OplRegister channel = ChannelToRegister(oplCh);
			m_KeyOnBlock[oplCh] = (uint8)((keyOff ? 0 : KeyOn_Bit) | (fNum >> 8));		// Key on bit + Octave (block) + F-number high 2 bits
			Port(c, (OplRegister)(FNum_Low | channel), (OplValue)(fNum & 0xff));		// F-Number low 8 bits

			Port(c, (OplRegister)(KeyOn_Block | channel), m_KeyOnBlock[oplCh]);

			m_IsActive = true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Volume(ChannelIndex c, uint8 vol, bool applyToModulator)
		{
			uint8 oplCh = GetVoice(c);

			if ((oplCh == Opl_Channel_Invalid) || (m_Opl == null))
				return;

			OplPatch patch = m_Patches[oplCh];
			OplRegister modulator = OperatorToRegister(oplCh), carrier = (OplRegister)(modulator + 3);

			if (((patch[10] & Connection_Bit) != 0) || applyToModulator)
			{
				// Set volume of both operators in additive mode
				Port(c, (OplRegister)(Ksl_Level + modulator), CalcVolume(vol, patch[2]));
			}

			if (!applyToModulator)
				Port(c, (OplRegister)(Ksl_Level + carrier), CalcVolume(vol, patch[3]));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int8 Pan(ChannelIndex c, int32 pan)
		{
			uint8 oplCh = GetVoice(c);

			if ((oplCh == Opl_Channel_Invalid) || (m_Opl == null))
				return 0;

			OplPatch patch = m_Patches[oplCh];
			uint8 fbConn = (uint8)(patch[10] & ~Stereo_Bits);

			// OPL3 only knows hard left, center and right, so we need to translate our
			// continuous panning range into one of those three states.
			// 0...84 = left, 85...170 = center, 171...256 = right
			if (pan <= 170)
				fbConn |= Voice_To_Left;

			if (pan >= 85)
				fbConn |= Voice_To_Right;

			Port(c, (OplRegister)(Feedback_Connection | ChannelToRegister(oplCh)), fbConn);

			return (int8)(((fbConn & Voice_To_Left) != 0 ? -1 : 0) + ((fbConn & Voice_To_Right) != 0 ? 1 : 0));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Patch(ChannelIndex c, OplPatch patch)
		{
			uint8 oplCh = AllocateVoice(c);

			if ((oplCh == Opl_Channel_Invalid) || (m_Opl == null))
				return;

			patch.CopyTo(m_Patches[oplCh]);

			OplRegister modulator = OperatorToRegister(oplCh), carrier = (OplRegister)(modulator + 3);

			for (uint8 op = 0; op < 2; op++)
			{
				OplRegister opReg = op != 0 ? carrier : modulator;

				Port(c, (OplRegister)(Am_Vib | opReg), patch[0 + op]);
				Port(c, (OplRegister)(Ksl_Level | opReg), patch[2 + op]);
				Port(c, (OplRegister)(Attack_Decay | opReg), patch[4 + op]);
				Port(c, (OplRegister)(Sustain_Release | opReg), patch[6 + op]);
				Port(c, (OplRegister)(Wave_Select | opReg), patch[8 + op]);
			}

			Port(c, (OplRegister)(Feedback_Connection | ChannelToRegister(oplCh)), patch[10]);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Reset()
		{
			if (m_IsActive)
			{
				for (ChannelIndex chn = 0; chn < Snd_Def.Max_Channels; chn++)
					NoteCut(chn);

				m_IsActive = false;
			}

			m_KeyOnBlock.fill(0);
			m_OplToChan.fill(Snd_Def.ChannelIndex_Invalid);
			m_ChanToOpl.fill(Opl_Channel_Invalid);

			Port(Snd_Def.ChannelIndex_Invalid, 0x105, 1);	// Enable OPL3
			Port(Snd_Def.ChannelIndex_Invalid, 0x104, 0);	// No 4-op voices
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private OplRegister ChannelToRegister(uint8 oplCh)
		{
			if (oplCh < 9)
				return oplCh;
			else
				return (OplRegister)((oplCh - 9) | 0x100);
		}



		/********************************************************************/
		/// <summary>
		/// Translate a channel's first operator address into a register
		/// </summary>
		/********************************************************************/
		private OplRegister OperatorToRegister(uint8 oplCh)
		{
			if (oplCh < 9)
				return oplChannelToOperator[oplCh];
			else
				return (OplRegister)(oplChannelToOperator[oplCh - 9] | 0x100);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint8 GetVoice(ChannelIndex c)
		{
			if (((m_ChanToOpl[c] & Opl_Channel_Cut) != 0) || (m_ChanToOpl[c] == Opl_Channel_Invalid))
				return Opl_Channel_Invalid;

			return (uint8)(m_ChanToOpl[c] & Opl_Channel_Mask);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint8 AllocateVoice(ChannelIndex c)
		{
			// Can we re-use a previous channel?
			uint8 oplCh = m_ChanToOpl[c];

			if (oplCh != Opl_Channel_Invalid)
			{
				if ((m_ChanToOpl[c] & Opl_Channel_Cut) == 0)
					return oplCh;

				// Check re-use hint
				oplCh &= Opl_Channel_Mask;

				if ((m_OplToChan[oplCh] == Snd_Def.ChannelIndex_Invalid) || (m_OplToChan[oplCh] == c))
				{
					m_OplToChan[oplCh] = c;
					m_ChanToOpl[c] = oplCh;

					return oplCh;
				}
			}

			// Search for unused channel or channel with released note
			uint8 releasedChn = Opl_Channel_Invalid, releasedCutChn = Opl_Channel_Invalid;

			for (oplCh = 0; oplCh < Opl_Channels; oplCh++)
			{
				if (m_OplToChan[oplCh] == Snd_Def.ChannelIndex_Invalid)
				{
					m_OplToChan[oplCh] = c;
					m_ChanToOpl[c] = oplCh;

					return oplCh;
				}
				else if ((m_KeyOnBlock[oplCh] & KeyOn_Bit) == 0)
				{
					releasedChn = oplCh;

					if ((m_ChanToOpl[m_OplToChan[oplCh]] & Opl_Channel_Cut) != 0)
						releasedCutChn = oplCh;
				}
			}

			if (releasedChn != Opl_Channel_Invalid)
			{
				// Prefer channel that has been marked as cut over channel that has just been released
				if (releasedCutChn != Opl_Channel_Invalid)
					releasedChn = releasedCutChn;

				m_ChanToOpl[m_OplToChan[releasedChn]] = Opl_Channel_Invalid;
				m_OplToChan[releasedChn] = c;
				m_ChanToOpl[c] = releasedChn;
			}

			return GetVoice(c);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private uint8 CalcVolume(uint8 trackerVol, uint8 kslVolume)
		{
			if (trackerVol >= 63U)
				return kslVolume;

			if (trackerVol > 0)
				trackerVol++;

			return (uint8)((kslVolume & Ksl_Mask) | (uint8)(63U - (((63U - (kslVolume & Total_Level_Mask)) * trackerVol) / 64U)));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Port(ChannelIndex c, OplRegister reg, OplValue value)
		{
			m_Opl.Port(reg, value);
		}
		#endregion
	}
}
