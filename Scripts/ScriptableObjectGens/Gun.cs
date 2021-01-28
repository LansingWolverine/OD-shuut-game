using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

[CreateAssetMenu(fileName = "New Gun", menuName = "Gun")]
public class Gun : ScriptableObject
{
    public string name;

    public GameObject prefab;

    public float damage; // damage per pellet
    public float range; // how far a pellet can go
    public float fireRate; // how fast the gun fires per second
    public int pellets; // the amount of pellets sent

    public bool projectile; // if checked, shoots projectile
    public int fireType; // 0 semi | 1 auto | 2 burst

    public float impactForce;
    public float recoil;
    public float kickback;
    public float bloom; // weapon spread

    public AudioClip gunshotSound;
    public float pitchRandomization;
    public float shotVolume;

    public bool recovery; //if the weapon has a inbetween shot animation

    public AudioClip reloadSound;
    public AudioClip reloadSound2;

    public float reloadTime;
    public int reloadType; // 0 clip | 1 sequential | 2 manual | 3 no reload

    public float secondReloadTime; // If sequential, time each reload after the first occurs. Otherwise, leave as 0 

    public int maxAmmo; //Bullets in total
    public int maxClipAmmo; //Bullets in full magazine

    public int stashAmmo; //Bullets left in stash
    public int ammoLeft; //Bullets left in clip

    public AudioClip emptyClick;

    public void Initialize()
    {
        if (reloadType != 2)
        {
            ammoLeft = maxClipAmmo;
            stashAmmo = maxAmmo;
        }

    }

    public bool FireBullet()
    {
        if (ammoLeft > 0)
        {
            ammoLeft--;
            return true;
        }
        else return false; 
    }

    public void Reload()
    {
        int depletedAmmo = maxClipAmmo - ammoLeft;

        if (depletedAmmo < stashAmmo)
        {
            stashAmmo -= depletedAmmo;
            ammoLeft += depletedAmmo;
            depletedAmmo = 0;
        }

        else if (depletedAmmo >= stashAmmo)
        {
            ammoLeft += stashAmmo;
            stashAmmo = 0;
            depletedAmmo = 0;
        } 

    }

    public void SingleReload()
    {
        ammoLeft += 1;  
        stashAmmo -= 1;
    }

    public int GetStash() { return stashAmmo;  }
    public int GetClip() { return ammoLeft;  }
}