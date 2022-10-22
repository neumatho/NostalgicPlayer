/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ProWizardConverter
{
	/// <summary>
	/// Base class used by different Cryptoburners format to make some checks
	/// </summary>
	internal abstract class CryptoburnersFormatBase : ProWizardConverterWorker31SamplesBase
	{
		/********************************************************************/
		/// <summary>
		/// Do some common checks for Cryptoburners formats
		/// </summary>
		/********************************************************************/
		protected bool Check(ModuleStream moduleStream, int moduleStart, int checkStart, out uint samplesSize)
		{
			samplesSize = 0;

			// Check position table size
			moduleStream.Seek(checkStart, SeekOrigin.Begin);

			byte temp = moduleStream.Read_UINT8();
			if ((temp == 0) || (temp > 127))
				return false;

			// Check NTK byte
			temp = moduleStream.Read_UINT8();
			if (temp > 0x7f)
				return false;

			// Check first two bytes in the position table
			if ((moduleStream.Read_UINT8() > 63) || (moduleStream.Read_UINT8() > 63))
				return false;

			// Check mark if any
			moduleStream.Seek(checkStart + 0x17a, SeekOrigin.Begin);

			uint temp1 = moduleStream.Read_B_UINT32();
			if ((temp1 == 0x4d2e4b2e) || (temp1 == 0x4d264b21) || (temp1 == 0x464c5434) ||	// M.K. - M&K! - FLT4
			    (temp1 == 0x45584f34) || (temp1 == 0x554e4943) || (temp1 == 0x534e542e))	// EXO4 - UNIC - SNT.
			{
				return false;
			}

			// Check sample information
			moduleStream.Seek(moduleStart, SeekOrigin.Begin);

			for (int i = 0; i < 31; i++)
			{
				// Get sample size
				ushort temp2 = moduleStream.Read_B_UINT16();
				samplesSize += temp2 * 2U;

				// Check volume and fine tune
				if ((moduleStream.Read_UINT8() > 0x0f) || (moduleStream.Read_UINT8() > 0x40))
					return false;

				if (temp2 != 0)
				{
					// Check loop and loop length
					uint temp3 = moduleStream.Read_B_UINT16();
					uint temp4 = moduleStream.Read_B_UINT16();

					if ((temp3 + temp4) > temp2)
						return false;
				}
				else
					moduleStream.Seek(4, SeekOrigin.Current);
			}

			return true;
		}
	}
}
