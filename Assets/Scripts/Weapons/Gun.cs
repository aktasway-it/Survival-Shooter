using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Gun : MonoBehaviour
{
    public enum FireMode { Auto, Burst, Single };

    [SerializeField]
    private FireMode _fireMode;

    [SerializeField]
    private int _bulletBurstCount;

    [SerializeField]
    private Transform[] _muzzles;

    [SerializeField]
    private Bullet _bullet;

    [SerializeField]
    private int _magazineSize = 10;

    [SerializeField]
    private float _reloadTime = 0.3f;

    [SerializeField]
    private float _msBetweenShots = 100f;

    [SerializeField]
    private float _muzzleVelocity = 35f;

    [SerializeField]
    private ParticleSystem _shellSpawner;

    [SerializeField]
    private Animator _muzzleFlash;

    [SerializeField]
    private AudioClip _shootSfx;

    [SerializeField]
    private AudioClip _reloadSfx;

    [SerializeField]
    private AudioMixerGroup _audioMixerGroup;

    private Animator _animator;
    private float _nextShotTime;
    private int _bulletsShot;
    private bool _triggerPulled;
    private int _magazineRemainingBullets;
    private bool _isReloading;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _magazineRemainingBullets = _magazineSize;
    }

    private void Update()
    {
        if (_triggerPulled)
        {
            if (CanShoot())
                Shoot();
            else if (!HasBullets())
                Reload();
        }
    }

    private void Shoot()
    {
        for (int i = 0; i < _muzzles.Length; i++)
        {
            _bulletsShot++;
            _magazineRemainingBullets--;

            _nextShotTime = Time.time + _msBetweenShots / 1000;
            Bullet bullet = BulletSpawner.Instance.Get();
            bullet.transform.SetPositionAndRotation(_muzzles[i].position, _muzzles[i].rotation);
            bullet.Speed = _muzzleVelocity;

            _shellSpawner.Emit(1);
        }

        _animator.SetTrigger("Shoot");
        _muzzleFlash.SetTrigger("Flash");
        AudioManager.Instance.Play(_shootSfx, audioMixerGroup: _audioMixerGroup, position: transform.position);
    }

    public void Reload()
    {
        if (_isReloading || _magazineRemainingBullets == _magazineSize)
            return;

        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        _isReloading = true;
        yield return new WaitForSeconds(0.2f);

        float reloadSpeed = 1 / _reloadTime;
        float percent = 0;
        Vector3 initialRotation = transform.localEulerAngles;

        AudioManager.Instance.Play(_reloadSfx, audioMixerGroup: _audioMixerGroup, position: transform.position);

        while (percent < 1)
        {
            percent += Time.deltaTime * reloadSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            float reloadAngle = Mathf.Lerp(0, -80, interpolation);

            transform.localEulerAngles = new Vector3(reloadAngle, 0, 0);
            yield return null;
        }

        transform.localEulerAngles = initialRotation;
        _isReloading = false;
        _magazineRemainingBullets = _magazineSize;
    }

    public void OnTriggerHold()
    {
        _triggerPulled = true;
    }

    public void OnTriggerRelease()
    {
        _triggerPulled = false;
        _bulletsShot = 0;
    }

    private bool HasBullets()
    {
        return _magazineRemainingBullets > 0;
    }

    private bool CanShoot()
    {
        bool canShoot = Time.time > _nextShotTime;
        canShoot &= HasBullets();
        canShoot &= _fireMode == FireMode.Auto || (_fireMode == FireMode.Burst && _bulletsShot < _bulletBurstCount) || (_fireMode == FireMode.Single && _bulletsShot == 0);
        return canShoot;
    }

}
