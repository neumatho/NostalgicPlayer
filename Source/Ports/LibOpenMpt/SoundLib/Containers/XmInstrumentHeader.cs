/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.C.Std;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.String;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Endian;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers.Strings;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// XM Instrument Header
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 263)]
	internal struct XmInstrumentHeader
	{
		/// <summary>
		/// Size of XMInstrumentHeader + XMInstrument
		/// </summary>
		public uint32le Size;

		/// <summary>
		/// Instrument Name, space-padded
		/// </summary>
		public String22 Name;

		/// <summary>
		/// Instrument Type (FT2 does not initialize this field properly, so it contains a random value, but it's the same random value for all instruments of the same module!)
		/// </summary>
		public uint8le Type;

		/// <summary>
		/// Number of Samples associated with instrument
		/// </summary>
		public uint16le NumSamples;

		/// <summary>
		/// Size of XMSample
		/// </summary>
		public uint32le SampleHeaderSize;

		/// <summary>
		/// 
		/// </summary>
		public XmInstrument Instrument;

		/********************************************************************/
		/// <summary>
		/// Convert an XMInstrumentHeader to OpenMPT's internal instrument
		/// representation
		/// </summary>
		/********************************************************************/
		public void ConvertToMpt(ModInstrument mptIns)
		{
			Instrument.ConvertToMpt(mptIns);

			// Create sample assignment table
			array<uint8> sampleMap = Instrument.SampleMap.ToArray();

			for (size_t i = 0; i < sampleMap.size(); i++)
			{
				if (sampleMap[i] < NumSamples)
					mptIns.Keyboard[i + 12] = sampleMap[i];
				else
					mptIns.Keyboard[i + 12] = 0;
			}

			mptIns.Name.Assign(MptString.ReadBuf(ReadWriteMode.SpacePadded, Name.ToArray()));

			// Old MPT backwards compatibility
			if (Instrument.MidiEnabled == 0)
				mptIns.nMidiProgram = Type;
		}
	}
}
