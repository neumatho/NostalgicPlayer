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
using Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers.Strings;

namespace Polycode.NostalgicPlayer.Ports.LibOpenMpt.SoundLib.Containers
{
	/// <summary>
	/// XM Sample Header
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 40)]
	internal struct XmSample : IClearable
	{
		#region XmSampleFlags
		public const uint8 SampleLoop = 0x01;
		public const uint8 SampleBidiLoop = 0x02;
		public const uint8 Sample16Bit = 0x10;
		public const uint8 SampleStereo = 0x20;
		public const uint8 SampleAdpcm = 0xad;		// MODPlugin :(
		#endregion

		/// <summary>
		/// Sample Length (in bytes)
		/// </summary>
		public uint32le Length;

		/// <summary>
		/// Loop Start (in bytes)
		/// </summary>
		public uint32le LoopStart;

		/// <summary>
		/// Loop Length (in bytes)
		/// </summary>
		public uint32le LoopLength;

		/// <summary>
		/// Default Volume
		/// </summary>
		public uint8le Vol;

		/// <summary>
		/// Sample Finetune
		/// </summary>
		public int8le FineTune;

		/// <summary>
		/// Sample Flags
		/// </summary>
		public uint8le Flags;

		/// <summary>
		/// Sample Panning
		/// </summary>
		public uint8le Pan;

		/// <summary>
		/// Sample Transpose
		/// </summary>
		public int8le RelNote;

		/// <summary>
		/// Reserved (abused for ModPlug's ADPCM compression)
		/// </summary>
		public uint8le Reserved;

		/// <summary>
		/// Sample Name, space-padded
		/// </summary>
		public String22 Name;

		/********************************************************************/
		/// <summary>
		/// Convert an XMSample to OpenMPT's internal sample representation
		/// </summary>
		/********************************************************************/
		public void ConvertToMpt(ModSample mptSmp)
		{
			mptSmp.Initialize(ModType.Xm);

			// Volume
			mptSmp.nVolume = (uint16)(Vol * 4);
			OpenMpt.LimitMax(ref mptSmp.nVolume, (uint16)256);

			// Panning
			mptSmp.nPan = Pan;
			mptSmp.uFlags = ChannelFlags.Chn_Panning;

			// Sample Frequency
			mptSmp.nFineTune = FineTune;
			mptSmp.RelativeTone = RelNote;

			// Sample Length and Loops
			mptSmp.nLength = Length;
			mptSmp.nLoopStart = LoopStart;
			mptSmp.nLoopEnd = mptSmp.nLoopStart + LoopLength;

			if ((Flags & Sample16Bit) != 0)
			{
				mptSmp.nLength /= 2;
				mptSmp.nLoopStart /= 2;
				mptSmp.nLoopEnd /= 2;
			}

			if ((Flags & SampleStereo) != 0)
			{
				mptSmp.nLength /= 2;
				mptSmp.nLoopStart /= 2;
				mptSmp.nLoopEnd /= 2;
			}

			if (((Flags & (SampleLoop | SampleBidiLoop)) != 0) && (mptSmp.nLoopEnd > mptSmp.nLoopStart))
			{
				mptSmp.uFlags.Set(ChannelFlags.Chn_Loop);

				if ((Flags & SampleBidiLoop) != 0)
					mptSmp.uFlags.Set(ChannelFlags.Chn_PingPongLoop);
			}

			mptSmp.FileName.Assign(string.Empty);
		}



		/********************************************************************/
		/// <summary>
		/// Retrieve the internal sample format flags for this instrument
		/// </summary>
		/********************************************************************/
		public SampleIO GetSampleFormat()
		{
			if ((Reserved == SampleAdpcm) && ((Flags & (Sample16Bit | SampleStereo)) == 0))
			{
				// MODPlugin :(
				return new SampleIO(SampleIO.BitDepth._8Bit, SampleIO.Channels.Mono, SampleIO.Endianness.LittleEndian, SampleIO.Encoding.Adpcm);
			}

			return new SampleIO(
				(Flags & Sample16Bit) != 0 ? SampleIO.BitDepth._16Bit : SampleIO.BitDepth._8Bit,
				(Flags & SampleStereo) != 0 ? SampleIO.Channels.StereoSplit : SampleIO.Channels.Mono,
				SampleIO.Endianness.LittleEndian, SampleIO.Encoding.DeltaPcm);
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
