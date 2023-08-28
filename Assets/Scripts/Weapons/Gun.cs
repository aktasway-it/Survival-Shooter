using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private float _msBetweenShots = 100f;

    [SerializeField]
    private float _muzzleVelocity = 35f;

    [SerializeField]
    private ParticleSystem _shellSpawner;

    [SerializeField]
    private Animator _muzzleFlash;

    private float _nextShotTime;
    private int _bulletsShot;
    private bool _triggerPulled;

    private void Update()
    {
        if (_triggerPulled && CanShoot())
            Shoot();
    }

    private void Shoot()
    {
        for (int i = 0; i < _muzzles.Length; i++)
        {
            _bulletsShot++;
            _nextShotTime = Time.time + _msBetweenShots / 1000;
            Bullet bullet = BulletSpawner.Instance.Get();
            bullet.transform.SetPositionAndRotation(_muzzles[i].position, _muzzles[i].rotation);
            bullet.Speed = _muzzleVelocity;

            _shellSpawner.Emit(1);
            _muzzleFlash.SetTrigger("Flash");
        }
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

    private bool CanShoot()
    {
        bool canShoot = Time.time > _nextShotTime;
        canShoot &= _fireMode == FireMode.Auto || (_fireMode == FireMode.Burst && _bulletsShot < _bulletBurstCount) || (_fireMode == FireMode.Single && _bulletsShot == 0);
        return canShoot;
    }

}
