/*Created by Pawe³ Mularczyk*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerSoundType
{
    jump,
    landing,
};

public class PlayerSoundSystem : MonoBehaviour
{
    [SerializeField] private VolumeMaster _volumeMaster;
    [Space]
    [Header("FOOT STEPS SETTINGS")]
    [SerializeField] private Clip _footStepClip;
    [SerializeField] private Vector2 _footStepFrequency;
    [SerializeField] private AudioSource _footStepAudioSource;
    [Space]
    [Header("JUMPING SETTINGS")]
    [SerializeField] private Clip _jumpClip;
    [SerializeField] private AudioSource _jumpAudioSource;
    [Space]
    [Header("LANDING SETTINGS")]
    [SerializeField] private Clip _landingClip;
    [SerializeField] private AudioSource _landingAudioSource;

    private float _footStepTimer;

    public void FootStepSound(Vector2 moveInput, bool isGrounded)
    {
        _footStepTimer += Time.deltaTime;

        if (moveInput.magnitude == 0 || !isGrounded)
            return;

        if (_footStepTimer >= RemapFootStepFrequency(moveInput.magnitude, _footStepFrequency.y, _footStepFrequency.x))
        {
            _footStepAudioSource.pitch = Random.Range(_footStepClip._pitchRange.x, _footStepClip._pitchRange.y);
            _footStepAudioSource.volume = _volumeMaster.GetVolume(_footStepClip._soundType) + (_volumeMaster.GetVolume(_footStepClip._soundType) * _footStepClip._volumeCorection);

            _footStepAudioSource.PlayOneShot(_footStepClip._audioClip);

            _footStepTimer = 0;
        }
    }

    private static float RemapFootStepFrequency(float input, float maxFrequency, float minFrequency)
    {
        float x = Mathf.InverseLerp(0, 1.1f, input);
        float value = Mathf.Lerp(maxFrequency, minFrequency, x);
        return value;
    }

    public void PlaySound(PlayerSoundType playerSoundType)
    {
        switch (playerSoundType)
        {
            case PlayerSoundType.jump:

                if (_jumpAudioSource.isPlaying)
                    return;

                _jumpAudioSource.pitch = Random.Range(_jumpClip._pitchRange.x, _jumpClip._pitchRange.y);
                _jumpAudioSource.volume = _volumeMaster.GetVolume(_jumpClip._soundType) + (_volumeMaster.GetVolume(_jumpClip._soundType) * _jumpClip._volumeCorection);

                _jumpAudioSource.PlayOneShot(_jumpClip._audioClip);

                break;

            case PlayerSoundType.landing:

                if (_landingAudioSource.isPlaying)
                    return;

                _landingAudioSource.pitch = Random.Range(_landingClip._pitchRange.x, _landingClip._pitchRange.y);
                _landingAudioSource.volume = _volumeMaster.GetVolume(_landingClip._soundType) + (_volumeMaster.GetVolume(_landingClip._soundType) * _landingClip._volumeCorection);

                _landingAudioSource.PlayOneShot(_landingClip._audioClip);

                break;
        }
    }

}
