using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Actor_PlaySound : MonoBehaviour
{
    public AudioClip Clip;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Play(GameObject sender)
    {
        if (Clip != null) audioSource.PlayOneShot(Clip);
    }
}