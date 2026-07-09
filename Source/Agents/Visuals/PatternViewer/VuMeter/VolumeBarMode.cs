//---------------------------------------------------------------------------------------
// <copyright file="VolumeBarMode.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.VuMeter
{
	/// <summary>
	/// Volume bar display mode
	/// </summary>
	internal enum VolumeBarMode
	{
		Off, // No volume bars
		NoteKick, // Show bar only on note trigger (original ProTracker style)
		RealVolume // Show actual sample volume (enhanced ProTracker style)
	}
}
