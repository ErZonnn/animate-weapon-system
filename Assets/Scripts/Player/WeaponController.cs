using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    [Header("WEAPON CONTROL SETTINGS")]
    [SerializeField] private bool _isAiming = false;

    [Space]
    [Header("WEAPONS SETTINGS")]
    [SerializeField] private int _activeWeaponID;
    [SerializeField] private Weapon[] _weapons;

    #region PRIVATE VARIABLES

    private VolumeMaster _volumeMaster;

    private InputMaster _inputMaster;
    private InputAction _aimingAction;
    private InputAction _shootAction;
    private InputAction _reloadAction;
    private InputAction _switchAction;

    private MovementSystem _movementSystem;

    private bool _shootInput;
    private bool _isShooting = false;
    private bool _isReloading = false;
    private bool _canShoot = true;
    private bool _lastShoot = false;
    private bool _reloadInput = false;
    private bool _drawnWeapon = false;
    private bool _switchInput;
    private bool _switchedWeapon = false;

    private float _reloadOneTimer;

    #endregion

    private void Awake()
    {
        _inputMaster = new InputMaster();
    }

    private void Start()
    {
        _volumeMaster = GameObject.FindGameObjectWithTag("GameController").GetComponent<VolumeMaster>();
        _movementSystem = GetComponent<MovementSystem>();

        for(int i = 0; i < _weapons.Length; i++)
        {
            _weapons[i]._startWeaponLocalPosition = _weapons[i]._weaponTransform.localPosition;
        }
    }

    private void OnEnable()
    {
        _aimingAction = _inputMaster.Player.Aiming;
        _shootAction = _inputMaster.Player.Fire;
        _reloadAction = _inputMaster.Player.Reload;
        _switchAction = _inputMaster.Player.Switch;

        _switchAction.Enable();
        _reloadAction.Enable();
        _shootAction.Enable();
        _aimingAction.Enable();
    }

    private void OnDisable()
    {
        _switchAction.Disable();
        _reloadAction.Disable();
        _shootAction.Disable();
        _aimingAction.Disable();
    }

    private void Update()
    {
        InputHandle();

        if (_drawnWeapon)
        {
            AnimationController();
            ReloadControl();
            SwitchWeapon();
        }
    }
  
    private void AnimationController()
    {
        _weapons[_activeWeaponID]._animator.SetBool("_empty", _weapons[_activeWeaponID]._magazineAmmo == 0 ? true : false);
        _weapons[_activeWeaponID]._animator.SetBool("_shoot", _isShooting);
        _weapons[_activeWeaponID]._animator.SetBool("_reload", _isReloading);
        _weapons[_activeWeaponID]._animator.SetBool("_lastShoot", _lastShoot);
        _weapons[_activeWeaponID]._animator.SetBool("_drawnWeapon", _drawnWeapon);
    }

    private void SwitchWeapon()
    {
        if (_isReloading)
            return;

        int unlockedWeapons = 0;

        for (int i = 0; i < _weapons.Length; i++)
            if (_weapons[i]._isUnlocked)
                unlockedWeapons++;

        if (unlockedWeapons <= 1)
            return;

        if (_switchInput && !_switchedWeapon)
        {
            //for now
            int switchedWeapon = _activeWeaponID;
            switchedWeapon++;

            if (switchedWeapon > _weapons.Length - 1)
                switchedWeapon = 0;

            Debug.Log("Weapon ID: " + switchedWeapon);

            DrawnWeaponControl(switchedWeapon);

            _switchedWeapon = true;

            StartCoroutine(SwitchWeaponDelay());
        }
    }

    private IEnumerator SwitchWeaponDelay()
    {
        yield return new WaitForSeconds(1f);
        _switchedWeapon = false;
    }

    private void ReloadControl()
    {
        if (_weapons[_activeWeaponID]._magazineAmmo == _weapons[_activeWeaponID]._maxMagazineAmmo)
            return;

        if (_reloadInput && _weapons[_activeWeaponID]._ammoCount > 0 && !_isShooting && _canShoot)
        {
            float reloadTime;

            if (_weapons[_activeWeaponID]._reloadOne)
            {
                int missingAmmo = _weapons[_activeWeaponID]._maxMagazineAmmo - _weapons[_activeWeaponID]._magazineAmmo;
                missingAmmo = _weapons[_activeWeaponID]._ammoCount >= missingAmmo ? missingAmmo : _weapons[_activeWeaponID]._ammoCount;

                reloadTime = _weapons[_activeWeaponID]._reloadTime * missingAmmo;

                StartCoroutine(CanShoot(reloadTime + _weapons[_activeWeaponID]._opcionalReloadTime));
                StartCoroutine(ControlReloadOneSound(missingAmmo, _weapons[_activeWeaponID]._reloadTime));
            }
            else 
            {
                reloadTime = _weapons[_activeWeaponID]._magazineAmmo == 0 ? _weapons[_activeWeaponID]._opcionalReloadTime : _weapons[_activeWeaponID]._reloadTime;
                StartCoroutine(CanShoot(reloadTime));
                PlayWeaponSound(_weapons[_activeWeaponID]._magazineAmmo == 0 ? _weapons[_activeWeaponID]._weaponSounds._reloadOptionalClip : _weapons[_activeWeaponID]._weaponSounds._reloadClip);
            }

            _isReloading = true;
            _canShoot = false;
            StartCoroutine(ReloadCoolDown(reloadTime));
        }
    }

    private IEnumerator ReloadCoolDown(float reloadTime)
    {
        yield return new WaitForSeconds(reloadTime);
        _weapons[_activeWeaponID].Reload();
        _isReloading = false;
    }

    private IEnumerator CanShoot(float time)
    {
        yield return new WaitForSeconds(time);
        _canShoot = true;
    }

    private IEnumerator ControlReloadOneSound(int counts, float delayTime)
    {
        int i = 0;

        while (i < counts)
        {
            PlayWeaponSound(_weapons[_activeWeaponID]._weaponSounds._reloadClip);
            yield return new WaitForSeconds(delayTime);
            i++;
        }

        PlayWeaponSound(_weapons[_activeWeaponID]._weaponSounds._reloadOptionalClip);
    }

    private void FixedUpdate()
    {
        if (_drawnWeapon)
        {
            AimWeapon();
            ShootingControl();
        }
    }

    private void ShootingControl()
    {
        if (_shootInput && !_isShooting && _weapons[_activeWeaponID]._magazineAmmo > 0 && !_isReloading && _canShoot)
        {
            if (_weapons[_activeWeaponID]._magazineAmmo == 1)
            {
                _lastShoot = true;
                PlayWeaponSound(_weapons[_activeWeaponID]._weaponSounds._lastShotClip);
            }
            else
            {
                _lastShoot = false;
                PlayWeaponSound(_weapons[_activeWeaponID]._weaponSounds._shootClip);
            }

            _weapons[_activeWeaponID].ReduceAmmo();

            BulletControl();

            StartCoroutine(SpawnBulletShell());
            StartCoroutine(ResetShooting());
            _isShooting = true;
        }
    }

    private IEnumerator ResetShooting()
    {
        yield return new WaitForSeconds(_weapons[_activeWeaponID]._shootCoolDown);
        _isShooting = false;
    }

    private IEnumerator SpawnBulletShell()
    {
        yield return new WaitForSeconds(0.1f);

        float bulletShellOutSpeed = 2f;

        GameObject spawnedBulletShell = Instantiate(_weapons[_activeWeaponID]._bulletShellPrefab, _weapons[_activeWeaponID]._bulletShellSpawnPoint.position, transform.rotation);
        spawnedBulletShell.transform.SetParent(_weapons[_activeWeaponID]._bulletShellSpawnPoint);
        Vector3 bulletShellOutForce = -_weapons[_activeWeaponID]._bulletShellSpawnPoint.right + _weapons[_activeWeaponID]._bulletShellSpawnPoint.up;
        spawnedBulletShell.GetComponent<Rigidbody>().AddForce(bulletShellOutForce * bulletShellOutSpeed, ForceMode.Impulse);
        spawnedBulletShell.GetComponent<Rigidbody>().AddTorque(new Vector3(Random.Range(50, 180), 0, Random.Range(50, 180)) * bulletShellOutSpeed);

        Destroy(spawnedBulletShell, 0.5f);
    }

    private void BulletControl()
    {
        for (int i = 0; i < _weapons[_activeWeaponID]._bulletSpawn.Length; i++)
        {
            GameObject spawnedBullet = Instantiate(_weapons[_activeWeaponID]._bulletPrefab, _weapons[_activeWeaponID]._bulletSpawn[i].position, _weapons[_activeWeaponID]._bulletSpawn[i].rotation);

            Vector3 bulletForce = _weapons[_activeWeaponID]._bulletSpawn[i].forward * _weapons[_activeWeaponID]._bulletSpeed;

            spawnedBullet.GetComponent<Rigidbody>().AddForce(bulletForce, ForceMode.Impulse);
        }
    }

    private void AimWeapon()
    {
        if (_isAiming)
            _weapons[_activeWeaponID]._weaponTransform.position = Vector3.Lerp(_weapons[_activeWeaponID]._weaponTransform.position, _weapons[_activeWeaponID]._aimingPosition.position, _weapons[_activeWeaponID]._aimingSpeed * Time.fixedDeltaTime);
    }

    private void PlayWeaponSound(Clip clip)
    {
        AudioSource weaponAudioSource = _weapons[_activeWeaponID]._weaponAudioSource.AddComponent<AudioSource>();

        weaponAudioSource.spatialBlend = 1;
        weaponAudioSource.minDistance = 0.5f;
        weaponAudioSource.maxDistance = 100f;

        weaponAudioSource.clip = clip._audioClip;

        if (clip._pitchRange != Vector2.zero)
            weaponAudioSource.pitch = Random.Range(clip._pitchRange.x, clip._pitchRange.y);

        weaponAudioSource.volume = _volumeMaster.GetVolume(clip._soundType) + (_volumeMaster.GetVolume(clip._soundType) * clip._volumeCorection);

        weaponAudioSource.Play();

        Destroy(weaponAudioSource, clip._audioClip.length);
    }

    private void InputHandle()
    {
        _isAiming = _aimingAction.ReadValue<float>() > 0.15f ? true : false;
        _shootInput = _shootAction.ReadValue<float>() > 0.15f ? true : false;
        _reloadInput = _reloadAction.ReadValue<float>() > 0.1f ? true : false;
        _switchInput = _switchAction.ReadValue<float>() > 0.1f ? true : false;
    }

    public bool GetAimingStatus()
    {
        return _isAiming;
    }

    private void DrawnWeaponControl(int weaponID)
    {
        if (_drawnWeapon)
        {
            StartCoroutine(ChangeWeapon(weaponID));
            return;
        }
        else
        {
            _canShoot = false;
            _activeWeaponID = weaponID;
            _drawnWeapon = true;

            _weapons[_activeWeaponID]._weaponTransform.gameObject.SetActive(true);
            PlayWeaponSound(_weapons[_activeWeaponID]._weaponSounds._drawnClip);
            _movementSystem.SwitchHands(_weapons[_activeWeaponID]._weaponTransform, _weapons[_activeWeaponID]._startWeaponLocalPosition);
           
            StartCoroutine(CanShoot(_weapons[_activeWeaponID]._drawnWeaponTime));

            return;
        }
    }

    private IEnumerator ChangeWeapon(int weaponID)
    {
        _drawnWeapon = false;
        _canShoot = false;

        AnimationController();

        yield return new WaitForSeconds(_weapons[_activeWeaponID]._hideWeaponTime);
        _weapons[_activeWeaponID]._weaponTransform.gameObject.SetActive(false);

        _drawnWeapon = true;
        _activeWeaponID = weaponID;

        _weapons[_activeWeaponID]._weaponTransform.gameObject.SetActive(true);
        PlayWeaponSound(_weapons[_activeWeaponID]._weaponSounds._drawnClip);
        _movementSystem.SwitchHands(_weapons[_activeWeaponID]._weaponTransform, _weapons[_activeWeaponID]._startWeaponLocalPosition);

        StartCoroutine(CanShoot(_weapons[_activeWeaponID]._drawnWeaponTime));
    }

    public void UnlockWeapon(int weaponID)
    {
        for (int i = 0; i < _weapons.Length; i++)
        {
            if(weaponID == i)
            {
                if (_weapons[i]._isUnlocked == true)
                    return;

                _weapons[i]._isUnlocked = true;

                if(!_isReloading)
                    DrawnWeaponControl(weaponID);
                return;
            }
        }
    }

    public Vector3 GetAimingLocalPosition()
    {
        return _weapons[_activeWeaponID]._aimingPosition.localPosition;
    }
}
