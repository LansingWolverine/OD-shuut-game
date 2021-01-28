using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Weapon : MonoBehaviourPunCallbacks
{
    public Gun[] loadout;
    [HideInInspector] public Gun currentGunData;

    public Transform weaponParent;
    public GameObject bulletholePrefab;
    public LayerMask canBeShot;
    public AudioSource sfx;

    private float currentCooldown;
    private int currentIndex;
    private GameObject currentWeapon;

    private Image hitmarkerImage;
    private float hitmarkerWait;
    public AudioClip hitmarkerSound;

    public bool isShooting;
    public bool isReloading;

    private void Start()
    {
        foreach (Gun a in loadout) a.Initialize();
        hitmarkerImage = GameObject.Find("HUD/Hitmarker/Image").GetComponent<Image>();
        hitmarkerImage.color = new Color(1, 1, 1, 0);
        Equip(0);
    }

    void Update()
    {

        if (Pause.paused && photonView.IsMine) return;

        if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha1)) { photonView.RPC("Equip", RpcTarget.All, 0); }

        if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha2)) { photonView.RPC("Equip", RpcTarget.All, 1); }

        if (photonView.IsMine && Input.GetKeyDown(KeyCode.Alpha3)) { photonView.RPC("Equip", RpcTarget.All, 2); }



        if (currentWeapon != null)
        {
            if (photonView.IsMine)
            {
                if (loadout[currentIndex].fireType != 1)
                {
                    if (Input.GetMouseButtonDown(0) && currentCooldown <= 0)
                    {
                        if (isReloading) return;
                        else if (loadout[currentIndex].FireBullet()) photonView.RPC("Shoot", RpcTarget.All);
                        else
                        {
                            sfx.clip = currentGunData.emptyClick;
                            sfx.Play();
                            StartCoroutine(Reload(loadout[currentIndex].reloadTime, loadout[currentIndex].secondReloadTime));
                        }
                    }
                }

                else
                {
                    if (Input.GetMouseButton(0) && currentCooldown <= 0)
                    {
                        if (isReloading) return;
                        else if (loadout[currentIndex].FireBullet()) photonView.RPC("Shoot", RpcTarget.All);
                        else
                        {
                            sfx.clip = currentGunData.emptyClick;
                            sfx.Play();
                            StartCoroutine(Reload(loadout[currentIndex].reloadTime, loadout[currentIndex].secondReloadTime));
                        }
                    }
                }

                if (Input.GetKeyDown(KeyCode.R))
                {
                    if (loadout[currentIndex].ammoLeft == loadout[currentIndex].maxClipAmmo) return;
                    else if (isReloading) return;
                    else StartCoroutine(Reload(loadout[currentIndex].reloadTime, loadout[currentIndex].secondReloadTime));
                }

                //cooldown
                if (currentCooldown > 0) currentCooldown -= Time.deltaTime;
            }

            currentWeapon.transform.localRotation = Quaternion.Lerp(currentWeapon.transform.localRotation, Quaternion.identity, Time.deltaTime * 4f);
        }

        if(photonView.IsMine)
        {
            if(hitmarkerWait > 0) hitmarkerWait -= Time.deltaTime;

            else hitmarkerImage.color = Color.Lerp(hitmarkerImage.color, new Color(1, 1, 1, 0), Time.deltaTime * .5f);
        }
    }

    IEnumerator Reload(float p_wait, float t_wait)
    {
        isReloading = true;

        if (loadout[currentIndex].reloadType == 0)
        {
            if (currentWeapon.GetComponent<Animator>())
                currentWeapon.GetComponent<Animator>().Play("Reload", 0, 0);
            else
                currentWeapon.SetActive(false);

            sfx.clip = currentGunData.reloadSound;
            sfx.Play();

            yield return new WaitForSeconds(p_wait);

            loadout[currentIndex].Reload();

            currentWeapon.SetActive(true);

            isReloading = false;

        }

        else if (loadout[currentIndex].reloadType == 1)
        {
            if (currentWeapon.GetComponent<Animator>())
                currentWeapon.GetComponent<Animator>().Play("ReloadStart", 0, 0);
            else
                currentWeapon.SetActive(false);
            
            yield return new WaitForSeconds(p_wait);

            sfx.clip = currentGunData.reloadSound;
            sfx.Play();

            loadout[currentIndex].SingleReload();

            int i = loadout[currentIndex].ammoLeft;
            int j = loadout[currentIndex].maxClipAmmo;

            isReloading = false;

            while (i != j && !isShooting)
            {
                currentWeapon.GetComponent<Animator>().Play("Reload", 0, 0);

                sfx.clip = currentGunData.reloadSound;
                sfx.Play();

                yield return new WaitForSeconds(t_wait);

                loadout[currentIndex].SingleReload();

                i++;

            }

            currentWeapon.GetComponent<Animator>().Play("ReloadEnd", 0, 0);

            sfx.clip = currentGunData.reloadSound;

            sfx.Play();

        }

        isReloading = false;

    }


    public void RefreshStash(Text p_text)
    {
        int t_stash = loadout[currentIndex].GetStash();
        p_text.text = t_stash.ToString();
    }

    public void RefreshClip(Text p_text)
    {
        int t_clip = loadout[currentIndex].GetClip();
        p_text.text = t_clip.ToString();
    }

    [PunRPC]
    void Equip(int p_ind)
    {
        if (currentWeapon != null)
        {
            StopCoroutine("Reload");
            Destroy(currentWeapon);
        }

        currentIndex = p_ind;

        GameObject t_newWeapon = Instantiate(loadout[p_ind].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
        t_newWeapon.transform.localPosition = Vector3.zero;
        t_newWeapon.transform.localEulerAngles = Vector3.zero;

        t_newWeapon.GetComponent<Animator>().Play("Equip", 0, 0);

        currentWeapon = t_newWeapon;
        currentGunData = loadout[p_ind];

    }

    [PunRPC]
    void Shoot()
    {
        StopCoroutine("Reload");

        isShooting = true;

        Transform t_spawn = transform.Find("Cameras/FP Camera");

        //cooldown
        currentCooldown = loadout[currentIndex].fireRate;

        for (int i = 0; i < Mathf.Max(1, currentGunData.pellets); i++)
        {
            //bloom
            Vector3 t_bloom = t_spawn.position + t_spawn.forward * 1000f;
            t_bloom += Random.Range(-loadout[currentIndex].bloom, loadout[currentIndex].bloom) * t_spawn.up;
            t_bloom += Random.Range(-loadout[currentIndex].bloom, loadout[currentIndex].bloom) * t_spawn.right;
            t_bloom -= t_spawn.position;
            t_bloom.Normalize();

            //raycast
            RaycastHit t_hit = new RaycastHit();
            if (Physics.Raycast(t_spawn.position, t_bloom, out t_hit, 100f, canBeShot))
            {
                GameObject t_newHole = Instantiate(bulletholePrefab, t_hit.point + t_hit.normal * 0.001f, Quaternion.identity) as GameObject;
                t_newHole.transform.LookAt(t_hit.point + t_hit.normal);
                Destroy(t_newHole, .6f);

                if (photonView.IsMine)
                {
                    if (t_hit.collider.gameObject.layer == 12)
                    {
                        t_hit.collider.transform.root.gameObject.GetPhotonView().RPC("TakeDamage", RpcTarget.All, loadout[currentIndex].damage);
                        //RPC to Damage Player

                        hitmarkerImage.color = Color.white;
                        sfx.PlayOneShot(hitmarkerSound);
                        hitmarkerWait = 0.5f;
                    }
                }
            }
        }

        sfx.clip = currentGunData.gunshotSound;
        sfx.pitch = 1 - currentGunData.pitchRandomization + Random.Range(-currentGunData.pitchRandomization, currentGunData.pitchRandomization);
        sfx.volume = currentGunData.shotVolume;
        sfx.Play();

        currentWeapon.transform.Rotate(-loadout[currentIndex].recoil, 0, 0);
        currentWeapon.transform.position -= currentWeapon.transform.forward * loadout[currentIndex].kickback;


        if (currentGunData.recovery) currentWeapon.GetComponent<Animator>().Play("Recovery", 0, 0);

        isShooting = false;
    }

    [PunRPC]
    private void TakeDamage(int p_damage)
    {
        GetComponent<CPMPlayer>().TakeDamage(p_damage);
    }

}
