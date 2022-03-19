/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.OctaMed.Containers;
using Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation.Block;
using Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation.Sequences;
using Polycode.NostalgicPlayer.Kit;

namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation
{
	/// <summary>
	/// Sub-song interface
	/// </summary>
	internal class SubSong
	{
		[Flags]
		private enum SongFlags
		{
			VolHex = 0x01,
			StSlide = 0x02,
			Dummy = 0,
			StereoMode = 0x04,
			FreePan = 0x08,
			Gm = 0x10
		}

		private readonly OctaMedWorker worker;

		private readonly List<MedBlock> blocks = new List<MedBlock>();
		private readonly List<PlaySeq> pSeqs = new List<PlaySeq>();
		private readonly List<SectSeqEntry> sections = new List<SectSeqEntry>();
		private Tempo tempo = new Tempo();
		private Tempo startTempo = new Tempo();
		private readonly LimVar<short> playTransp = new(-128, 127);
		private readonly LimVar<uint> channels = new(1, Constants.MaxTracks);
		private readonly LimVar<short> volAdj = new(1, 800);
		private readonly byte[] trkVol = new byte[Constants.MaxTracks];
		private readonly sbyte[] trkPan = new sbyte[Constants.MaxTracks];
		private readonly LimVar<short> masterVol = new(1, 64);

		private SongFlags flags;
		private string name;
		private PlayPosition position;
		private bool filter;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SubSong(OctaMedWorker worker, bool empty)
		{
			// Initialize member variables
			position = new PlayPosition(worker);

			this.worker = worker;

			playTransp.Value = 0;
			channels.Value = 4;
			volAdj.Value = 100;
			masterVol.Value = 64;
			filter = false;

			position.SetParent(this);

			// Set default values
			for (TrackNum cnt = 0; cnt < Constants.MaxTracks; cnt++)
			{
				SetTrackVol(cnt, 64);
				SetTrackPan(cnt, 0);
			}

			flags = SongFlags.StereoMode;

			if (!empty)
			{
				Append(new MedBlock(64, 4));
				AppendNewSec(0);
				PlaySeq newPSeq = new PlaySeq();
				Append(newPSeq);
				newPSeq.Add(new PlaySeqEntry(0));
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void AppendNewSec(ushort secNum)
		{
			Append(new SectSeqEntry(secNum));
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Append(SectSeqEntry sec)
		{
			if (sec != null)
				sections.Add(sec);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Append(PlaySeq pSeq)
		{
			pSeqs.Add(pSeq);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Append(MedBlock blk)
		{
			blocks.Add(blk);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public PlaySeq PSeq(uint pos)
		{
			return pSeqs[(int)pos];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public MedBlock Block(BlockNum pos)
		{
			return blocks[(int)pos];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public SectSeqEntry Sect(uint pos)
		{
			return sections[(int)pos];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public PlayPosition Pos()
		{
			return position;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetPos(PlayPosition newPos)
		{
			position = newPos;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetTempoBpm(ushort newBpm)
		{
			tempo.tempo.Value = newBpm;
			worker.plr.SetMixTempo(tempo);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetTempoTpl(ushort newTpl)
		{
			if (newTpl != 0)
				tempo.ticksPerLine.Value = newTpl;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetTempoLpb(ushort newLpb)
		{
			tempo.linesPerBeat.Value = newLpb;
			worker.plr.SetMixTempo(tempo);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetTempoMode(bool bpm)
		{
			tempo.bpm = bpm;
			worker.plr.SetMixTempo(tempo);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetTempo(Tempo tempo)
		{
			this.tempo = new Tempo(tempo);
			worker.plr.SetMixTempo(tempo);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetStartTempo(Tempo tempo)
		{
			startTempo = new Tempo(tempo);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetPlayTranspose(short pTransp)
		{
			playTransp.Value = pTransp;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetNumChannels(uint channels)
		{
			worker.plr.Stop();
			this.channels.Value = channels;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetVolAdjust(short volAdjust)
		{
			volAdj.Value = volAdjust;
			worker.plr.RecalcVolAdjust();
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetMasterVol(int vol)
		{
			masterVol.Value = (short)vol;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetTrackVol(TrackNum trk, int vol)
		{
			trkVol[trk] = (byte)vol;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetTrackPan(TrackNum trk, int pan)
		{
			trkPan[trk] = (sbyte)pan;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetStereo(bool stereo)
		{
			if (stereo)
				flags |= SongFlags.StereoMode;
			else
				flags &= ~SongFlags.StereoMode;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetSlide1st(bool slide)
		{
			if (slide)
				flags |= SongFlags.StSlide;
			else
				flags &= ~SongFlags.StSlide;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetGm(bool gmMode)
		{
			if (gmMode)
				flags |= SongFlags.Gm;
			else
				flags &= ~SongFlags.Gm;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetFreePan(bool fp)
		{
			if (fp)
				flags |= SongFlags.FreePan;
			else
				flags &= ~SongFlags.FreePan;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetAmigaFilter(bool amigaFilter)
		{
			filter = amigaFilter;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetSongName(byte[] name)
		{
			this.name = EncoderCollection.Amiga.GetString(name);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint NumSections()
		{
			return (uint)sections.Count;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint NumBlocks()
		{
			return (uint)blocks.Count;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint NumPlaySeqs()
		{
			return (uint)pSeqs.Count;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ushort GetTempoBpm()
		{
			return tempo.tempo.Value;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public ushort GetTempoTpl()
		{
			return tempo.ticksPerLine.Value;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Tempo GetTempo()
		{
			return tempo;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public Tempo GetStartTempo()
		{
			return startTempo;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public short GetPlayTranspose()
		{
			return playTransp.Value;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public uint GetNumChannels()
		{
			return channels.Value;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public short GetMasterVol()
		{
			return masterVol.Value;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int GetTrackVol(TrackNum trk)
		{
			return trkVol[trk];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int GetTrackPan(TrackNum trk)
		{
			return trkPan[trk];
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool GetSlide1st()
		{
			return (flags & SongFlags.StSlide) != 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public bool GetAmigaFilter()
		{
			return filter;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public string GetSongName()
		{
			return name ?? string.Empty;
		}
	}
}
