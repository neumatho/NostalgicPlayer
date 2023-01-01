/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit;

namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation
{
	/// <summary>
	/// Instrument interface
	/// </summary>
	internal class Instr
	{
		[Flags]
		public enum Flag
		{
			Loop = 0x01,
			MidiExtPSet = 0x02,
			Disabled = 0x04,
			PingPong = 0x08,
			MidiSuppressOff = 0x10
		}

		private OctaMedWorker worker;

		public Flag flags;

		private string name;
		private readonly LimVar<uint> initVol = new(0, 128);	// Start volume
		private readonly LimVar<uint> vol = new(0, 128);		// Volume
		private uint repStart;									// Repeat start
		private uint repLength;									// Repeat length
		private readonly LimVar<short> sTrans = new(-128, 127);	// Sample transpose
		private readonly LimVar<short> fineTune = new(-8, 7);	// Fine tuning value
		private readonly LimVar<ushort> hold = new(0, 128);		// Hold value
		private readonly LimVar<ushort> decay = new(0, 128);	// Decay value
		private uint midiCh;									// MIDI channel
		private InstNum num;									// Ordinal number of this instrument
		private uint defFreq;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Instr()
		{
			Reset();
		}



		/********************************************************************/
		/// <summary>
		/// Initialize the instruments
		/// </summary>
		/********************************************************************/
		public void Init()
		{
			vol.Value = initVol.Value;
		}



		/********************************************************************/
		/// <summary>
		/// Return the sample that corresponds with this instrument. Returns
		/// null if no sample in slot, or this is not a sample instrument
		/// </summary>
		/********************************************************************/
		public Sample GetSample()
		{
			if (!IsMidi())
				return worker.sg.GetSample(num);

			return null;
		}



		/********************************************************************/
		/// <summary>
		/// Sets the instrument number
		/// </summary>
		/********************************************************************/
		public void SetNum(OctaMedWorker worker, InstNum num)
		{
			this.worker = worker;
			this.num = num;
		}



		/********************************************************************/
		/// <summary>
		/// Set the instrument name
		/// </summary>
		/********************************************************************/
		public void SetName(byte[] newName)
		{
			name = EncoderCollection.Amiga.GetString(newName);
		}



		/********************************************************************/
		/// <summary>
		/// Set init volume
		/// </summary>
		/********************************************************************/
		public void SetInitVol(uint vol)
		{
			initVol.Value = vol;
		}



		/********************************************************************/
		/// <summary>
		/// Set the volume
		/// </summary>
		/********************************************************************/
		public void SetVol(uint newVol)
		{
			vol.Value = newVol;
		}



		/********************************************************************/
		/// <summary>
		/// Set the start loop
		/// </summary>
		/********************************************************************/
		public void SetRepeat(uint newRep, bool keepEnd = false)
		{
			if (keepEnd)
			{
				uint repEnd = repStart + repLength;	// The 1st sample following loop
				repStart = newRep;

				if (repStart >= (repEnd - 1))
					KillLoop();

				repLength = repEnd - repStart;
			}
			else
				repStart = newRep;

			ValidateLoop();
		}



		/********************************************************************/
		/// <summary>
		/// Set the loop length
		/// </summary>
		/********************************************************************/
		public void SetRepeatLen(uint newLen)
		{
			repLength = newLen;
			ValidateLoop();
		}



		/********************************************************************/
		/// <summary>
		/// Set the sample transpose value
		/// </summary>
		/********************************************************************/
		public void SetTransp(short iTransp)
		{
			sTrans.Value = iTransp;
		}



		/********************************************************************/
		/// <summary>
		/// Change the fine tune
		/// </summary>
		/********************************************************************/
		public void SetFineTune(short newFT)
		{
			fineTune.Value = newFT;
		}



		/********************************************************************/
		/// <summary>
		/// Set the hold value
		/// </summary>
		/********************************************************************/
		public void SetHold(ushort hold)
		{
			this.hold.Value = hold;
		}



		/********************************************************************/
		/// <summary>
		/// Set the decay value
		/// </summary>
		/********************************************************************/
		public void SetDecay(ushort decay)
		{
			this.decay.Value = decay;
		}



		/********************************************************************/
		/// <summary>
		/// Set the default pitch value
		/// </summary>
		/********************************************************************/
		public void SetDefPitch(NoteNum newPitch)
		{
			if (newPitch == 0)
				SetDefFreq(0);
			else
				SetDefFreq(worker.plr.GetNoteFrequency((NoteNum)(newPitch - 1), GetFineTune()));
		}



		/********************************************************************/
		/// <summary>
		/// Set the default frequency value
		/// </summary>
		/********************************************************************/
		public void SetDefFreq(uint newFreq)
		{
			defFreq = newFreq;
		}



		/********************************************************************/
		/// <summary>
		/// Set the midi channel
		/// </summary>
		/********************************************************************/
		public void SetMidiCh(uint midiCh)
		{
			this.midiCh = midiCh;

			ValidateLoop();
		}



		/********************************************************************/
		/// <summary>
		/// Return the name
		/// </summary>
		/********************************************************************/
		public string GetName()
		{
			return name ?? string.Empty;
		}



		/********************************************************************/
		/// <summary>
		/// Return the init volume
		/// </summary>
		/********************************************************************/
		public uint GetInitVol()
		{
			return initVol.Value;
		}



		/********************************************************************/
		/// <summary>
		/// Return the volume
		/// </summary>
		/********************************************************************/
		public uint GetVol()
		{
			return vol.Value;
		}



		/********************************************************************/
		/// <summary>
		/// Return the hold value
		/// </summary>
		/********************************************************************/
		public ushort GetHold()
		{
			return hold.Value;
		}



		/********************************************************************/
		/// <summary>
		/// Return the decay value
		/// </summary>
		/********************************************************************/
		public ushort GetDecay()
		{
			return decay.Value;
		}



		/********************************************************************/
		/// <summary>
		/// Return a valid default frequency for the instrument
		/// </summary>
		/********************************************************************/
		public uint GetValidDefFreq()
		{
			return defFreq != 0 ? defFreq : 22050;
		}



		/********************************************************************/
		/// <summary>
		/// Return the repeat start position
		/// </summary>
		/********************************************************************/
		public uint GetRepeat()
		{
			return repStart;
		}



		/********************************************************************/
		/// <summary>
		/// Return the repeat length
		/// </summary>
		/********************************************************************/
		public uint GetRepeatLen()
		{
			return repLength;
		}



		/********************************************************************/
		/// <summary>
		/// Return the sample transpose value
		/// </summary>
		/********************************************************************/
		public short GetTransp()
		{
			return sTrans.Value;
		}



		/********************************************************************/
		/// <summary>
		/// Return the fine tune
		/// </summary>
		/********************************************************************/
		public short GetFineTune()
		{
			return fineTune.Value;
		}



		/********************************************************************/
		/// <summary>
		/// Checks to see if the instrument is a midi instrument
		/// </summary>
		/********************************************************************/
		public bool IsMidi()
		{
			return midiCh != 0;
		}



		/********************************************************************/
		/// <summary>
		/// Validates the loop information
		/// </summary>
		/********************************************************************/
		public void ValidateLoop()
		{
			Sample sample = GetSample();

			if (sample != null)
			{
				uint length = sample.GetLength();

				if (length == 0)
					KillLoop();
				else
				{
					if (repStart >= length)
						repStart = length - 1;

					if ((repStart + repLength) > length)
						repLength = length - repStart;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Update()
		{
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Initialize all the member variables
		/// </summary>
		/********************************************************************/
		private void Reset()
		{
			vol.Value = 0;
			sTrans.Value = 0;
			hold.Value = 0;
			decay.Value = 0;
			fineTune.Value = 0;

			repStart = 0;
			repLength = 0;
			flags = 0;
			midiCh = 0;
			defFreq = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Sets the instrument to have no loop
		/// </summary>
		/********************************************************************/
		private void KillLoop()
		{
			repStart = 0;
			repLength = 0;
			flags &= Flag.Loop;
		}
		#endregion
	}
}
