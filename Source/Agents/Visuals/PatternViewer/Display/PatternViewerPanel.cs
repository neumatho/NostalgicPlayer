//---------------------------------------------------------------------------------------
// <copyright file="PatternViewerPanel.cs" company="NostalgicPlayer">
// Copyright (c) NostalgicPlayer. All rights reserved.
// </copyright>
//---------------------------------------------------------------------------------------
using System.Windows.Forms;

namespace Polycode.NostalgicPlayer.Agent.Visual.PatternViewer.Display
{
	/// <summary>
	/// Double buffered panel for flicker-free drawing
	/// </summary>
	internal class PatternViewerPanel : Panel
	{
		public PatternViewerPanel()
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint |
			         ControlStyles.UserPaint |
			         ControlStyles.OptimizedDoubleBuffer |
			         ControlStyles.ResizeRedraw, true);
			UpdateStyles();
		}
	}
}
