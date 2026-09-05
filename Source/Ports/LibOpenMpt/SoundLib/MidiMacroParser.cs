/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// Class for parsing IT MIDI macro strings and splitting them into individual raw MIDI messages
	/// </summary>
	internal class MidiMacroParser
	{
		private readonly MptSpan<uint8> m_Data;
		private uint32 m_SendPos = 0;
		private uint32 m_RunningStatusPos = uint32.MaxValue;
		private uint8 m_RunningStatus = 0;
		private uint8 m_RunningStatusOldData = 0;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public MidiMacroParser(MptSpan<uint8> data)
		{
			m_Data = data;
		}



		/********************************************************************/
		/// <summary>
		/// Constructor
		///
		/// Parse the given MIDI macro into the out span. out needs to be at
		/// least one byte longer than the input string to support the
		/// longest possible macro translation
		/// </summary>
		/********************************************************************/
		public MidiMacroParser(CSoundFile sndFile, PlayState playState, ChannelIndex nChn, bool isSmooth, MptSpan<uint8> macro, MptSpan<uint8> @out, uint8 param, PlugIndex plugin)//XX 95
		{
			ModChannel chn = (playState != null) && (nChn < playState.Chn.size()) ? playState.Chn[nChn] : null;
			ModInstrument pIns = chn != null ? chn.pModInstrument : null;

			uint8 lastZxxParam = chn != null ? chn.LastZxxParam : (uint8)0xff;	// Always interpolate based on original value in case z appears multiple times in macro string
			uint8 updateZxxParam = 0xff;										// Avoid updating lastZxxParam immediately if macro contains both internal and external MIDI message

			bool firstNibble = true;
			size_t outPos = 0;		// Output buffer position, which also equals the number of complete bytes

			for (size_t pos = 0; (pos < macro.Size()) && (outPos < @out.Size()); pos++)
			{
				bool isNibble = false;	// Did we parse a nibble or a byte value?
				uint8 data = 0;			// Data that has just been parsed

				// Parse next macro byte... See Impulse Tracker's MIDI.TXT for detailed information on each possible character
				if ((macro[pos] >= '0') && (macro[pos] <= '9'))
				{
					isNibble = true;
					data = (uint8)(macro[pos] - '0');
				}
				else if ((macro[pos] >= 'A') && (macro[pos] <= 'F'))
				{
					isNibble = true;
					data = (uint8)(macro[pos] - 'A' + 0x0a);
				}
				else if (macro[pos] == 'c')
				{
					// MIDI channel
					isNibble = true;
					data = 0xff;

					if (data == 0xff)
					{
						// Fallback if no plugin was found
						if ((pIns != null) && (chn != null))
							data = pIns.GetMidiChannel(chn, nChn);
						else
							data = 0;
					}
				}
				else if (macro[pos] == 'n')
				{
					// Last triggered note
					if ((chn != null) && ModCommand.IsNote(chn.nLastNote))
						data = (uint8)(chn.nLastNote - ModCommand.Note_Min);
				}
				else if (macro[pos] == 'v')
				{
					// Velocity
					// This is "almost" how IT does it - apparently, IT seems to lag one row behind on global volume or channel volume changes
					if ((chn != null) && (playState != null))
					{
						c_int swing = sndFile.m_PlayBehaviour[PlayBehaviour.ItSwingBehaviour] || sndFile.m_PlayBehaviour[PlayBehaviour.MptOldSwingBehaviour] ? chn.nVolSwing : 0;
						c_int vol = Util.MulDiv((chn.nVolume + swing) * playState.m_nGlobalVolume, chn.nGlobalVol * chn.nInsVol, 1 << 20);
						data = (uint8)OpenMpt.Clamp(vol / 2, 1, 127);
					}
				}
				else if (macro[pos] == 'u')
				{
					// Calculated volume
					// Same note as with velocity applies here, but apparently also for instrument / sample volumes?
					if ((chn != null) && (playState != null))
					{
						c_int vol = Util.MulDiv(chn.nCalcVolume * playState.m_nGlobalVolume, chn.nGlobalVol * chn.nInsVol, 1 << 26);
						data = (uint8)OpenMpt.Clamp(vol / 2, 1, 127);
					}
				}
				else if (macro[pos] == 'x')
				{
					// Pan set
					if (chn != null)
						data = (uint8)(Math.Min(chn.nPan / 2, 127));
				}
				else if (macro[pos] == 'y')
				{
					// Calculated pan
					if (chn != null)
						data = (uint8)(Math.Min(chn.nRealPan / 2, 127));
				}
				else if (macro[pos] == 'a')
				{
					// High byte of bank select
					if ((pIns != null) && (pIns.wMidiBank != 0))
						data = (uint8)(((pIns.wMidiBank - 1) >> 7) & 0x7f);
				}
				else if (macro[pos] == 'b')
				{
					// Low byte of bank select
					if ((pIns != null) && (pIns.wMidiBank != 0))
						data = (uint8)((pIns.wMidiBank - 1) & 0x7f);
				}
				else if (macro[pos] == 'o')
				{
					// Offset (ignoring high offset)
					if (chn != null)
						data = (uint8)((chn.OldOffset >> 8) & 0xff);
				}
				else if (macro[pos] == 'h')
				{
					// Host channel number
					if (chn != null)
						data = (uint8)((nChn >= sndFile.GetNumChannels() ? (chn.nMasterChn - 1) : nChn) & 0x7f);
				}
				else if (macro[pos] == 'm')
				{
					// Loop direction (on sample channels - MIDI note on MIDI channels)
					if (chn != null)
						data = (uint8)(chn.dwFlags.Test(ChannelFlags.Chn_PingPongFlag) ? 1 : 0);
				}
				else if (macro[pos] == 'p')
				{
					// Program select
					if ((pIns != null) && (pIns.nMidiProgram != 0))
						data = (uint8)((pIns.nMidiProgram - 1) & 0x7f);
				}
				else if (macro[pos] == 'z')
				{
					// Zxx parameter
					data = param;

					if (isSmooth && (playState != null) && (chn != null) && (chn.LastZxxParam < 0x80) && ((outPos < 3) || (@out[outPos - 3] != 0xf0) || (@out[outPos - 2] < 0xf0)))
					{
						// Interpolation for external MIDI messages - interpolation for internal messages
						// is handled separately to allow for more than 7-bit granularity where it's possible
						data = (uint8)CSoundFile.CalculateSmoothParamChange(playState, lastZxxParam, data);
						chn.LastZxxParam = data;
						updateZxxParam = 0x80;
					}
					else if (updateZxxParam == 0xff)
						updateZxxParam = data;
				}
				else if (macro[pos] == 's')
				{
					// SysEx Checksum (not an original Impulse Tracker macro variable, but added for convenience)
					if (!firstNibble)	// From MIDI.TXT: '9n' is exactly the same as '09 n' or '9 n' -- so finish current byte first
					{
						outPos++;
						firstNibble = true;
					}

					size_t startPos = outPos;

					while ((startPos > 0) && (@out[--startPos] != 0xf0))
						;

					if (((outPos - startPos) < 3) || (@out[startPos] != 0xf0))
						continue;

					// If first byte of model number is 0, read one more
					uint8 checksumStart = (uint8)(@out[startPos + 3] != 0 ? 5 : 6);

					if ((outPos - startPos) < checksumStart)
						continue;

					for (size_t p = startPos + checksumStart; p != outPos; p++)
						data += @out[p];

					data = (uint8)((~data + 1) & 0x7f);
				}
				else
				{
					// Unrecognized byte (e.g. space char)
					continue;
				}

				// Append parsed data
				if (isNibble)	// Parsed a nibble (constant or 'c' variable)
				{
					if (firstNibble)
						@out[outPos] = data;
					else
					{
						@out[outPos] = (uint8)((@out[outPos] << 4) | data);
						outPos++;
					}

					firstNibble = !firstNibble;
				}
				else	// Parsed a byte (variable)
				{
					if (!firstNibble)	// From MIDI.TXT: '9n' is exactly the same as '09 n' or '9 n' -- so finish current byte first
						outPos++;

					@out[outPos++] = data;
					firstNibble = true;
				}
			}

			// Finish current byte
			if (!firstNibble)
				outPos++;

			if ((chn != null) && (updateZxxParam < 0x80))
				chn.LastZxxParam = updateZxxParam;

			// Add end of SysEx byte if necessary
			for (size_t i = 0; i < outPos; i++)
			{
				if (@out[i] != 0xf0)
					continue;

				if (((outPos - i) >= 4) && ((@out[i + 1] == 0xf0) || (@out[i + 1] == 0xf1)))
				{
					// Internal message
					i += 3;
				}
				else
				{
					// Real SysEx
					while ((i < outPos) && (@out[i] != 0xf7))
						i++;

					if ((i == outPos) && (outPos < @out.Size()))
						@out[outPos++] = 0xf7;
				}
			}

			m_Data = @out.First(outPos);
		}



		/********************************************************************/
		/// <summary>
		///
		/// </summary>
		/********************************************************************/
		public bool NextMessage(out MptSpan<uint8> message, bool outputRunningStatus = true)//XX 20
		{
			if (m_RunningStatusPos < m_Data.Size())
			{
				m_Data[m_RunningStatusPos] = m_RunningStatusOldData;
				m_RunningStatusPos = uint32.MaxValue;
			}

			uint32 outSize = (uint32)m_Data.Size();

			while (m_SendPos < outSize)
			{
				uint32 sendLen = 0;

				if (m_Data[m_SendPos] == 0xf0)
				{
					// SysEx start
					if (((outSize - m_SendPos) >= 4) && ((m_Data[m_SendPos + 1] == 0xf0) || (m_Data[m_SendPos + 1] == 0xf1)))
					{
						// Internal macro (normal (F0F0) or extended (F0F1)), 4 bytes long
						sendLen = 4;
					}
					else
					{
						// SysEx message, find end of message
						sendLen = outSize - m_SendPos;

						for (uint32 i = (m_SendPos + 1); i < outSize; i++)
						{
							if (m_Data[i] == 0xf7)
							{
								// Found end of SysEx message
								sendLen = i - m_SendPos + 1;
								break;
							}
						}
					}
				}
				else if ((m_Data[m_SendPos] & 0x80) == 0)
				{
					// Missing status byte? Try inserting running status
					if (m_RunningStatus != 0)
					{
						if (outputRunningStatus)
						{
							m_SendPos--;
							m_RunningStatusOldData = m_Data[m_SendPos];
							m_Data[m_SendPos] = m_RunningStatus;
							m_RunningStatusPos = m_SendPos;
							continue;
						}
						else
							sendLen = Math.Min((uint32)(MidiEvents.GetEventLength(m_RunningStatus) - 1), outSize - m_SendPos);
					}
					else
					{
						//  No running status to re-use; skip this byte
						m_SendPos++;
						continue;
					}
				}
				else
				{
					// Other MIDI messages
					sendLen = Math.Min((uint32)MidiEvents.GetEventLength(m_Data[m_SendPos]), outSize - m_SendPos);
				}

				if (sendLen == 0)
					break;

				if (m_Data[m_SendPos] < 0xf0)
					m_RunningStatus = m_Data[m_SendPos];

				message = m_Data.SubSpan(m_SendPos, sendLen);
				m_SendPos += sendLen;

				return true;
			}

			message = null;

			return false;
		}
	}
}
