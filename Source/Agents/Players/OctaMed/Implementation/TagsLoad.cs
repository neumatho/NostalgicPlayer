/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Polycode.NostalgicPlayer.Agent.Player.OctaMed.Containers;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation
{
	/// <summary>
	/// 
	/// </summary>
	internal class TagsLoad<T> where T : Enum
	{
		private class TagPair
		{
			public T Tag { get; set; }
			public uint Value { get; set; }
			public bool TagChecked { get; set; }
		}

		private readonly ModuleStream stream;

		private readonly List<TagPair> tags;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public TagsLoad(ModuleStream moduleStream)
		{
			// Remember arguments
			stream = moduleStream;

			// Read all the tags
			tags = new List<TagPair>();

			for (;;)
			{
				uint tag = moduleStream.Read_B_UINT32();
				if (tag == ExpansionTags.TagEnd)
					break;

				tags.Add(new TagPair
				{
					Tag = (T)Enum.ToObject(typeof(T), tag),
					Value = moduleStream.Read_B_UINT32(),
					TagChecked = false
				});
			}
		}



		/********************************************************************/
		/// <summary>
		/// Checks to see if the tag given exists
		/// </summary>
		/********************************************************************/
		public bool TagExists(T tag)
		{
			foreach (TagPair tagPair in tags)
			{
				if (tagPair.Tag.Equals(tag))
				{
					tagPair.TagChecked = true;
					return true;
				}
			}

			return false;
		}



		/********************************************************************/
		/// <summary>
		/// Return the tag value of the tag given
		/// </summary>
		/********************************************************************/
		public uint TagVal(T tag, uint defVal = 0)
		{
			foreach (TagPair tagPair in tags)
			{
				if (tagPair.Tag.Equals(tag))
				{
					tagPair.TagChecked = true;
					return tagPair.Value;
				}
			}

			return defVal;
		}



		/********************************************************************/
		/// <summary>
		/// Will set the file pointer to the tag given's value
		/// </summary>
		/********************************************************************/
		public void SeekToTag(T tag)
		{
			foreach (TagPair tagPair in tags)
			{
				if (tagPair.Tag.Equals(tag))
				{
					tagPair.TagChecked = true;
					stream.Seek(tagPair.Value, SeekOrigin.Begin);
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Check to see if any tags hasn't been touched
		/// </summary>
		/********************************************************************/
		public bool CheckUnused()
		{
			return tags.Any(tagPair => !tagPair.TagChecked && (((uint)(object)tagPair.Tag & ExpansionTags.TagMustKnow) != 0));
		}
	}
}
