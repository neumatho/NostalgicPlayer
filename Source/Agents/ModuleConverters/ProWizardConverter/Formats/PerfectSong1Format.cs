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

namespace Polycode.NostalgicPlayer.Agent.ModuleConverter.ProWizardConverter.Formats
{
	/// <summary>
	/// Perfect Song Packer 
	/// </summary>
	internal class PerfectSong1Format : PerfectSongFormatBase
	{
		#region ModuleConverterAgentBase implementation
		/********************************************************************/
		/// <summary>
		/// Return the original format. If it returns null or an empty
		/// string, the agent name will be used
		/// </summary>
		/********************************************************************/
		public override string OriginalFormat => Resources.IDS_PROWIZ_NAME_AGENT62;
		#endregion

		#region ProWizardConverterWorkerBase implementation
		/********************************************************************/
		/// <summary>
		/// Check to see if the module is in the correct format
		/// </summary>
		/********************************************************************/
		protected override bool CheckModule(ModuleStream moduleStream)
		{
			if (moduleStream.Length < 0x3fe)
				return false;

			// Start to check the mark
			moduleStream.Seek(0, SeekOrigin.Begin);

			if (moduleStream.Read_B_UINT64() != 0x50455246534f4e47)		// PERFSONG
				return false;

			uint temp = moduleStream.Read_B_UINT32() + 12;
			if ((temp > moduleStream.Length) || (temp < 802))
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Prepare conversion by initialize what is needed etc.
		/// </summary>
		/********************************************************************/
		protected override bool PrepareConversion(ModuleStream moduleStream)
		{
			moduleStream.Seek(8, SeekOrigin.Begin);

			uint temp = moduleStream.Read_B_UINT32();
			textOffset = temp - 690;
			sampleStartOffset = 0;

			CreatePositionList(moduleStream);

			return true;
		}
		#endregion

		#region PerfectSongFormatBase implemenatation
		/********************************************************************/
		/// <summary>
		/// Convert mapped effect back to ProTracker and change the effect
		/// value if needed
		/// </summary>
		/********************************************************************/
		protected override void ConvertEffect(ref byte effect, ref byte effectVal)
		{
			switch (effect)
			{
				case 0x00:
					break;

				case 0x04:
				{
					effect = 0x0;
					break;
				}

				case 0x08:
				{
					effect = 0x1;
					break;
				}

				case 0x0c:
				{
					effect = 0x2;
					break;
				}

				case 0x10:
				case 0x14:
				{
					effect = 0x3;
					break;
				}

				case 0x18:
				case 0x1c:
				{
					effect = 0x4;
					break;
				}

				case 0x20:
				{
					effect = 0x5;
					break;
				}

				case 0x24:
				{
					effect = 0x6;
					effectVal = (byte)(((effectVal & 0xf0) >> 4) | ((effectVal & 0x0f) << 4));
					break;
				}

				case 0x28:
				case 0x2c:
				{
					effect = 0x6;
					break;
				}

				case 0x30:
				{
					effect = 0x7;
					break;
				}

				case 0x38:
				{
					effect = 0x9;
					break;
				}

				case 0x3c:
				{
					effect = 0xa;
					effectVal = (byte)(((effectVal & 0xf0) >> 4) | ((effectVal & 0x0f) << 4));
					break;
				}

				case 0x40:
				{
					effect = 0xa;
					break;
				}

				case 0x44:
				{
					effect = 0xb;
					break;
				}

				case 0x48:
				{
					effect = 0xc;
					break;
				}

				case 0x4c:
				{
					effect = 0xd;
					break;
				}

				case 0x50:
				{
					effect = 0xf;
					break;
				}

				case 0x58:
				{
					effect = 0xe;
					effectVal = 0x01;
					break;
				}

				case 0x5c:
				{
					effect = 0xe;
					effectVal |= 0x10;
					break;
				}

				case 0x60:
				{
					effect = 0xe;
					effectVal |= 0x20;
					break;
				}

				case 0x74:
				{
					effect = 0xe;
					effectVal |= 0xa0;
					break;
				}

				case 0x78:
				{
					effect = 0xe;
					effectVal |= 0xb0;
					break;
				}

				case 0x7c:
				{
					effect = 0xe;
					effectVal |= 0xe0;
					break;
				}

				default:
				{
					effect = 0x0;
					effectVal = 0x00;
					break;
				}
			}
		}
		#endregion
	}
}
