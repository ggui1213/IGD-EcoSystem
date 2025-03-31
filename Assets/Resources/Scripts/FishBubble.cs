using System.Collections;
using UnityEngine;

public class FishBubble : MonoBehaviour
{
    public AudioClip Bubble;        // Assign via Inspector
    public float minInterval = 3f;       // Minimum time between sounds
    public float maxInterval = 10f;      // Maximum time between sounds

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        StartCoroutine(PlaySoundRandomly());
    }

    private IEnumerator PlaySoundRandomly()
    {
        while (true)
        {
            float waitTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(waitTime);
        
            // Set a random pitch (e.g., between 0.8 and 1.2)
            audioSource.pitch = Random.Range(0.8f, 1.2f);
        
            // Play the sound with the random pitch.
            audioSource.PlayOneShot(Bubble);
        }
    }
}
