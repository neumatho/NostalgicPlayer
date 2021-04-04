/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Linq;
using Polycode.NostalgicPlayer.Kit.Bases;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.Sample
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class SampleWorker : SamplePlayerAgentBase
	{
		private readonly ISampleLoaderAgent loaderAgent;

		private LoadSampleFormatInfo formatInfo;

		private long totalLength;
		private long samplesRead;
		private int oldPosition;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SampleWorker(AgentInfo agentInfo)
		{
			loaderAgent = (ISampleLoaderAgent)agentInfo.Agent.CreateInstance(agentInfo.TypeId);
		}

		#region IPlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public override string[] FileExtensions => loaderAgent.FileExtensions;



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public override AgentResult Identify(PlayerFileInfo fileInfo)
		{
			return loaderAgent.Identify(fileInfo.ModuleStream);
		}



		/********************************************************************/
		/// <summary>
		/// Return the name of the module
		/// </summary>
		/********************************************************************/
		public override string ModuleName => formatInfo.Name;



		/********************************************************************/
		/// <summary>
		/// Return the name of the author
		/// </summary>
		/********************************************************************/
		public override string Author => formatInfo.Author;



		/********************************************************************/
		/// <summary>
		/// Returns the description and value on the line given. If the line
		/// is out of range, false is returned
		/// </summary>
		/********************************************************************/
		public override bool GetInformationString(int line, out string description, out string value)
		{
			if (line >= 3)
				return loaderAgent.GetInformationString(line - 3, out description, out value);

			// Find out which line to take
			switch (line)
			{
				// Bit size
				case 0:
				{
					description = Resources.IDS_SAMPLE_INFODESCLINE0;
					value = formatInfo.Bits.ToString();
					break;
				}

				// Frequency
				case 1:
				{
					description = Resources.IDS_SAMPLE_INFODESCLINE1;
					value = formatInfo.Frequency.ToString();
					break;
				}

				// Looping
				case 2:
				{
					description = Resources.IDS_SAMPLE_INFODESCLINE2;
					value = (formatInfo.Flags & LoadSampleFormatInfo.SampleFlags.Loop) != 0 ? Resources.IDS_SAMPLE_YES : Resources.IDS_SAMPLE_NO;
					break;
				}

				default:
				{
					description = null;
					value = null;

					return false;
				}
			}

			return true;
		}
		#endregion

		#region ISamplePlayerAgent implementation
		/********************************************************************/
		/// <summary>
		/// Will load the file into memory
		/// </summary>
		/********************************************************************/
		public override SamplePlayerSupportFlag SupportFlags => SamplePlayerSupportFlag.SetPosition;



		/********************************************************************/
		/// <summary>
		/// Will load the header information from the file
		/// </summary>
		/********************************************************************/
		public override AgentResult LoadHeaderInfo(ModuleStream stream, out string errorMessage)
		{
			// Start to initialize the converter
			if (!loaderAgent.InitLoader(out errorMessage))
				return AgentResult.Error;

			// Load the header
			if (!loaderAgent.LoadHeader(stream, out formatInfo, out errorMessage))
				return AgentResult.Error;

			return AgentResult.Ok;
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the player
		/// </summary>
		/********************************************************************/
		public override bool InitPlayer(ModuleStream stream)
		{
			// Get number of samples of the file
			totalLength = loaderAgent.GetTotalSampleLength(formatInfo);

			return base.InitPlayer(stream);
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the player
		/// </summary>
		/********************************************************************/
		public override void CleanupPlayer()
		{
			loaderAgent.CleanupLoader();

			base.CleanupPlayer();
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the player to start the sample from start
		/// </summary>
		/********************************************************************/
		public override void InitSound()
		{
			// Reset the sample position
			loaderAgent.SetSamplePosition(moduleStream, 0, formatInfo);

			samplesRead = 0;
			oldPosition = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Will load and decode a data block and store it in the buffer
		/// given
		/// </summary>
		/********************************************************************/
		public override int LoadDataBlock(int[] outputBuffer, int count)
		{
			// Load the next block of data
			int filled = loaderAgent.LoadData(moduleStream, outputBuffer, count, formatInfo);
			samplesRead += filled;

			if (filled == 0)
			{
				OnEndReached();

				// Loop the sample
				if ((formatInfo.Flags & LoadSampleFormatInfo.SampleFlags.Loop) != 0)
				{
					loaderAgent.SetSamplePosition(moduleStream, formatInfo.LoopStart, formatInfo);
					samplesRead = formatInfo.LoopStart;
				}
				else
				{
					loaderAgent.SetSamplePosition(moduleStream, 0, formatInfo);
					samplesRead = 0;
				}
			}

			// Check if we have changed position
			int pos = SongPosition;
			if (pos != oldPosition)
			{
				oldPosition = pos;
				OnPositionChanged();
			}

			return filled;
		}



		/********************************************************************/
		/// <summary>
		/// Return the number of channels the sample uses
		/// </summary>
		/********************************************************************/
		public override int ChannelCount => formatInfo.Channels;



		/********************************************************************/
		/// <summary>
		/// Return the frequency the sample is stored with
		/// </summary>
		/********************************************************************/
		public override int Frequency => formatInfo.Frequency;



		/********************************************************************/
		/// <summary>
		/// Holds the current position of the song
		/// </summary>
		/********************************************************************/
		public override int SongPosition
		{
			get
			{
				int pos = (int)(samplesRead * 100 / totalLength);
				if (pos >= 100)
					pos = 99;

				return pos;
			}

			set
			{
				long newPos = value * totalLength / 100;
				samplesRead = loaderAgent.SetSamplePosition(moduleStream, newPos, formatInfo);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Calculates the position time for each position
		/// </summary>
		/********************************************************************/
		public override TimeSpan GetPositionTimeTable(out TimeSpan[] positionTimes)
		{
			// Calculate the total time
			long totalTime = totalLength * 1000 / formatInfo.Frequency / formatInfo.Channels;

			// Now build the list
			positionTimes = Enumerable.Range(0, 100).Select(i => new TimeSpan(i * (totalTime / 100) * TimeSpan.TicksPerMillisecond)).ToArray();

			return new TimeSpan(totalTime * TimeSpan.TicksPerMillisecond);
		}
		#endregion
	}
}
