/*Created by Pawe³ Mularczyk*/

using UnityEngine;

public enum SoundType
{
    effects,
    music,
    ambient,
    ui
};

public class VolumeMaster : MonoBehaviour
{
    [Header("VOLUME SETTINGS")]
    [Range(0, 1)]
    [SerializeField] private float _globalVolume = 1f;
    [Range(0, 1)]
    [SerializeField] private float _effectsVolume = 0.5f;
    [Range(0, 1)]
    [SerializeField] private float _musicVolume = 0.5f;
    [Range(0, 1)]
    [SerializeField] private float _ambientVolume = 0.5f;
    [Range(0, 1)]
    [SerializeField] private float _uiVolume = 0.5f;

    public float GetVolume(SoundType soundType)
    {
        switch (soundType)
        {
            case SoundType.effects:
                return _effectsVolume * _globalVolume;

            case SoundType.music:
                return _musicVolume * _globalVolume;

            case SoundType.ambient:
                return _ambientVolume * _globalVolume;

            case SoundType.ui:
                return _uiVolume * _globalVolume;
        }

        return 0;
    }
}
