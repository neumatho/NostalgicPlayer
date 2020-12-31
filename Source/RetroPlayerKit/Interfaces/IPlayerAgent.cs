/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of RetroPlayer is keep. See the LICENSE file for more information. */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / RetroPlayer team.                         */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.RetroPlayer.RetroPlayerKit.Containers;

namespace Polycode.RetroPlayer.RetroPlayerKit.Interfaces
{
	/// <summary>
	/// Agents of this type can play some kind of file
	/// </summary>
	public interface IPlayerAgent : IAgentWorker
	{
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		string[] GetFileExtensions();

		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		AgentResult Identify(PlayerFileInfo fileInfo);

		/// <summary>
		/// Return the name of the module
		/// </summary>
		string GetModuleName();

		/// <summary>
		/// Return the name of the author
		/// </summary>
		string GetAuthor();

		/// <summary>
		/// This is the main player method
		/// </summary>
		void Play();

		/// <summary>
		/// Indicate if the end has been reached of the file
		/// </summary>
		bool EndReached { get; set; }
	}
}
