/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
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
		private bool lastStereoMode;

		private string name;

		private EchoMode echoMode;
		private byte echoDepth;
		private ushort echoLength;
		private int[] echoBuffer;
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
		public void DoEffects(int[] dest, int todo, uint mixerFrequency, bool stereo)
		{
			if (echoMode != EchoMode.None)
				DoEcho(dest, todo, mixerFrequency, stereo);

			if (stereo && (stereoSeparator != 0) && subSong.GetStereo())
				DoStereoSeparation(dest, todo);
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
		private void DoEcho(int[] dest, int todo, uint mixerFrequency, bool stereo)
		{
			if ((echoLength > 0) && (echoDepth > 0))
			{
				if ((echoBuffer == null) || (mixerFrequency != lastMixerFrequency) || (stereo != lastStereoMode))
				{
					uint bufLen = echoLength * mixerFrequency / 1000;
					if (stereo)
						bufLen *= 2;

					echoBuffer = new int[bufLen];
					lastMixerFrequency = mixerFrequency;
					lastStereoMode = stereo;
				}

				switch (echoMode)
				{
					case EchoMode.Normal:
					{
						if (stereo)
							DoNormalEchoStereo(dest, todo);
						else
							DoNormalEchoMono(dest, todo);

						break;
					}

					case EchoMode.CrossEcho:
					{
						if (stereo)
							DoCrossEchoStereo(dest, todo);
						else
							DoNormalEchoMono(dest, todo);

						break;
					}
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Apply normal echo to the mono output
		/// </summary>
		/********************************************************************/
		private void DoNormalEchoMono(int[] dest, int todo)
		{
			int copied = 0;

			while (copied < todo)
			{
				if (echoPosition >= echoBuffer.Length)
					echoPosition = 0;

				int toCopy = Math.Min(todo - copied, echoBuffer.Length - echoPosition);
				if (toCopy == 0)
					break;

				for (int i = 0; i < toCopy; i++)
				{
					dest[copied] += echoBuffer[echoPosition] >> echoDepth;
					echoBuffer[echoPosition++] = dest[copied++];
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Apply normal echo to the stereo output
		/// </summary>
		/********************************************************************/
		private void DoNormalEchoStereo(int[] dest, int todo)
		{
			int copied = 0;

			while (copied < todo)
			{
				if (echoPosition >= echoBuffer.Length)
					echoPosition = 0;

				int toCopy = Math.Min(todo - copied, echoBuffer.Length - echoPosition) / 2;
				if (toCopy == 0)
					break;

				for (int i = 0; i < toCopy; i++)
				{
					dest[copied] += echoBuffer[echoPosition] >> echoDepth;
					echoBuffer[echoPosition++] = dest[copied++];

					dest[copied] += echoBuffer[echoPosition] >> echoDepth;
					echoBuffer[echoPosition++] = dest[copied++];
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Apply cross echo to the stereo output
		/// </summary>
		/********************************************************************/
		private void DoCrossEchoStereo(int[] dest, int todo)
		{
			int copied = 0;

			while (copied < todo)
			{
				if (echoPosition >= echoBuffer.Length)
					echoPosition = 0;

				int toCopy = Math.Min(todo - copied, echoBuffer.Length - echoPosition) / 2;
				if (toCopy == 0)
					break;

				for (int i = 0; i < toCopy; i++)
				{
					dest[copied] += echoBuffer[echoPosition + 1] >> echoDepth;
					dest[copied + 1] += echoBuffer[echoPosition] >> echoDepth;

					echoBuffer[echoPosition++] = dest[copied++];
					echoBuffer[echoPosition++] = dest[copied++];
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Apply stereo separation to the output
		/// </summary>
		/********************************************************************/
		private void DoStereoSeparation(int[] dest, int todo)
		{
			todo /= 2;

			if (stereoSeparator < 0)
			{
				int shift = stereoSeparator + 5;

				for (int i = 0; i < todo; i += 2)
				{
					int left = dest[i];
					int right = dest[i + 1];

					dest[i] += right >> shift;
					dest[i + 1] += left >> shift;
				}
			}
			else
			{
				int shift = 5 - stereoSeparator;

				for (int i = 0; i < todo; i += 2)
				{
					int left = dest[i];
					int right = dest[i + 1];

					dest[i] -= right >> shift;
					dest[i + 1] -= left >> shift;
				}
			}
		}
		#endregion
	}
}
