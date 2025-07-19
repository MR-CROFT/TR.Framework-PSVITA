using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerSFX : MonoBehaviour
{
    private AudioSource playerSource;

    [Header("Properties")]
    public float footMinVol = 0.22f;
    public float footMaxVol = 0.3f;

    [Header("Sound Files")]
    public AudioClip[] feetSounds;
    public AudioClip[] jumpSounds;
    public AudioClip[] grabSounds;
    public AudioClip[] vaultSounds;
    public AudioClip[] screamSounds;
    public AudioClip[] slapSounds;
    public AudioClip[] hitGroundSounds;
    public AudioClip[] deathSounds;
    public AudioClip[] shootSounds; // Adicionado: Som dos disparos

    private void Start()
    {
        playerSource = GetComponent<AudioSource>();
    }

    public void PlayFootSound()
    {
        PlayRandomSound(feetSounds, Random.Range(footMinVol, footMaxVol));
    }

    public void PlayJumpSound()
    {
        PlayRandomSound(jumpSounds, 1);
    }

    public void PlayGrabSound()
    {
        PlayRandomSound(grabSounds, 1);
    }

    public void PlayHitGroundSound()
    {
        PlayRandomSound(hitGroundSounds, .5f);
    }

    public void PlayVaultSound()
    {
        PlayRandomSound(vaultSounds, 1);
    }

    public void PlayScreamSound()
    {
        PlayRandomSound(screamSounds, 1);
    }

    public void PlaySlapSounds()
    {
        PlayRandomSound(slapSounds, 0.25f);
    }

    public void PlayDeathSounds()
    {
        PlayRandomSound(deathSounds, 0.6f);
    }

    public void PlayShootSound() // Adicionado: Método para tocar som de disparo
    {
        PlayRandomSound(shootSounds, 1);
    }

    private void PlayRandomSound(AudioClip[] sounds, float volume)
    {
        if (sounds.Length == 0) return;
        int random = Random.Range(0, sounds.Length);
        playerSource.PlayOneShot(sounds[random], volume);
    }
}
