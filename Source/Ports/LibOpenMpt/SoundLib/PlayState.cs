/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.CompilerServices;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib
{
	/// <summary>
	/// This class represents all of the playback state of a module
	/// </summary>
	internal class PlayState : IDeepCloneable<PlayState>
	{
		public class MidiMacroEvaluationResults : IDeepCloneable<MidiMacroEvaluationResults>
		{
			public map<PlugIndex, c_float> pluginDryWetRatio = new map<PlugIndex, c_float>();
			public map<pair<PlugIndex, PlugParamIndex>, PlugParamValue> pluginParameter = new map<pair<PlugIndex, PlugParamIndex>, PlugParamValue>();

			/********************************************************************/
			/// <summary>
			/// Make a deep copy of the current object
			/// </summary>
			/********************************************************************/
			public MidiMacroEvaluationResults MakeDeepClone()
			{
				MidiMacroEvaluationResults clone = (MidiMacroEvaluationResults)MemberwiseClone();

				clone.pluginDryWetRatio = pluginDryWetRatio.MakeDeepClone();
				clone.pluginParameter = pluginParameter.MakeDeepClone();

				return clone;
			}
		}

		/// <summary>
		/// Total number of rendered samples
		/// </summary>
		public samplecount_t m_lTotalSampleCount = 0;

		/// <summary>
		/// Remaining number samples to render for this tick
		/// </summary>
		public samplecount_t m_nBufferCount = 0;

		/// <summary>
		/// Modern tempo rounding error compensation
		/// </summary>
		public c_double m_dBufferDiff = 0.0;

		/// <summary>
		/// Fractional PPQ position within current measure
		/// </summary>
		public c_double m_ppqPosFract = 0.0;

		/// <summary>
		/// PPQ position of the last start of measure
		/// </summary>
		public uint32 m_ppqPosBeat = 0;

		/// <summary>
		/// Current tick being processed
		/// </summary>
		public uint32 m_nTickCount = 0;

		/// <summary>
		/// Pattern delay (rows)
		/// </summary>
		public uint32 m_nPatternDelay = 0;

		/// <summary>
		/// Fine pattern delay (ticks)
		/// </summary>
		public uint32 m_nFrameDelay = 0;

		/// <summary>
		/// 
		/// </summary>
		public uint32 m_nSamplesPerTick = 0;

		/// <summary>
		/// Current time signature
		/// </summary>
		public RowIndex m_nCurrentRowsPerBeat = 0;

		/// <summary>
		/// Current time signature
		/// </summary>
		public RowIndex m_nCurrentRowsPerMeasure = 0;

		/// <summary>
		/// Current speed
		/// </summary>
		public uint32 m_nMusicSpeed = 0;

		/// <summary>
		/// Current tempo
		/// </summary>
		public Tempo m_nMusicTempo = new Tempo();

		/// <summary>
		/// Current row being processed
		/// </summary>
		public RowIndex m_nRow = 0;

		/// <summary>
		/// Next row to process
		/// </summary>
		public RowIndex m_nNextRow = 0;

		/// <summary>
		/// For FT2's E60 bug
		/// </summary>
		public RowIndex m_NextPatStartRow = 0;

		/// <summary>
		/// Candidate target row for pattern break
		/// </summary>
		public RowIndex m_BreakRow = 0;

		/// <summary>
		/// Candidate target row for pattern loop
		/// </summary>
		public RowIndex m_PatLoopRow = 0;

		/// <summary>
		/// Candidate target order for position jump
		/// </summary>
		public OrderIndex m_PosJump = 0;

		/// <summary>
		/// Current pattern being processed
		/// </summary>
		public PatternIndex m_nPattern = 0;

		/// <summary>
		/// Current order being processed
		/// </summary>
		public OrderIndex m_nCurrentOrder = 0;

		/// <summary>
		/// Next order to process
		/// </summary>
		public OrderIndex m_nNextOrder = 0;

		/// <summary>
		/// Queued order to be processed next, regardless of what order would normally follow
		/// </summary>
		public OrderIndex m_nSeqOverride = Snd_Def.OrderIndex_Invalid;

		/// <summary>
		/// 
		/// </summary>
		public OrderTransitionMode m_SeqOverrideMode = OrderTransitionMode.AtPatternEnd;

		/// <summary>
		/// Current global volume (0...MAX_GLOBAL_VOLUME)
		/// </summary>
		public int32 m_nGlobalVolume = (int32)Snd_Def.Max_Global_Volume;

		/// <summary>
		/// 
		/// </summary>
		public int32 m_nSamplesToGlobalVolRampDest = 0;

		/// <summary>
		/// 
		/// </summary>
		public int32 m_nGlobalVolumeRampAmount = 0;

		/// <summary>
		/// 
		/// </summary>
		public int32 m_nGlobalVolumeDestination = 0;

		/// <summary>
		/// 
		/// </summary>
		public int32 m_lHighResRampingGlobalVolume = 0;

		/// <summary>
		/// 
		/// </summary>
		public PlayFlags m_Flags = PlayFlags.Song_PositionChanged;

		/// <summary>
		/// Index of channels in Chn to be actually mixed
		/// </summary>
		public array<ChannelIndex> ChnMix = new array<ChannelIndex>(Snd_Def.Max_Channels);

		/// <summary>
		/// Mixing channels... First m_nChannels channels are directly mapped
		/// to pattern channels (i.e. they are never NNA channels)
		/// </summary>
		public array<ModChannel> Chn = new array<ModChannel>(Snd_Def.Max_Channels);

		/// <summary>
		/// 
		/// </summary>
		public GlobalScriptState m_GlobalScriptState = new GlobalScriptState();

		/// <summary>
		/// 
		/// </summary>
		public vector<uint8> m_MidiMacroScratchSpace = new vector<uint8>();

		/// <summary>
		/// 
		/// </summary>
		public MidiMacroEvaluationResults m_MidiMacroEvaluationResults = null;

		/// <summary>
		/// TNE: Added so it is possible to enable/disable surround
		/// </summary>
		public bool m_SurroundEnabled = true;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public PlayState()
		{
			Chn.fill(new ModChannel());

			// Note: If macros ever become variable-length, the scratch space needs to be at least
			// one byte longer than the longest macro in the file for end-of-SysEx insertion to
			// stay allocation-free in the mixer
			m_MidiMacroScratchSpace.reserve(MidiMacros.MacroLength);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public uint32 TicksOnRow()
		{
			return (m_nMusicSpeed + m_nFrameDelay) * Math.Max(m_nPatternDelay, 1);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void ResetGlobalVolumeRamping()//XX 28
		{
			m_lHighResRampingGlobalVolume = m_nGlobalVolume << Mixer.VolumeRampPrecision;
			m_nGlobalVolumeDestination = m_nGlobalVolume;
			m_nSamplesToGlobalVolRampDest = 0;
			m_nGlobalVolumeRampAmount = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void UpdateTimeSignature(CSoundFile sndFile)//XX 37
		{
			if (!sndFile.Patterns.IsValidIndex(m_nPattern) || !sndFile.Patterns[m_nPattern].GetOverrideSignature())
			{
				m_nCurrentRowsPerBeat = sndFile.m_nDefaultRowsPerBeat;
				m_nCurrentRowsPerMeasure = sndFile.m_nDefaultRowsPerMeasure;
			}
			else
			{
				m_nCurrentRowsPerBeat = sndFile.Patterns[m_nPattern].GetRowsPerBeat();
				m_nCurrentRowsPerMeasure = sndFile.Patterns[m_nPattern].GetRowsPerMeasure();
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void UpdatePpq(bool patternTransition)//XX 51
		{
			RowIndex rpm = m_nCurrentRowsPerMeasure != 0 ? m_nCurrentRowsPerMeasure : Snd_Def.Default_Rows_Per_Measure;
			RowIndex rpb = m_nCurrentRowsPerBeat != 0 ? m_nCurrentRowsPerBeat : Snd_Def.Default_Rows_Per_Beat;

			if ((m_lTotalSampleCount > 0) && (patternTransition || ((m_nRow % rpm) == 0)))
			{
				// Pattern end = end of measure, so round up PPQ to the next full measure
				m_ppqPosBeat += (rpm + (rpb - 1)) / rpb;
				m_ppqPosFract = 0;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public MptSpan<ModChannel> PatternChannels(CSoundFile sndFile)//XX 71
		{
			return new MptSpan<ModChannel>(Chn).SubSpan(0, Math.Min(Chn.size(), sndFile.GetNumChannels()));
		}



		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public PlayState MakeDeepClone()
		{
			PlayState clone = (PlayState)MemberwiseClone();

			clone.ChnMix = ChnMix.MakeDeepClone();
			clone.Chn = Chn.MakeDeepClone();
			clone.m_GlobalScriptState = m_GlobalScriptState.MakeDeepClone();
			clone.m_MidiMacroScratchSpace = m_MidiMacroScratchSpace.MakeDeepClone();
			clone.m_MidiMacroEvaluationResults = m_MidiMacroEvaluationResults?.MakeDeepClone();

			return clone;
		}
	}
}
