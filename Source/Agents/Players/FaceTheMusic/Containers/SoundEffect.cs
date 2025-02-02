/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.FaceTheMusic.Containers
{
	/// <summary>
	/// The different script effect commands
	/// </summary>
	internal enum SoundEffect
	{
		Nothing = 0,
		Wait,
		Goto,
		Loop,
		GotoScript,
		End,
		IfPitchEqual,
		IfPitchLessThan,
		IfPitchGreaterThan,
		IfVolumeEqual,
		IfVolumeLessThan,
		IfVolumeGreaterThan,
		OnNewPitch,
		OnNewVolume,
		OnNewSample,
		OnRelease,
		OnPortamento,
		OnVolumeDown,
		PlayCurrentSample,
		PlayQuietSample,
		PlayPosition,
		PlayPositionAdd,
		PlayPositionSub,
		Pitch,
		Detune,
		DetunePitchAdd,
		DetunePitchSub,
		Volume,
		VolumeAdd,
		VolumeSub,
		CurrentSample,
		SampleStart,
		SampleStartAdd,
		SampleStartSub,
		OneshotLength,
		OneshotLengthAdd,
		OneshotLengthSub,
		RepeathLength,
		RepeathLengthAdd,
		RepeathLengthSub,
		GetPitchOfTrack,
		GetVolumeOfTrack,
		GetSampleOfTrack,
		CloneTrack,
		FirstLfoStart,
		FirstLfoSpeedDepthAdd,
		FirstLfoSpeedDepthSub,
		SecondLfoStart,
		SecondLfoSpeedDepthAdd,
		SecondLfoSpeedDepthSub,
		ThirdLfoStart,
		ThirdLfoSpeedDepthAdd,
		ThirdLfoSpeedDepthSub,
		FourthLfoStart,
		FourthLfoSpeedDepthAdd,
		FourthLfoSpeedDepthSub,
		WorkOnTrack,
		WorkTrackAdd,
		GlobalVolume,
		GlobalSpeed,
		TicksPerLine,
		JumpToSongLine
	}
}
