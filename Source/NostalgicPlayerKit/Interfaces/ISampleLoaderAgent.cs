/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Kit.Interfaces
{
	/// <summary>
	/// Agents of this type, can load samples
	/// </summary>
	public interface ISampleLoaderAgent : IAgentWorker
	{
		/// <summary>
		/// Return the file extensions that is supported by the loader
		///
		/// Has to be in lowercase
		/// </summary>
		string[] FileExtensions { get; }

		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		AgentResult Identify(ModuleStream stream);

		/// <summary>
		/// Returns the description and value on the line given. If the line
		/// is out of range, false is returned
		/// </summary>
		bool GetInformationString(int line, out string description, out string value);

		/// <summary>
		/// Initialize the loader
		/// </summary>
		bool InitLoader(out string errorMessage);

		/// <summary>
		/// Cleanup the loader
		/// </summary>
		void CleanupLoader();

		/// <summary>
		/// Load the sample header
		/// </summary>
		bool LoadHeader(ModuleStream stream, out LoadSampleFormatInfo formatInfo, out string errorMessage);

		/// <summary>
		/// Load some part of the sample data
		/// </summary>
		int LoadData(ModuleStream stream, int[] buffer, int length, LoadSampleFormatInfo formatInfo);

		/// <summary>
		/// Calculates how many samples that will be returned
		/// </summary>
		long GetTotalSampleLength(LoadSampleFormatInfo formatInfo);

		/// <summary>
		/// Sets the file position to the sample position given
		/// </summary>
		long SetSamplePosition(ModuleStream stream, long position, LoadSampleFormatInfo formatInfo);
	}
}
