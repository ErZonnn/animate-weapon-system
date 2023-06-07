/*Created by Pawe³ Mularczyk*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Weapon
{
    public bool _isUnlocked;

    [Space]
    public Animator _animator;
    public Transform _weaponTransform;
    [HideInInspector]
    public Vector3 _startWeaponLocalPosition;
    public Transform _aimingPosition;

    [Space]
    public int _ammoCount;
    public int _magazineAmmo;
    public int _maxMagazineAmmo;
    public bool _reloadOne;

    [Space]
    public float _aimingSpeed;
    public float _shootCoolDown;
    public float _reloadTime;
    public float _opcionalReloadTime;
    public float _hideWeaponTime;
    public float _drawnWeaponTime;

    [Space]
    public Transform[] _bulletSpawn;
    public GameObject _bulletPrefab;
    public float _bulletSpeed;
    public float _bulletBaseDamage;

    [Space]
    public GameObject _bulletShellPrefab;
    public Transform _bulletShellSpawnPoint;

    [Space]
    public GameObject _weaponAudioSource;
    public WeaponSounds _weaponSounds;

    [System.Serializable]
    public struct WeaponSounds
    {
        public Clip _shootClip;
        public Clip _lastShotClip;
        public Clip _reloadOptionalClip;
        public Clip _reloadClip;
        public Clip _drawnClip;
    }

    public void ReduceAmmo()
    {
        _magazineAmmo -= 1;
    }

    public void Reload()
    {
        if (_magazineAmmo == 0)
        {
            if (_ammoCount >= _maxMagazineAmmo)
            {
                _magazineAmmo = _maxMagazineAmmo;
                _ammoCount -= _maxMagazineAmmo;
            }
            else
            {
                _magazineAmmo = _ammoCount;
                _ammoCount = 0;
            }
        }
        else
        {
            _ammoCount += _magazineAmmo;
            _magazineAmmo = 0;

            if (_ammoCount >= _maxMagazineAmmo)
            {
                _magazineAmmo = _maxMagazineAmmo;
                _ammoCount -= _maxMagazineAmmo;
            }
            else
            {
                _magazineAmmo = _ammoCount;
                _ammoCount = 0;
            }
        }
    }
}
