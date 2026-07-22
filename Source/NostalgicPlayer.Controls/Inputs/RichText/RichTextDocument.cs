/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Polycode.NostalgicPlayer.Controls.Inputs.RichText
{
	/// <summary>
	/// The logical content of a rich text view, built up as a list of
	/// paragraphs. Text is appended imperatively (mirroring the way a
	/// RichTextBox is filled via SelectedText), so no RTF parsing is needed.
	///
	/// The plain text representation joins the paragraphs with a single
	/// newline. This defines the global character index space that the
	/// layout and selection work in
	/// </summary>
	internal sealed class RichTextDocument
	{
		private readonly List<RichTextParagraph> paragraphs = new List<RichTextParagraph>();

		/********************************************************************/
		/// <summary>
		/// All the paragraphs in the document
		/// </summary>
		/********************************************************************/
		public IReadOnlyList<RichTextParagraph> Paragraphs => paragraphs;



		/********************************************************************/
		/// <summary>
		/// Remove all content
		/// </summary>
		/********************************************************************/
		public void Clear()
		{
			paragraphs.Clear();
		}



		/********************************************************************/
		/// <summary>
		/// Append text using the given attributes. Newlines in the text
		/// start new paragraphs. The indentation is applied to the paragraph
		/// the text is appended to as well as any new paragraphs created
		/// </summary>
		/********************************************************************/
		public void Append(string text, Font font, Color? color, int indent, int hangingIndent)
		{
			if (string.IsNullOrEmpty(text))
				return;

			RichTextParagraph current = EnsureCurrentParagraph();
			current.Indent = indent;
			current.HangingIndent = hangingIndent;

			int start = 0;

			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] == '\n')
				{
					string part = text.Substring(start, i - start).TrimEnd('\r');
					if (part.Length > 0)
						current.Runs.Add(new RichTextRun(part, font, color));

					current = new RichTextParagraph
					{
						Indent = indent,
						HangingIndent = hangingIndent
					};
					paragraphs.Add(current);

					start = i + 1;
				}
			}

			string last = text.Substring(start);
			if (last.Length > 0)
				current.Runs.Add(new RichTextRun(last, font, color));
		}



		/********************************************************************/
		/// <summary>
		/// Return the plain text of the whole document. Paragraphs are
		/// separated by a single newline. This is the text that selection
		/// indices refer to and what is copied to the clipboard
		/// </summary>
		/********************************************************************/
		public string GetPlainText()
		{
			StringBuilder sb = new StringBuilder();

			for (int p = 0; p < paragraphs.Count; p++)
			{
				if (p > 0)
					sb.Append('\n');

				foreach (RichTextRun run in paragraphs[p].Runs)
					sb.Append(run.Text);
			}

			return sb.ToString();
		}



		/********************************************************************/
		/// <summary>
		/// Return the total number of characters, matching the length of
		/// GetPlainText (including the newlines between paragraphs)
		/// </summary>
		/********************************************************************/
		public int GetTextLength()
		{
			if (paragraphs.Count == 0)
				return 0;

			int length = paragraphs.Count - 1;

			foreach (RichTextParagraph paragraph in paragraphs)
				length += paragraph.Length;

			return length;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Make sure there is a paragraph to append to
		/// </summary>
		/********************************************************************/
		private RichTextParagraph EnsureCurrentParagraph()
		{
			if (paragraphs.Count == 0)
				paragraphs.Add(new RichTextParagraph());

			return paragraphs[^1];
		}
		#endregion
	}
}
