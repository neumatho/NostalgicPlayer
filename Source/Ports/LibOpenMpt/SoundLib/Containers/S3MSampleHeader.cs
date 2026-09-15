/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
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
	/// S3M Sample Header
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 80)]
	internal struct S3MSampleHeader : IClearable
	{
		#region SampleType
		public const uint8 TypeNone = 0;
		public const uint8 TypePcm = 1;
		public const uint8 TypeAdMel = 2;
		#endregion

		#region SampleFlags
		public const uint8 SmpLoop = 0x01;
		public const uint8 SmpStereo = 0x02;
		public const uint8 Smp16Bit = 0x04;
		#endregion

		#region SamplePacking
		public const uint8 PUnpacked = 0x00;
		public const uint8 PDp30Adpcm = 0x01;
		public const uint8 PAdpcm = 0x04;
		#endregion

		/// <summary>
		/// Sample type, see SampleType
		/// </summary>
		public uint8le SampleType;

		/// <summary>
		/// Sample filename
		/// </summary>
		public String12 FileName;

		/// <summary>
		/// Pointer to sample data (divided by 16)
		/// </summary>
		public Array3 DataPointer;

		/// <summary>
		/// Sample length, in samples
		/// </summary>
		public uint32le Length;

		/// <summary>
		/// Loop start, in samples
		/// </summary>
		public uint32le LoopStart;

		/// <summary>
		/// Loop end, in samples
		/// </summary>
		public uint32le LoopEnd;

		/// <summary>
		/// Default volume (0...64)
		/// </summary>
		public uint8le DefaultVolume;

		/// <summary>
		/// Reserved
		/// </summary>
		public byte Reserved1;

		/// <summary>
		/// Packing algorithm, SamplePacking
		/// </summary>
		public uint8le Pack;

		/// <summary>
		/// Sample flags
		/// </summary>
		public uint8le Flags;

		/// <summary>
		/// Middle-C frequency
		/// </summary>
		public uint32le C5Speed;

		/// <summary>
		/// Reserved
		/// </summary>
		public Array4 Reserved2;

		/// <summary>
		/// Sample address in GUS memory (used for fingerprinting)
		/// </summary>
		public uint16le GusAddress;

		/// <summary>
		/// SoundBlaster loop expansion stuff
		/// </summary>
		public uint16le Sb512;

		/// <summary>
		/// More SoundBlaster stuff
		/// </summary>
		public uint32le LastUsedPos;

		/// <summary>
		/// Sample name
		/// </summary>
		public String28 Name;

		/// <summary>
		/// "SCRS" magic bytes ("SCRI" for Adlib instruments)
		/// </summary>
		public Array4 Magic;

		/********************************************************************/
		/// <summary>
		/// Convert an S3M sample header to OpenMPT's internal sample header
		/// </summary>
		/********************************************************************/
		public void ConvertToMpt(ModSample mptSmp, bool isSt3)
		{
			mptSmp.Initialize(ModType.S3M);
			mptSmp.FileName.Assign(MptString.ReadBuf(ReadWriteMode.MaybeNullTerminated, FileName.ToArray()));

			if ((SampleType == TypePcm) || (SampleType == TypeNone))
			{
				// Sample Length and Loops
				if (SampleType == TypePcm)
				{
					mptSmp.nLength = Length;
					mptSmp.nLoopStart = Math.Min(LoopStart, mptSmp.nLength - 1);
					mptSmp.nLoopEnd = Math.Min(LoopEnd, mptSmp.nLength);
					mptSmp.uFlags.Set(ChannelFlags.Chn_Loop, (Flags & SmpLoop) != 0);
				}

				if ((mptSmp.nLoopEnd < 2) || (mptSmp.nLoopStart >= mptSmp.nLoopEnd) || ((mptSmp.nLoopEnd - mptSmp.nLoopStart) < 1))
				{
					mptSmp.nLoopStart = mptSmp.nLoopEnd = 0;
					mptSmp.uFlags.Reset();
				}
			}
			else if (SampleType == TypeAdMel)
			{
				//XX OPL
			}

			// Volume / Panning
			mptSmp.nVolume = (uint16)(Math.Min(DefaultVolume, (uint8)64) * 4);

			// C-5 frequency
			mptSmp.nC5Speed = C5Speed;

			if (isSt3)
			{
				// ST3 ignores or clamps the high 16 bits depending on the instrument type
				if (SampleType == TypeAdMel)
					mptSmp.nC5Speed &= 0xffff;
				else
					OpenMpt.LimitMax(ref mptSmp.nC5Speed, (uint32)uint16.MaxValue);
			}

			if (mptSmp.nC5Speed == 0)
				mptSmp.nC5Speed = 8363;
			else if (mptSmp.nC5Speed < 1024)
				mptSmp.nC5Speed = 1024;
		}



		/********************************************************************/
		/// <summary>
		/// Retrieve the internal sample format flags for this sample
		/// </summary>
		/********************************************************************/
		public SampleIO GetSampleFormat(bool signedSamples)
		{
			if ((Pack == S3MSampleHeader.PAdpcm) && ((Flags & S3MSampleHeader.Smp16Bit) == 0) && ((Flags & S3MSampleHeader.SmpStereo) == 0))
			{
				// MODPlugin :(
				return new SampleIO(SampleIO.BitDepth._8Bit, SampleIO.Channels.Mono, SampleIO.Endianness.LittleEndian, SampleIO.Encoding.Adpcm);
			}
			else
				return new SampleIO((Flags & S3MSampleHeader.Smp16Bit) != 0 ? SampleIO.BitDepth._16Bit : SampleIO.BitDepth._8Bit, (Flags & S3MSampleHeader.SmpStereo) != 0 ? SampleIO.Channels.StereoSplit : SampleIO.Channels.Mono, SampleIO.Endianness.LittleEndian, signedSamples ? SampleIO.Encoding.SignedPcm : SampleIO.Encoding.UnsignedPcm);
		}



		/********************************************************************/
		/// <summary>
		/// Calculate the sample position in file
		/// </summary>
		/********************************************************************/
		public uint32 GetSampleOffset()
		{
			return (uint32)((DataPointer[1] << 4) | (DataPointer[2] << 12) | (DataPointer[0] << 20));
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
