/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Polycode.NostalgicPlayer.Controls.Inputs.RichText
{
	/// <summary>
	/// Turns a document into a list of laid out lines (word wrapped, with
	/// indentation and per run fonts) and provides mapping between pixel
	/// positions and character indices.
	///
	/// All character indices are global indices into the document's plain
	/// text (see RichTextDocument.GetPlainText), so they line up with what
	/// is copied to the clipboard
	/// </summary>
	internal sealed class RichTextLayout
	{
		#region Builder class
		/// <summary>
		/// Mutable state used while building the lines
		/// </summary>
		private sealed class Builder
		{
			private readonly List<LayoutLine> lines;
			private readonly int defaultHeight;

			private LayoutLine current;
			private int lineHeight;

			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public Builder(List<LayoutLine> lines, int defaultHeight)
			{
				this.lines = lines;
				this.defaultHeight = defaultHeight;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public int PenX
			{
				get; private set;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public bool HasContent
			{
				get; private set;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public int TotalHeight { get; private set; }



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public void StartLine(int left, int firstIndex)
			{
				current = new LayoutLine
				{
					Left = left,
					FirstIndex = firstIndex,
					EndIndex = firstIndex
				};

				PenX = left;
				HasContent = false;
				lineHeight = defaultHeight;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public void AddFragment(string text, Font font, Color? color, int width, int startIndex, int fontHeight)
			{
				current.Fragments.Add(new LayoutFragment
				{
					Text = text,
					Font = font,
					Color = color,
					X = PenX,
					Width = width,
					StartIndex = startIndex
				});

				PenX += width;
				HasContent = true;

				if (fontHeight > lineHeight)
					lineHeight = fontHeight;
			}



			/********************************************************************/
			/// <summary>
			/// 
			/// </summary>
			/********************************************************************/
			public void FinishLine(int endIndex)
			{
				current.Height = lineHeight;
				current.EndIndex = endIndex;
				current.Top = TotalHeight;

				TotalHeight += lineHeight;

				lines.Add(current);
			}
		}
		#endregion

		/// <summary>
		/// Text rendering flags. Must be identical for measuring and drawing
		/// so the selection highlight lines up with the glyphs
		/// </summary>
		public const TextFormatFlags TextFlags = TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.Left;

		private static readonly Size MeasureBounds = new Size(int.MaxValue, int.MaxValue);

		/********************************************************************/
		/// <summary>
		/// All the laid out lines, top to bottom
		/// </summary>
		/********************************************************************/
		public List<LayoutLine> Lines
		{
			get; private init;
		}



		/********************************************************************/
		/// <summary>
		/// The total pixel height of all the lines
		/// </summary>
		/********************************************************************/
		public int ContentHeight
		{
			get; private init;
		}



		/********************************************************************/
		/// <summary>
		/// Measure the pixel width of a piece of text in the given font
		/// </summary>
		/********************************************************************/
		public static int MeasureWidth(Graphics g, string text, Font font)
		{
			if (string.IsNullOrEmpty(text))
				return 0;

			return TextRenderer.MeasureText(g, text, font, MeasureBounds, TextFlags).Width;
		}



		/********************************************************************/
		/// <summary>
		/// Build the layout for a document
		/// </summary>
		/********************************************************************/
		public static RichTextLayout Build(RichTextDocument document, Font defaultFont, int textLeft, int textRight, Graphics g)
		{
			List<LayoutLine> lines = new List<LayoutLine>();
			int availableRight = Math.Max(textLeft + 1, textRight);

			Builder builder = new Builder(lines, defaultFont.Height);
			int globalIndex = 0;

			IReadOnlyList<RichTextParagraph> paragraphs = document.Paragraphs;

			for (int p = 0; p < paragraphs.Count; p++)
			{
				// The newline separating this paragraph from the previous one
				if (p > 0)
					globalIndex++;

				RichTextParagraph para = paragraphs[p];
				int firstLeft = textLeft + para.Indent;
				int wrapLeft = textLeft + para.Indent + para.HangingIndent;

				builder.StartLine(firstLeft, globalIndex);

				foreach (RichTextRun run in para.Runs)
				{
					Font font = run.Font ?? defaultFont;
					string text = run.Text;

					int i = 0;
					while (i < text.Length)
					{
						bool isSpace = char.IsWhiteSpace(text[i]);

						int j = i;
						while ((j < text.Length) && (char.IsWhiteSpace(text[j]) == isSpace))
							j++;

						string token = text.Substring(i, j - i);
						int tokenIndex = globalIndex + i;
						int tokenWidth = MeasureWidth(g, token, font);

						if (isSpace)
						{
							// Drop spaces that would sit at the start of a
							// wrapped line, but still advance the index so the
							// mapping to the plain text stays correct
							if (builder.HasContent)
								builder.AddFragment(token, font, run.Color, tokenWidth, tokenIndex, font.Height);
						}
						else
						{
							if (builder.HasContent && ((builder.PenX + tokenWidth) > availableRight))
							{
								builder.FinishLine(tokenIndex);
								builder.StartLine(wrapLeft, tokenIndex);
							}

							builder.AddFragment(token, font, run.Color, tokenWidth, tokenIndex, font.Height);
						}

						i = j;
					}

					globalIndex += text.Length;
				}

				builder.FinishLine(globalIndex);
			}

			return new RichTextLayout
			{
				Lines = lines,
				ContentHeight = builder.TotalHeight
			};
		}



		/********************************************************************/
		/// <summary>
		/// Return the x pixel position of a character index within a line
		/// </summary>
		/********************************************************************/
		public int IndexToX(LayoutLine line, int index, Graphics g)
		{
			int x = line.Left;

			foreach (LayoutFragment fragment in line.Fragments)
			{
				if (index <= fragment.StartIndex)
					return fragment.X;

				int fragmentEnd = fragment.StartIndex + fragment.Text.Length;
				if (index < fragmentEnd)
				{
					string prefix = fragment.Text.Substring(0, index - fragment.StartIndex);
					return fragment.X + MeasureWidth(g, prefix, fragment.Font);
				}

				x = fragment.X + fragment.Width;
			}

			return x;
		}



		/********************************************************************/
		/// <summary>
		/// Map an x pixel position within a line to the nearest character
		/// index
		/// </summary>
		/********************************************************************/
		public int HitTestLine(LayoutLine line, int x, Graphics g)
		{
			if (x <= line.Left)
				return line.FirstIndex;

			foreach (LayoutFragment fragment in line.Fragments)
			{
				if (x < fragment.X)
					return fragment.StartIndex;

				if (x < (fragment.X + fragment.Width))
				{
					int relative = x - fragment.X;
					int prevWidth = 0;

					for (int k = 1; k <= fragment.Text.Length; k++)
					{
						int width = MeasureWidth(g, fragment.Text.Substring(0, k), fragment.Font);
						int mid = (prevWidth + width) / 2;

						if (relative < mid)
							return fragment.StartIndex + k - 1;

						prevWidth = width;
					}

					return fragment.StartIndex + fragment.Text.Length;
				}
			}

			return line.EndIndex;
		}



		/********************************************************************/
		/// <summary>
		/// Find the line that contains the given content relative y position
		/// (0 = top of the first line). Returns the nearest line when the
		/// position is above or below all lines
		/// </summary>
		/********************************************************************/
		public LayoutLine FindLineAtY(int contentY)
		{
			if (Lines.Count == 0)
				return null;

			if (contentY < 0)
				return Lines[0];

			foreach (LayoutLine line in Lines)
			{
				if ((contentY >= line.Top) && (contentY < (line.Top + line.Height)))
					return line;
			}

			return Lines[^1];
		}
	}
}
