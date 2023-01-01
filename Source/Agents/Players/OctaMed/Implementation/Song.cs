/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Player.OctaMed.Containers;
using Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation.Synth;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation
{
	/// <summary>
	/// Song interface
	/// </summary>
	internal class Song
	{
		private readonly OctaMedWorker worker;

		private readonly List<SubSong> songs = new List<SubSong>();
		private readonly Instr[] inst = new Instr[Constants.MaxInstr];
		private readonly Sample[] samp = new Sample[Constants.MaxInstr];

		private SubSong current;
		private uint currNum;
		private uint currInstr;		// Currently selected instrument #
		private string annoText;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Song(OctaMedWorker worker)
		{
			// Initialize member variables
			this.worker = worker;
			current = null;
			currNum = 0;
			currInstr = 0;

			for (InstNum cnt = 0; cnt < Constants.MaxInstr; cnt++)
			{
				samp[cnt] = null;
				inst[cnt] = new Instr();
				inst[cnt].SetNum(worker, cnt);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will try to lookup an instrument for a sample
		/// </summary>
		/********************************************************************/
		public Instr Sample2Instrument(Sample sample)
		{
			for (int i = 0; i < Constants.MaxInstr; i++)
			{
				if (sample == samp[i])
					return inst[i];
			}

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Will append a new sub-song
		/// </summary>
		/********************************************************************/
		public void AppendNew(bool empty)
		{
			SubSong ss = new SubSong(worker, empty);

			songs.Add(ss);
			if (current == null)
				current = ss;
		}



		/********************************************************************/
		/// <summary>
		/// Return the current sub-song
		/// </summary>
		/********************************************************************/
		public SubSong CurrSS()
		{
			return current;
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of sub-songs in the song object
		/// </summary>
		/********************************************************************/
		public uint NumSubSongs()
		{
			return (uint)songs.Count;
		}



		/********************************************************************/
		/// <summary>
		/// Will set the pointer to point on the sub-song given
		/// </summary>
		/********************************************************************/
		public void SetSSNum(int ssNum)
		{
			if (ssNum < 0)
				ssNum = 0;

			currNum = Math.Min(NumSubSongs() - 1, (uint)ssNum);
			current = songs[(int)currNum];
		}



		/********************************************************************/
		/// <summary>
		/// Will check to see if the sample slot at the given instrument is
		/// used or not
		/// </summary>
		/********************************************************************/
		public bool SampleSlotUsed(InstNum iNum)
		{
			return samp[iNum] != null;
		}



		/********************************************************************/
		/// <summary>
		/// Will check to see if the instrument slot at the given instrument
		/// is used or not
		/// </summary>
		/********************************************************************/
		public bool InstrSlotUsed(InstNum iNum)
		{
			return (samp[iNum] != null) || inst[iNum].IsMidi();
		}



		/********************************************************************/
		/// <summary>
		/// Return the sub-song given
		/// </summary>
		/********************************************************************/
		public SubSong GetSubSong(uint sNum)
		{
			return songs[(int)sNum];
		}



		/********************************************************************/
		/// <summary>
		/// Return the sample given
		/// </summary>
		/********************************************************************/
		public Sample GetSample(InstNum num)
		{
			return samp[num];
		}



		/********************************************************************/
		/// <summary>
		/// Return the synth sound object on the instrument given
		/// </summary>
		/********************************************************************/
		public SynthSound GetSynthSound(InstNum num)
		{
			return samp[num] as SynthSound;
		}



		/********************************************************************/
		/// <summary>
		/// Return the current instrument
		/// </summary>
		/********************************************************************/
		public Instr CurrInstr()
		{
			return inst[currInstr];
		}



		/********************************************************************/
		/// <summary>
		/// Return the instrument given
		/// </summary>
		/********************************************************************/
		public Instr GetInstr(InstNum iNum)
		{
			return inst[iNum];
		}



		/********************************************************************/
		/// <summary>
		/// Return the current instrument number
		/// </summary>
		/********************************************************************/
		public InstNum CurrInstrNum()
		{
			return currInstr;
		}



		/********************************************************************/
		/// <summary>
		/// Return the annotation text
		/// </summary>
		/********************************************************************/
		public string GetAnnoText()
		{
			return annoText;
		}



		/********************************************************************/
		/// <summary>
		/// Assign the sample to the instrument
		/// </summary>
		/********************************************************************/
		public void SetSample(InstNum num, Sample s)
		{
			if (s != samp[num])
			{
				samp[num] = s;
				inst[num].ValidateLoop();
			}

			if (num == CurrInstrNum())
				UpdateSample();
		}



		/********************************************************************/
		/// <summary>
		/// Set the current instrument and update it
		/// </summary>
		/********************************************************************/
		public void SetInstrNum(InstNum iNum)
		{
			inst[currInstr = iNum].Update();
		}



		/********************************************************************/
		/// <summary>
		/// Set the annotation text
		/// </summary>
		/********************************************************************/
		public void SetAnnoText(byte[] text)
		{
			annoText = EncoderCollection.Amiga.GetString(text);
		}



		/********************************************************************/
		/// <summary>
		/// Update the sample information
		/// </summary>
		/********************************************************************/
		public void UpdateSample()
		{
			SetInstrNum(currInstr);
		}



		/********************************************************************/
		/// <summary>
		/// Read in synth sound information from the file
		/// </summary>
		/********************************************************************/
		public bool ReadSynthSound(InstNum iNum, ModuleStream moduleStream, bool isHybrid, out string errorString)
		{
			errorString = string.Empty;

			// Remember the position in the file
			long startOffs = moduleStream.Position - 6;

			// Allocate the synth sound object
			SynthSound sy = new SynthSound(worker);

			// Read in the synth structure from the file
			MmdSynthSound synHdr = new MmdSynthSound();

			synHdr.Decay = moduleStream.Read_UINT8();
			moduleStream.Seek(3, SeekOrigin.Current);
			synHdr.Rpt = moduleStream.Read_B_UINT16();
			synHdr.RptLen = moduleStream.Read_B_UINT16();
			synHdr.VolTblLen = moduleStream.Read_B_UINT16();
			synHdr.WfTblLen = moduleStream.Read_B_UINT16();
			synHdr.VolSpeed = moduleStream.Read_UINT8();
			synHdr.WfSpeed = moduleStream.Read_UINT8();
			synHdr.NumWfs = moduleStream.Read_B_UINT16();

			if ((synHdr.VolTblLen > 128) || (synHdr.WfTblLen > 128) || (synHdr.NumWfs > 256))
				return false;

			// The easiest things first...
			sy.SetVolSpeed(synHdr.VolSpeed);
			sy.SetWfSpeed(synHdr.WfSpeed);

			uint cnt2;
			for (cnt2 = 0; cnt2 < synHdr.VolTblLen; )
			{
				byte data = moduleStream.Read_UINT8();
				sy.SetVolData(cnt2++, data);

				if (data == SynthSound.CmdEnd)
				{
					sy.SetVolTableLen(cnt2);
					break;
				}
			}

			moduleStream.Seek(synHdr.VolTblLen - cnt2, SeekOrigin.Current);

			for (cnt2 = 0; cnt2 < synHdr.WfTblLen; )
			{
				byte data = moduleStream.Read_UINT8();
				sy.SetWfData(cnt2++, data);

				if (data == SynthSound.CmdEnd)
				{
					sy.SetWfTableLen(cnt2);
					break;
				}
			}

			moduleStream.Seek(synHdr.WfTblLen - cnt2, SeekOrigin.Current);

			uint[] wfPtr = new uint[synHdr.NumWfs];
			moduleStream.ReadArray_B_UINT32s(wfPtr, 0, synHdr.NumWfs);

			for (cnt2 = 0; cnt2 < synHdr.NumWfs; cnt2++)
			{
				moduleStream.Seek(startOffs + wfPtr[cnt2], SeekOrigin.Begin);

				SynthWf wf = new SynthWf();
				sy.Add(wf);

				// As the first wave, read the sample if hybrid
				if ((cnt2 == 0) && isHybrid)
				{
					MmdSampleHdr hybHdr = new MmdSampleHdr(worker, moduleStream, iNum, out errorString);
					if (!string.IsNullOrEmpty(errorString))
						return false;

					if (hybHdr.IsSample())
						hybHdr.ReadSampleData(moduleStream, sy);
				}
				else
				{
					wf.SyWfLength = moduleStream.Read_B_UINT16();
					moduleStream.ReadSigned(wf.SyWfData, 0, (int)wf.SyWfLength * 2);
				}
			}

			SetSample(iNum, sy);

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Will set the pointer to the next sub-song
		/// </summary>
		/********************************************************************/
		public static Song operator ++(Song song)
		{
			song.SetSSNum((int)song.currNum + 1);
			return song;
		}
	}
}
