/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.C;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.String;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.FileFormat_Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;
using Version = Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.Version;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// IT (Impulse Tracker) module loader
	/// Notes: Also handles MPTM loading, as the formats are almost identical
	/// </summary>
	internal partial class CSoundFile
	{
		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Get version of Impulse Tracker that was used to create an IT/S3M
		/// file
		/// </summary>
		/********************************************************************/
		private string GetImpulseTrackerVersion(uint16 cwtv, uint16 cmwt)//XX 345
		{
			string version;

			cwtv &= 0xfff;

			if (cmwt > 0x0214)
				version = "Impulse Tracker 2.15";
			else if ((cwtv >= 0x0215) && (cwtv <= 0x0217))
			{
				string[] versions = [ "1-2", "3", "4-5" ];

				version = string.Format("Impulse Tracker 2.14p{0}", versions[cwtv - 0x0215]);
			}
			else
				version = string.Format("Impulse Tracker {0}.{1:x2}", (cwtv & 0x0f00) >> 8, cwtv & 0xff);

			return version;
		}



		/********************************************************************/
		/// <summary>
		/// Get version of Schism Tracker that was used to create an IT/S3M
		/// file
		/// </summary>
		/********************************************************************/
		private string GetSchismTrackerVersion(uint16 cwtv, uint32 reserved)//XX 365
		{
			// Schism Tracker version information in a nutshell:
			// < 0x020: a proper version (files saved by such versions are likely very rare)
			// = 0x020: any version between the 0.2a release (2005-04-29?) and 2007-04-17
			// = 0x050: anywhere from 2007-04-17 to 2009-10-31
			// > 0x050: the number of days since 2009-10-31
			// = 0xFFF: any version starting from 2020-10-28 (exact version stored in reserved value)
			cwtv &= 0xfff;

			if (cwtv > 0x50)
			{
				int32 date = (int32)(ItTools.SchismTrackerEpoch + (cwtv < 0xfff ? cwtv - 0x050 : reserved));
				int32 y = (int32)((Util.Mul32To64(10000, date) + 14780) / 3652425);
				int32 ddd = date - ((365 * y) + (y / 4) - (y / 100) + (y / 400));

				if (ddd < 0)
				{
					y--;
					ddd = date - ((365 * y) + (y / 4) - (y / 100) + (y / 400));
				}

				int32 mi = ((100 * ddd) + 52) / 3060;

				return string.Format("Schism Tracker {0:D4}-{1:D2}-{2:D2}", y + ((mi + 2) / 12), ((mi + 2) % 12) + 1, ddd - (((mi * 306) + 5) / 10) + 1);
			}
			else
				return string.Format("Schism Tracker 0.{0:x2}", cwtv);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private pair<bool, bool> LoadMixPlugins(FileReader file, bool ignoreChannelCount = true)//XX 2199
		{
			bool hasPluginChunks = false, isBeRoTracker = false;

			while (file.CanRead(9))
			{
				uint8[] code = new uint8[4];

				file.ReadArray(code);
				uint32 chunkSize = file.ReadUInt32LE();

				if ((CMemory.memcmp(code, "IMPI", 4) == 0) ||		// IT instrument, we definitely read too far
					(CMemory.memcmp(code, "IMPS", 4) == 0) ||		// IT sample, ditto
					(CMemory.memcmp(code, "XTPM", 4) == 0) ||		// Instrument extensions, ditto
					(CMemory.memcmp(code, "STPM", 4) == 0) ||		// Song extensions, ditto
					!file.CanRead(chunkSize))
				{
					file.SkipBack(8);

					return new pair<bool, bool>(hasPluginChunks, isBeRoTracker);
				}

				FileReader chunk = new FileReader(file.ReadChunk(chunkSize));

				// Channel FX
				if (CMemory.memcmp(code, "CHFX", 4) == 0)
				{
					if (!ignoreChannelCount)
						ChnSettings.resize(Math.Clamp((ChannelIndex)(chunkSize / 4), GetNumChannels(), Snd_Def.Max_BaseChannels));

					foreach (ModChannelSettings chn in ChnSettings)
						chn.nMixPlugin = (PlugIndex)chunk.ReadUInt32LE();

					hasPluginChunks = true;
				}
				else if ((code[0] == 'F') && ((code[1] == 'X') || char.IsAsciiDigit((char)code[1])) && char.IsAsciiDigit((char)code[2]) && char.IsAsciiDigit((char)code[3]))
				{
					uint16 fxPlug = (uint16)(((code[2] - '0') * 10) + (code[3] - '0'));	// Calculate plug-in number

					if (code[1] != 'X')
						fxPlug += (uint16)((code[1] - '0') * 100);

					if (fxPlug < Snd_Def.Max_MixPlugins)
					{
						PlugIndex plug = (PlugIndex)fxPlug;
						ReadMixPluginChunk(chunk/*, m_MixPlugins[plug]*/);//XX
					}

					hasPluginChunks = true;
				}
				else if (CMemory.memcmp(code, "MODU", 4) == 0)
				{
					isBeRoTracker = true;
					m_dwLastSavedWithVersion = new Version();	// Reset MPT detection for old files that have a similar fingerprint
				}
			}

			return new pair<bool, bool>(hasPluginChunks, isBeRoTracker);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void ReadMixPluginChunk(FileReader file)//XX 2257
		{
			//XX Not done yet. Just collect the plug-in names
			file.Skip(64);

			uint8[] buffer = new uint8[64];
			file.ReadArray(buffer);

			string pluginName = EncoderCollection.Win1252.GetString(buffer);
			unsupportedPlugins.Add(pluginName);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void ReadField<T>(FileReader chunk, size_t size, out T field)//XX 2528
		{
			field = chunk.ReadSizedIntLE<T>(size);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static void ReadFieldCast<T>(FileReader chunk, size_t size, out T field) where T : Enum //XX 2535
		{
			field = (T)Enum.ToObject(typeof(T), chunk.ReadSizedIntLE<int32>(size));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool LoadExtendedSongProperties(FileReader file, bool ignoreChannelCount, ref bool pInterpretMptMade)//XX 2542
		{
			if (!file.ReadMagic("STPM"))	// 'MPTS'
				return false;

			// Found MPTS, interpret the file MPT made
			pInterpretMptMade = true;

			// HACK: Reset mod flags to default values here, as they are not always written
			m_PlayBehaviour.reset();

			while (file.CanRead(7))
			{
				uint32 code = file.ReadUInt32LE();
				uint16 size = file.ReadUInt16LE();

				// Start of MPTM extensions, non-ASCII ID or truncated field
				if (code == Magic.MagicLE("228\x04"))
				{
					file.SkipBack(6);
					break;
				}
				else if (((code & 0x80808080) != 0) || ((code & 0x60606060) == 0) || !file.CanRead(size))
					break;

				FileReader chunk = new FileReader(file.ReadChunk(size));

				switch (Magic.MagicToStringBE(code))	// Interpret field code
				{
					case "DT..":
					{
						ReadField(chunk, size, out uint32 tempo);
						Order.Current.SetDefaultTempo(new Tempo(tempo, Order.Current.GetDefaultTempo().GetFract()));
						break;
					}

					case "RPB.":
					{
						ReadField(chunk, size, out m_nDefaultRowsPerBeat);
						break;
					}

					case "RPM.":
					{
						ReadField(chunk, size, out m_nDefaultRowsPerMeasure);
						break;
					}

					case "C...":
					{
						if (!ignoreChannelCount)
						{
							ReadField(chunk, size, out ChannelIndex chn);
							ChnSettings.resize(OpenMpt.Clamp(chn, GetNumChannels(), Snd_Def.Max_BaseChannels));
						}

						break;
					}

					case "TM..":
					{
						ReadFieldCast(chunk, size, out m_nTempoMode);
						break;
					}

					case "PMM.":
					{
						ReadFieldCast(chunk, size, out m_nMixLevels);
						break;
					}

					case "CWV.":
					{
						ReadField(chunk, size, out uint32 ver);
						m_dwCreatedWithVersion = new Version(ver);
						break;
					}

					case "LSWV":
					{
						ReadField(chunk, size, out uint32 ver);

						if (ver != 0)
							m_dwLastSavedWithVersion = new Version(ver);

						break;
					}

					case "SPA.":
					{
						ReadField(chunk, size, out m_nSamplePreAmp);
						break;
					}

					case "VSTV":
					{
						ReadField(chunk, size, out m_nVstiVolume);
						break;
					}

					case "DGV.":
					{
						ReadField(chunk, size, out m_nDefaultGlobalVolume);
						break;
					}

					case "RP..":
					{
						if (GetType_() != ModType.Xm)
						{
							ReadField(chunk, size, out OrderIndex restartPos);
							Order.Current.SetRestartPos(restartPos);
						}

						break;
					}

					case "ChnS":
					{
						// Channel settings for channels 65+
						if ((size <= ((Snd_Def.Max_BaseChannels - 64) * 2)) && ((size % 2) == 0) && ((GetType_() & (ModType.It | ModType.Mpt)) != 0))
						{
							ChannelIndex channelsInFile = ChannelIndex.CreateSaturating(64 + (size / 2));

							if (!ignoreChannelCount)
								ChnSettings.resize(Math.Clamp(GetNumChannels(), channelsInFile, Snd_Def.Max_BaseChannels));

							ChannelIndex numChannels = Math.Min(channelsInFile, GetNumChannels());

							for (ChannelIndex chn = 64; chn < numChannels; chn++)
							{
								uint8 pan = chunk.ReadUInt8();
								uint8 vol = chunk.ReadUInt8();

								if (pan != 0xff)
								{
									ChnSettings[chn].nVolume = vol;
									ChnSettings[chn].nPan = 128;
									ChnSettings[chn].dwFlags.Reset();

									if ((pan & 0x80) != 0)
										ChnSettings[chn].dwFlags.Set(ChannelFlags.Chn_Mute);

									pan &= 0x7f;

									if (pan <= 64)
										ChnSettings[chn].nPan = (uint16)(pan << 2);

									if (pan == 100)
										ChnSettings[chn].dwFlags.Set(ChannelFlags.Chn_Surround);
								}
							}
						}

						break;
					}

					case "MSF.":
					{
						// Playback compatibility flags
						size_t bit = 0;
						m_PlayBehaviour.reset();

						while (chunk.CanRead(1) && (bit < m_PlayBehaviour.size()))
						{
							uint8 b = chunk.ReadUInt8();

							for (uint8 i = 0; i < 8; i++, bit++)
							{
								if (((b & (1 << i)) != 0) && (bit < m_PlayBehaviour.size()))
									m_PlayBehaviour.set((PlayBehaviour)bit);
							}
						}

						break;
					}
				}

				switch (Magic.MagicToStringLE(code))	// Interpret field code
				{
					case "DTFR":
					{
						ReadField(chunk, size, out uint32 tempoFract);
						Order.Current.SetDefaultTempo(new Tempo(Order.Current.GetDefaultTempo().GetInt(), tempoFract));
						break;
					}

					case "RSMP":
					{
						ReadFieldCast(chunk, size, out m_nResampling);

						if (!Resampling.IsKnownMode(m_nResampling))
							m_nResampling = ResamplingMode.Default;

						break;
					}

					case "AUTH":
					{
						StdString artist = new StdString();

						chunk.ReadString(ReadWriteMode.SpacePadded, artist, chunk.GetLength());
						m_SongArtist = Encoding.UTF8.GetString(artist.Span());
						break;
					}

					case "CUES":
					{
						// Sample cues
						if (size > 2)
						{
							SampleIndex smp = chunk.ReadUInt16LE();

							if ((smp > 0) && (smp <= GetNumSamples()))
							{
								ModSample sample = Samples[smp];

								for (size_t i = 0; i < sample.Cues.size(); i++)
								{
									SmpLength cue;

									if (chunk.CanRead(4))
										cue = chunk.ReadUInt32LE();
									else
										cue = Snd_Def.Max_Sample_Length;

									sample.Cues[i] = cue;
								}
							}
						}

						break;
					}

					case "SWNG":
					{
						// Tempo Swing Factors
						if (size > 2)
						{
							using (MemoryStream ms = new MemoryStream(chunk.ReadRawDataAsByteVector().data().ToArray()))//XX Hvor mange bytes snakker vi om her?
							{
								TempoSwing.Deserialize(ms, ref m_TempoSwing, chunk.GetLength());
							}
						}

						break;
					}
				}
			}

			// Validate read values
			Order.Current.SetDefaultTempo(OpenMpt.Clamp(Order.Current.GetDefaultTempo(), GetModSpecifications().GetTempoMin(), GetModSpecifications().GetTempoMax()));

			if (m_nTempoMode >= TempoMode.NumModes)
				m_nTempoMode = TempoMode.Classic;

			if (m_nMixLevels >= MixLevels.NumMixLevels)
				m_nMixLevels = MixLevels.Original;

			return true;
		}
		#endregion
	}
}
