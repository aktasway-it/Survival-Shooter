using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunController : MonoBehaviour
{
    [SerializeField]
    private Transform _weaponHold;
    [SerializeField]
    private Gun _startingGun;

    private Gun _equippedGun;

    private void Start()
    {
        if (_startingGun != null)
            EquipGun(_startingGun);
    }

    public void EquipGun(Gun gun)
    {
        if (_equippedGun != null)
            Destroy(_equippedGun.gameObject);

        _equippedGun = Instantiate(gun, _weaponHold.position, _weaponHold.rotation) as Gun;
        _equippedGun.transform.parent = _weaponHold;
    }

    public void OnTriggerHold()
    {
        if (_equippedGun != null)
            _equippedGun.OnTriggerHold();
    }

    public void OnTriggerRelease()
    {
        if (_equippedGun != null)
            _equippedGun.OnTriggerRelease();
    }
}
