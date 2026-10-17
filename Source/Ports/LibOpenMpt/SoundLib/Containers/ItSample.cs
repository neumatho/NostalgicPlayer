/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Utility.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Common.String;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Base;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.Mpt.Endian;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers.Arrays;
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers.Strings;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// IT Sample Format
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 80)]
	internal struct ItSample : IClearable
	{
		#region ItSampleFlags
		public const uint8 SampleDataPresent = 0x01;
		public const uint8 Sample16Bit = 0x02;
		public const uint8 SampleStereo = 0x04;
		public const uint8 SampleCompressed = 0x08;
		public const uint8 SampleLoop = 0x10;
		public const uint8 SampleSustain = 0x20;
		public const uint8 SampleBidiLoop = 0x40;
		public const uint8 SampleBidiSustain = 0x80;

		public const uint8 EnablePanning = 0x80;

		public const uint8 CvtSignedSample = 0x01;
		public const uint8 CvtOplInstrument = 0x40;		// FM instrument in MPTM
		public const uint8 CvtExternalSample = 0x80;	// Keep MPTM sample on disk
		public const uint8 CvtAdpcmSample = 0xff;		// MODPlugin :(

		// ITTECH.TXT says these convert flags are "safe to ignore". IT doesn't ignore them, though, so why should we? :)
		public const uint8 CvtBigEndian = 0x02;
		public const uint8 CvtDelta = 0x04;
		public const uint8 CvtPtm8To16 = 0x08;
		#endregion

		/// <summary>
		/// Magic Bytes (IMPS)
		/// </summary>
		public Array4 Id;

		/// <summary>
		/// DOS Filename, null-terminated
		/// </summary>
		public String13 FileName;

		/// <summary>
		/// Global Volume
		/// </summary>
		public uint8le Gvl;

		/// <summary>
		/// Sample Flags
		/// </summary>
		public uint8le Flags;

		/// <summary>
		/// Default Volume
		/// </summary>
		public uint8le Vol;

		/// <summary>
		/// Sample Name, null-terminated (but may also contain nulls)
		/// </summary>
		public String26 Name;

		/// <summary>
		/// Sample Import Format
		/// </summary>
		public uint8le Cvt;

		/// <summary>
		/// Sample Panning
		/// </summary>
		public uint8le Dfp;

		/// <summary>
		/// Sample Length (in samples)
		/// </summary>
		public uint32le Length;

		/// <summary>
		/// Sample Loop Begin (in samples)
		/// </summary>
		public uint32le LoopBegin;

		/// <summary>
		/// Sample Loop End (in samples)
		/// </summary>
		public uint32le LoopEnd;

		/// <summary>
		/// C-5 frequency
		/// </summary>
		public uint32le C5Speed;

		/// <summary>
		/// Sample Sustain Begin (in samples)
		/// </summary>
		public uint32le SusLoopBegin;

		/// <summary>
		/// Sample Sustain End (in samples)
		/// </summary>
		public uint32le SusLoopEnd;

		/// <summary>
		/// Pointer to sample data
		/// </summary>
		public uint32le SamplePointer;

		/// <summary>
		/// Auto-Vibrato Rate (called Sweep in IT)
		/// </summary>
		public uint8le Vis;

		/// <summary>
		/// Auto-Vibrato Depth
		/// </summary>
		public uint8le Vid;

		/// <summary>
		/// Auto-Vibrato Sweep (called Rate in IT)
		/// </summary>
		public uint8le Vir;

		/// <summary>
		/// Auto-Vibrato Type
		/// </summary>
		public uint8le Vit;

		/********************************************************************/
		/// <summary>
		/// Convert an ItSample to OpenMPT's internal sample representation
		/// </summary>
		/********************************************************************/
		public uint32 ConvertToMpt(ModSample mptSmp)
		{
			mptSmp.Initialize(ModType.It);
			mptSmp.SetDefaultCuePoints();	// For old IT/MPTM files
			mptSmp.FileName.Assign(MptString.ReadBuf(ReadWriteMode.NullTerminated, FileName.ToArray()));

			// Volume / Panning
			mptSmp.nVolume = (uint16)(Vol * 4);
			OpenMpt.LimitMax(ref mptSmp.nVolume, (uint16)256);

			mptSmp.nGlobalVol = Gvl;
			OpenMpt.LimitMax(ref mptSmp.nGlobalVol, (uint16)64);

			mptSmp.nPan = (uint16)((Dfp & 0x7f) * 4);
			OpenMpt.LimitMax(ref mptSmp.nPan, (uint16)256);

			if ((Dfp & EnablePanning) != 0)
				mptSmp.uFlags.Set(ChannelFlags.Chn_Panning);

			// Loop Flags
			if ((Flags & SampleLoop) != 0)
				mptSmp.uFlags.Set(ChannelFlags.Chn_Loop);

			if ((Flags & SampleSustain) != 0)
				mptSmp.uFlags.Set(ChannelFlags.Chn_SustainLoop);

			if ((Flags & SampleBidiLoop) != 0)
				mptSmp.uFlags.Set(ChannelFlags.Chn_PingPongLoop);

			if ((Flags & SampleBidiSustain) != 0)
				mptSmp.uFlags.Set(ChannelFlags.Chn_PingPongSustain);

			// Frequency
			mptSmp.nC5Speed = C5Speed;

			if (mptSmp.nC5Speed == 0)
				mptSmp.nC5Speed = 8363;

			if (mptSmp.nC5Speed < 256)
				mptSmp.nC5Speed = 256;

			// Size and loops
			mptSmp.nLength = Length;
			mptSmp.nLoopStart = LoopBegin;
			mptSmp.nLoopEnd = LoopEnd;
			mptSmp.nSustainStart = SusLoopBegin;
			mptSmp.nSustainEnd = SusLoopEnd;
			mptSmp.SanitizeLoops();

			// Auto Vibrato settings
			mptSmp.nVibType = Tables.AutoVibratoIt2Xm[Vit & 7];
			mptSmp.nVibRate = Vis;
			mptSmp.nVibDepth = (uint8)(Vid & 0x7f);
			mptSmp.nVibSweep = Vir;

			if (Cvt == CvtOplInstrument)
			{
				// FM instrument in MPTM
				mptSmp.uFlags.Set(ChannelFlags.Chn_Adlib);
			}
			else if (Cvt == CvtExternalSample)
			{
				// Read external sample (filename at sample pointer)
				mptSmp.uFlags.Set(ChannelFlags.Smp_KeepOnDisk);
			}

			return SamplePointer;
		}



		/********************************************************************/
		/// <summary>
		/// Retrieve the internal sample format flags for this instrument
		/// </summary>
		/********************************************************************/
		public SampleIO GetSampleFormat(uint16 cwtv)
		{
			SampleIO sampleIO = new SampleIO(
				(Flags & Sample16Bit) != 0 ? SampleIO.BitDepth._16Bit : SampleIO.BitDepth._8Bit,
				SampleIO.Channels.Mono,
				SampleIO.Endianness.LittleEndian,
				(Cvt & CvtSignedSample) != 0 ? SampleIO.Encoding.SignedPcm : SampleIO.Encoding.UnsignedPcm);

			// Some old version of IT didn't clear the stereo flag when importing samples. Luckily, all other trackers are identifying as IT 2.14+, so let's check for old IT versions
			if (((Flags & SampleStereo) != 0) && (cwtv >= 0x214))
				sampleIO |= SampleIO.Channels.StereoSplit;

			if ((Flags & SampleCompressed) != 0)
			{
				// IT 2.14 packed sample
				sampleIO |= ((Cvt & CvtDelta) != 0) ? SampleIO.Encoding.It215 : SampleIO.Encoding.It214;
			}
			else
			{
				// MODPlugin :(
				if (((Flags & Sample16Bit) == 0) && (Cvt == CvtAdpcmSample))
					sampleIO |= SampleIO.Encoding.Adpcm;
				else
				{
					// ITTECH.TXT says these convert flags are "safe to ignore". IT doesn't ignore them, though, so why should we? :)
					if ((Cvt & CvtBigEndian) != 0)
						sampleIO |= SampleIO.Endianness.BigEndian;

					if ((Cvt & CvtDelta) != 0)
						sampleIO |= SampleIO.Encoding.DeltaPcm;

					if (((Cvt & CvtPtm8To16) != 0) && ((Flags & Sample16Bit) != 0))
						sampleIO |= SampleIO.Encoding.Ptm8Dto16;
				}
			}

			return sampleIO;
		}



		/********************************************************************/
		/// <summary>
		/// Clear all members
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			this = default;
		}
	}
}
