using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public AudioClip[] soundEffects;
    public AudioClip[] backgroundMusic;

    private AudioSource audioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        PlayBackgroundMusic(0);
    }

    public void PlaySoundEffect(int index, float volume = 1.0f)
    {
        if (index >= 0 && index < soundEffects.Length)
        {
            AudioSource.PlayClipAtPoint(soundEffects[index], Camera.main.transform.position, volume);
        }
        else
        {
            Debug.LogWarning("Sound effect index out of range.");
        }
    }

    public void PlayBackgroundMusic(int index, bool loop = true, float volume = 1.0f)
    {
        if (index >= 0 && index < backgroundMusic.Length)
        {
            audioSource.clip = backgroundMusic[index];
            audioSource.loop = loop;
            audioSource.volume = volume;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("Background music index out of range.");
        }
    }

    public void PlaySoundBtn(int index)
    {
        PlaySoundEffect(index);
    }
}
