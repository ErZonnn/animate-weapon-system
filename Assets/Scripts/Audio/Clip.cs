/*Created by Pawe³ Mularczyk*/

using UnityEngine;

[System.Serializable]
public class Clip
{
    public AudioClip _audioClip;
    public Vector2 _pitchRange = new Vector2(1, 1);
    [Range(-1, 0)]
    public float _volumeCorection = 0f;
    public SoundType _soundType = SoundType.effects;
}
