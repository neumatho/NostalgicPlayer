/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.IO;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ProWizardConverter.Formats
{
	/// <summary>
	/// Laxity Tracker
	/// </summary>
	internal class LaxityTrackerFormat : UnicTrackerFormatBase
	{
		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT13;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 0x43c)
				return false;

			// Check the position list size
			moduleStream.Seek(0x3a2, SeekOrigin.Begin);

			byte positionListLength = moduleStream.Read_UINT8();
			if ((positionListLength > 0x7f) || (positionListLength == 0x00))
				return false;

			// Check NTK byte
			byte temp = moduleStream.Read_UINT8();
			if ((temp > 0x3f) && (temp != 0x7f))
				return false;

			// Check the first two pattern numbers in the position table
			if ((moduleStream.Read_UINT8() > 0x3f) || (moduleStream.Read_UINT8() > 0x3f))
				return false;

			// No 'M.K.' in a Laxity module
			moduleStream.Seek(0x438, SeekOrigin.Begin);
			if (moduleStream.Read_B_UINT32() == 0x4d2e4b2e)		// M.K.
				return false;

			// Check sample information
			moduleStream.Seek(0, SeekOrigin.Begin);

			uint samplesSize = 0;

			for (int i = 0; i < 31; i++)
			{
				// Skip sample name
				moduleStream.Seek(22, SeekOrigin.Current);

				// Check sample size
				ushort temp1 = moduleStream.Read_B_UINT16();
				if (temp1 >= 0x8000)
					return false;

				// Check volume
				if ((moduleStream.Read_UINT8() != 0x00) || (moduleStream.Read_UINT8() > 0x40))
					return false;

				// Check loop values
				if (temp1 == 0)
				{
					// Check loop length
					moduleStream.Seek(2, SeekOrigin.Current);

					if (moduleStream.Read_B_UINT16() != 0x0001)
						return false;
				}
				else
				{
					samplesSize += temp1 * 2U;

					uint temp2 = moduleStream.Read_B_UINT16();
					uint temp3 = moduleStream.Read_B_UINT16();

					if ((temp2 + temp3) > temp1)
						return false;
				}
			}

			// Get the number of positions
			if (FindHighestPatternNumber(moduleStream, 0x3a4, positionListLength) > 0x40)
				return false;

			// Check the module length
			if ((0x424 + numberOfPatterns * 0x300 + samplesSize) > (moduleStream.Length + MaxNumberOfMissingBytes))
				return false;

			// Check for ProTracker patterns
			moduleStream.Seek(0x424, SeekOrigin.Begin);

			if (CheckPatterns(moduleStream))
				return false;

			// Check for Unic pattern format
			moduleStream.Seek(0x424, SeekOrigin.Begin);

			for (int i = 0; i < numberOfPatterns * 256; i++)
			{
				// Check note
				if (moduleStream.Read_UINT8() > 0x66)
					return false;

				moduleStream.Seek(2, SeekOrigin.Current);
			}

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Return a collection with all the sample information
		/// </summary>
		/********************************************************************/
		protected override IEnumerable<SampleInfo> GetSamples(ModuleStream moduleStream)
		{
			// Find out, if fine tune should be converted or not
			long pos = moduleStream.Position;
			moduleStream.Seek(0x3a3, SeekOrigin.Begin);

			byte temp = moduleStream.Read_UINT8();
			bool setFineTune = (temp == 0x7f) || (temp == 0x00);

			moduleStream.Seek(pos, SeekOrigin.Begin);

			return GetAllSamples(moduleStream, setFineTune);
		}
		#endregion
	}
}
