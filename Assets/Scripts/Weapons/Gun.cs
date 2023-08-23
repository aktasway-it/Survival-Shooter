using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField]
    private Transform _muzzle;
    [SerializeField]
    private Bullet _bullet;
    [SerializeField]
    private float _msBetweenShots = 100f;
    [SerializeField]
    private float _muzzleVelocity = 35f;

    private float _nextShotTime;

    public void Shoot()
    {
        if (Time.time > _nextShotTime)
        {
            _nextShotTime = Time.time + _msBetweenShots / 1000;
            Bullet bullet = BulletSpawner.Instance.Get();
            bullet.transform.position =_muzzle.position;
            bullet.transform.rotation = _muzzle.rotation;
            bullet.Speed = _muzzleVelocity;
        }
    }

}
