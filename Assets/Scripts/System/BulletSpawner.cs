using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BulletSpawner : MonoBehaviour
{
    public static BulletSpawner Instance { get; private set; }

    [SerializeField]
    private Bullet _bulletPrefab;
    
    [SerializeField]
    private int _bulletPoolPreloadSize = 50;
    
    [SerializeField]
    private int _bulletPoolSize = 100;

    private ObjectPool<Bullet> _bulletPool;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _bulletPool = new ObjectPool<Bullet>(CreateBullet, GetBullet, ReturnBullet, DestroyBullet, true, _bulletPoolSize);
        for (int i = 0; i < _bulletPoolPreloadSize; i++)
        {
            CreateBullet();
        }
    }

    public Bullet Get()
    {
        return _bulletPool.Get();
    }

    public void Return(Bullet bullet)
    {
        _bulletPool.Release(bullet);
    }

    private Bullet CreateBullet()
    {
        Bullet bullet = Instantiate(_bulletPrefab);
        bullet.gameObject.SetActive(false);
        bullet.transform.parent = transform;
        return bullet;
    }

    private void GetBullet(Bullet bullet)
    {
        bullet.gameObject.SetActive(true);
    }

    private void ReturnBullet(Bullet bullet)
    {
        bullet.gameObject.SetActive(false);
    }

    private void DestroyBullet(Bullet bullet)
    {
        Destroy(bullet.gameObject);
    }
}
