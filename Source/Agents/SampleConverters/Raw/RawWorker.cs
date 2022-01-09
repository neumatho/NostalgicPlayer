/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Raw
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class RawWorker : ISampleSaverAgent
	{
		private SaveSampleFormatInfo format;

		private byte[] saveBuffer;

		#region ISampleSaverAgent implementation
		/********************************************************************/
		/// <summary>
		/// Return some flags telling what the saver supports
		/// </summary>
		/********************************************************************/
		public SampleSaverSupportFlag SaverSupportFlags => SampleSaverSupportFlag.Support8Bit | SampleSaverSupportFlag.Support16Bit | SampleSaverSupportFlag.Support32Bit | SampleSaverSupportFlag.SupportMono | SampleSaverSupportFlag.SupportStereo;



		/********************************************************************/
		/// <summary>
		/// Return the file extension that is used by the saver
		/// </summary>
		/********************************************************************/
		public string FileExtension => "raw";



		/********************************************************************/
		/// <summary>
		/// Initialize the saver so it is prepared to save the sample
		/// </summary>
		/********************************************************************/
		public bool InitSaver(SaveSampleFormatInfo formatInfo, out string errorMessage)
		{
			errorMessage = string.Empty;

			// Remember the format
			format = formatInfo;

			// Initialize buffer
			saveBuffer = null;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the saver
		/// </summary>
		/********************************************************************/
		public void CleanupSaver()
		{
			saveBuffer = null;
			format = null;
		}



		/********************************************************************/
		/// <summary>
		/// Save the header of the sample
		/// </summary>
		/********************************************************************/
		public void SaveHeader(Stream stream)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Save a part of the sample
		/// </summary>
		/********************************************************************/
		public void SaveData(Stream stream, int[] buffer, int length)
		{
			if (length > 0)
			{
				if ((saveBuffer == null) || (length > saveBuffer.Length))
				{
					// Allocate a new save buffer
					saveBuffer = new byte[length * (format.Bits / 8)];
				}

				int toWrite = 0;

				if (format.Bits == 8)
				{
					// 8-bit output
					for (int i = 0, j = 0; i < length; i++, j++)
					{
						int sample = buffer[i];

						saveBuffer[j] = (byte)(sample >> 24);
					}

					toWrite = length;
				}
				else if (format.Bits == 16)
				{
					// 16-bit output
					for (int i = 0, j = 0; i < length; i++, j += 2)
					{
						int sample = buffer[i];
						byte[] shortArray = BitConverter.GetBytes((short)(sample >> 16));

						saveBuffer[j] = shortArray[0];
						saveBuffer[j + 1] = shortArray[1];
					}

					toWrite = length * 2;
				}
				else if (format.Bits == 32)
				{
					// 32-bit output
					for (int i = 0, j = 0; i < length; i++, j += 4)
					{
						int sample = buffer[i];
						byte[] intArray = BitConverter.GetBytes(sample);

						saveBuffer[j] = intArray[0];
						saveBuffer[j + 1] = intArray[1];
						saveBuffer[j + 2] = intArray[2];
						saveBuffer[j + 3] = intArray[3];
					}

					toWrite = length * 4;
				}

				// Write the buffer to disk
				stream.Write(saveBuffer, 0, toWrite);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Save the tail of the sample
		/// </summary>
		/********************************************************************/
		public void SaveTail(Stream stream)
		{
		}
		#endregion
	}
}
