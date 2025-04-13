/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.Agent.Player.OctaMed.Containers;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Utility;

namespace Polycode.NostalgicPlayer.Agent.Player.OctaMed.Implementation
{
	/// <summary>
	/// 
	/// </summary>
	internal class EffectGroup : IDeepCloneable<EffectGroup>
	{
		private SubSong subSong;
		private uint lastMixerFrequency;

		private string name;

		private EchoMode echoMode;
		private byte echoDepth;
		private ushort echoLength;
		private int[][] echoBuffer;
		private int echoPosition;

		private sbyte stereoSeparator;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public EffectGroup(SubSong ss)
		{
			subSong = ss;
		}



		/********************************************************************/
		/// <summary>
		/// Set the name of the group
		/// </summary>
		/********************************************************************/
		public void SetName(byte[] name)
		{
			this.name = EncoderCollection.Amiga.GetString(name);
		}



		/********************************************************************/
		/// <summary>
		/// Set the echo mode parameter
		/// </summary>
		/********************************************************************/
		public void SetEchoMode(EchoMode mode)
		{
			echoMode = mode;
		}



		/********************************************************************/
		/// <summary>
		/// Set the echo depth parameter
		/// </summary>
		/********************************************************************/
		public void SetEchoDepth(byte depth)
		{
			echoDepth = depth;
			echoBuffer = null;
		}



		/********************************************************************/
		/// <summary>
		/// Set the echo length parameter
		/// </summary>
		/********************************************************************/
		public void SetEchoLength(ushort length)
		{
			echoLength = length;
		}



		/********************************************************************/
		/// <summary>
		/// Set the stereo separation parameter
		/// </summary>
		/********************************************************************/
		public void SetStereoSeparation(sbyte separator)
		{
			stereoSeparator = separator;
		}



		/********************************************************************/
		/// <summary>
		/// Change the parent sub-song
		/// </summary>
		/********************************************************************/
		public void SetParent(SubSong ss)
		{
			subSong = ss;
		}



		/********************************************************************/
		/// <summary>
		/// Initialize all effect groups
		/// </summary>
		/********************************************************************/
		public void Initialize()
		{
			echoPosition = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup all effect groups
		/// </summary>
		/********************************************************************/
		public void Cleanup()
		{
			echoBuffer = null;
			lastMixerFrequency = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Will add DSP effect to the mixed output
		/// </summary>
		/********************************************************************/
		public void DoEffects(int[][] dest, int todoInFrames, uint mixerFrequency)
		{
			if (echoMode != EchoMode.None)
				DoEcho(dest, todoInFrames, mixerFrequency);

			if ((stereoSeparator != 0) && subSong.GetStereo())
				DoStereoSeparation(dest, todoInFrames);
		}



		/********************************************************************/
		/// <summary>
		/// Make a deep copy of the current object
		/// </summary>
		/********************************************************************/
		public EffectGroup MakeDeepClone()
		{
			EffectGroup clone = (EffectGroup)MemberwiseClone();

			if (echoBuffer != null)
				clone.echoBuffer = ArrayHelper.CloneArray(echoBuffer);

			return clone;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Apply echo to the output
		/// </summary>
		/********************************************************************/
		private void DoEcho(int[][] dest, int todoInFrames, uint mixerFrequency)
		{
			if ((echoLength > 0) && (echoDepth > 0))
			{
				if ((echoBuffer == null) || (mixerFrequency != lastMixerFrequency))
				{
					uint bufLen = echoLength * mixerFrequency / 1000;

					echoBuffer = ArrayHelper.Initialize2Arrays<int>(2, (int)bufLen);
					lastMixerFrequency = mixerFrequency;
				}

				switch (echoMode)
				{
					case EchoMode.Normal:
					{
						DoNormalEcho(dest, todoInFrames);
						break;
					}

					case EchoMode.CrossEcho:
					{
						DoCrossEcho(dest, todoInFrames);
						break;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Apply normal echo to the output
		/// </summary>
		/********************************************************************/
		private void DoNormalEcho(int[][] dest, int todoInFrames)
		{
			int copied = 0;
			int echoBufferLength = echoBuffer[0].Length;

			while (copied < todoInFrames)
			{
				if (echoPosition >= echoBufferLength)
					echoPosition = 0;

				int toCopy = Math.Min(todoInFrames - copied, echoBufferLength - echoPosition);
				if (toCopy == 0)
					break;

				for (int i = 0; i < toCopy; i++)
				{
					dest[0][copied] += echoBuffer[0][echoPosition] >> echoDepth;
					echoBuffer[0][echoPosition] = dest[0][copied];

					dest[1][copied] += echoBuffer[1][echoPosition] >> echoDepth;
					echoBuffer[1][echoPosition++] = dest[1][copied++];
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Apply cross echo to the output
		/// </summary>
		/********************************************************************/
		private void DoCrossEcho(int[][] dest, int todoInFrames)
		{
			int copied = 0;
			int echoBufferLength = echoBuffer[0].Length;

			while (copied < todoInFrames)
			{
				if (echoPosition >= echoBufferLength)
					echoPosition = 0;

				int toCopy = Math.Min(todoInFrames - copied, echoBufferLength - echoPosition);
				if (toCopy == 0)
					break;

				for (int i = 0; i < toCopy; i++)
				{
					dest[0][copied] += echoBuffer[1][echoPosition] >> echoDepth;
					dest[1][copied] += echoBuffer[0][echoPosition] >> echoDepth;

					echoBuffer[0][echoPosition] = dest[0][copied];
					echoBuffer[1][echoPosition++] = dest[1][copied++];
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Apply stereo separation to the output
		/// </summary>
		/********************************************************************/
		private void DoStereoSeparation(int[][] dest, int todoInFrames)
		{
			if (stereoSeparator < 0)
			{
				int shift = stereoSeparator + 5;

				for (int i = 0; i < todoInFrames; i++)
				{
					long left = dest[0][i];
					long right = dest[1][i];

					long newLeft = left + (right >> shift);
					long newRight = right + (left >> shift);

					if (newLeft > int.MaxValue)
						newLeft = int.MaxValue;
					else if (newLeft < int.MinValue)
						newLeft = int.MinValue;

					if (newRight > int.MaxValue)
						newRight = int.MaxValue;
					else if (newRight < int.MinValue)
						newRight = int.MinValue;

					dest[0][i] = (int)newLeft;
					dest[1][i] = (int)newRight;
				}
			}
			else
			{
				int shift = 5 - stereoSeparator;

				for (int i = 0; i < todoInFrames; i++)
				{
					int left = dest[0][i];
					int right = dest[1][i];

					long newLeft = left - (right >> shift);
					long newRight = right - (left >> shift);

					if (newLeft > int.MaxValue)
						newLeft = int.MaxValue;
					else if (newLeft < int.MinValue)
						newLeft = int.MinValue;

					if (newRight > int.MaxValue)
						newRight = int.MaxValue;
					else if (newRight < int.MinValue)
						newRight = int.MinValue;

					dest[0][i] = (int)newLeft;
					dest[1][i] = (int)newRight;
				}
			}
		}
		#endregion
	}
}
