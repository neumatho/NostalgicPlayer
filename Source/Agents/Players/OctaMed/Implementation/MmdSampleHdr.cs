/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation
{
	/// <summary>
	/// MmdSampleHdr class
	///
	/// Sample header in MMD files:
	///
	/// uint32 length       Length of *one* *unpacked* channel in *bytes*
	/// uint16 type         See the below flag bits for definitions.
	///                     Bits 0-3 reserved for multi-octave instruments
	///
	/// If type and MMD_INSTR_PACK, these fields follow:
	///
	/// uint16 packType     See MMD_INSTRPACK_xx below
	/// uint16 subType      Packer subtype, see below
	/// uint8  commonFlags  Flags common to all pack types (none defined so far)
	/// uint8  packerFlags  Flags for the specific pack type
	/// uint32 leftChLen    Packed length of left channel in bytes
	/// uint32 rightChLen   Packed length of right channel in bytes. (ONLY IN
	///                     STEREO SAMPLES)
	/// + possible other packer-dependent fields
	/// </summary>
	internal class MmdSampleHdr
	{
		private const short Instr16Bit = 0x0010;
		private const short InstrStereo = 0x0020;
		private const short InstrDeltaCode = 0x0040;
		private const short InstrPack = 0x0080;

		private readonly OctaMedWorker worker;

		private readonly InstNum sampleNumber;

		private readonly uint numBytes;
		private readonly uint numFrames;
		private readonly bool sixtBit;
		private readonly bool stereo;
		private readonly short type;
		private readonly ushort packType;
		private readonly bool skipThis;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public MmdSampleHdr(OctaMedWorker worker, ModuleStream moduleStream, InstNum sampNum, out string errorMessage)
		{
			errorMessage = string.Empty;

			this.worker = worker;
			sampleNumber = sampNum;

			numBytes = moduleStream.Read_B_UINT32();
			numFrames = numBytes;
			skipThis = false;
			type = moduleStream.Read_B_INT16();

			if (type >= 0)
			{
				if ((type & 0x0f) > 7)
				{
					// Unknown sample
					errorMessage = Resources.IDS_MED_ERR_UNKNOWN_SAMPLE;
					return;
				}

				if ((type & Instr16Bit) != 0)
				{
					numFrames /= 2;
					sixtBit = true;
				}
				else
					sixtBit = false;

				if ((type & InstrStereo) != 0)
					stereo = true;
				else
					stereo = false;

				if ((type & InstrPack) != 0)
					errorMessage = Resources.IDS_MED_ERR_PACKED_SAMPLES;
				else
					packType = 0;
			}
			else
			{
				if (type < -2)
				{
					// Unknown instrument type
					errorMessage = Resources.IDS_MED_ERR_UNKNOWN_INSTRUMENT;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Allocate a sample object with the information in this header
		/// </summary>
		/********************************************************************/
		public Sample AllocSample()
		{
			return new Sample(worker);
		}



		/********************************************************************/
		/// <summary>
		/// Read the sample data into the sample object given
		/// </summary>
		/********************************************************************/
		public void ReadSampleData(ModuleStream moduleStream, Sample dest)
		{
			// Make sure that the dimension for receiving data are correct
			dest.SetProp(numFrames, stereo, sixtBit, type);

			// Unknown compression
			if (skipThis)
				return;

			switch (packType)
			{
				// No packing
				case 0:
				{
					using (ModuleStream sampleDataStream = moduleStream.GetSampleDataStream((int)sampleNumber, (int)numBytes))
					{
						for (ushort chCnt = 0; chCnt < (stereo ? 2 : 1); chCnt++)
						{
							for (int oct = 0; ; oct++)
							{
								sbyte[] buf = dest.GetSampleBuffer(chCnt, oct);
								if (buf == null)
									break;

								if (sixtBit)
								{
									Span<short> buf16 = MemoryMarshal.Cast<sbyte, short>(buf);

									if ((type & InstrDeltaCode) != 0)
									{
										short prev = sampleDataStream.Read_B_INT16();
										buf16[0] = prev;

										for (int cnt2 = 1; cnt2 < buf16.Length; cnt2++)
										{
											prev += sampleDataStream.Read_B_INT16();
											buf16[cnt2] = prev;
										}
									}
									else
									{
										for (int cnt2 = 0; cnt2 < buf16.Length; cnt2++)
											buf16[cnt2] = sampleDataStream.Read_B_INT16();
									}
								}
								else
								{
									if ((type & InstrDeltaCode) != 0)
									{
										sbyte prev = sampleDataStream.Read_INT8();
										buf[0] = prev;

										for (int cnt2 = 1; cnt2 < buf.Length; cnt2++)
										{
											prev += sampleDataStream.Read_INT8();
											buf[cnt2] = prev;
										}
									}
									else
									{
										for (int cnt2 = 0; cnt2 < buf.Length; cnt2++)
											buf[cnt2] = sampleDataStream.Read_INT8();
									}
								}
							}
						}
					}
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Check to see if the sample is a real sample
		/// </summary>
		/********************************************************************/
		public bool IsSample()
		{
			return type >= 0;
		}



		/********************************************************************/
		/// <summary>
		/// Check to see if the sample is a synth sample
		/// </summary>
		/********************************************************************/
		public bool IsSynth()
		{
			return type == -1;
		}



		/********************************************************************/
		/// <summary>
		/// Check to see if the sample is a hybrid sample
		/// </summary>
		/********************************************************************/
		public bool IsHybrid()
		{
			return type == -2;
		}



		/********************************************************************/
		/// <summary>
		/// Check to see if the sample is a stereo sample
		/// </summary>
		/********************************************************************/
		public bool IsStereo()
		{
			return stereo;
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of bytes the sample uses
		/// </summary>
		/********************************************************************/
		public uint GetNumBytes()
		{
			return numBytes;
		}
	}
}
