/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Kit.Utility
{
	/// <summary>
	/// Helper class to load IFF samples
	/// </summary>
	public static class IffSampleLoader
	{
		/********************************************************************/
		/// <summary>
		/// Load an IFF sample and return the data
		/// </summary>
		/********************************************************************/
		public static LoadResult Load(ModuleStream moduleStream, int sampleNumber, out IffSample iffSample)
		{
			iffSample = null;

			if (moduleStream.Read_B_UINT32() != 0x464f524d)		// FORM
			{
				// Seek back again
				moduleStream.Seek(-4, SeekOrigin.Current);

				return LoadResult.UnknownFormat;
			}

			uint originalFormLength = moduleStream.Read_B_UINT32();

			if (moduleStream.Read_B_UINT32() != 0x38535658)		// 8SVX
			{
				// Seek back again
				moduleStream.Seek(-8, SeekOrigin.Current);

				return LoadResult.UnknownFormat;
			}

			uint formLength = originalFormLength - 4;

			IffSample info = new IffSample();

			while (formLength > 0)
			{
				uint chunkName = moduleStream.Read_B_UINT32();
				uint chunkLength = moduleStream.Read_B_UINT32();
				formLength -= 8;

				if ((chunkLength % 2) != 0)
					chunkLength++;

				switch (chunkName)
				{
					// VHDR
					case 0x56484452:
					{
						info.OneShotHiSamples = moduleStream.Read_B_UINT32();
						info.RepeatHiSamples = moduleStream.Read_B_UINT32();
						info.SamplesPerHiCycle = moduleStream.Read_B_UINT32();
						info.SamplesPerSec = moduleStream.Read_B_UINT16();
						info.Octaves = moduleStream.Read_UINT8();

						byte compressed = moduleStream.Read_UINT8();
						if (compressed != 0)
							return LoadResult.Error;

						info.Volume = moduleStream.Read_B_UINT32();

						formLength -= 20;
						chunkLength -= 20;
						break;
					}

					// BODY
					case 0x424f4459:
					{
						if ((info.SampleData != null) || (info.Octaves == 0))
							return LoadResult.Error;

						// Some samples store the body length in words. Check for this
						if (((chunkLength * 2) + moduleStream.Position - 8) == originalFormLength)
							chunkLength *= 2;

						info.SampleData = moduleStream.ReadSampleData(sampleNumber, (int)chunkLength, out int readBytes);
						if (readBytes != chunkLength)
							return LoadResult.Error;

						formLength -= chunkLength;
						chunkLength = 0;
						break;
					}
				}

				if (chunkLength > 0)
				{
					moduleStream.Seek(chunkLength, SeekOrigin.Current);
					formLength -= chunkLength;
				}
			}

			if (info.SampleData == null)
				return LoadResult.Error;

			// Some samples have 0 in both one-shot and repeat fields. Fix them
			if ((info.OneShotHiSamples == 0) && (info.RepeatHiSamples == 0))
				info.OneShotHiSamples = (uint)info.SampleData.Length;

			iffSample = info;

			return LoadResult.Ok;
		}
	}
}
