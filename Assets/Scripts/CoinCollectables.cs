using UnityEngine;

public class CoinCollectables : MonoBehaviour
{
    public static int coinCount = 0;
    public static int coinTotal = 120;

    AudioSource audioSource;
    bool collected = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (collected || !other.CompareTag("Player")) return;
        collected = true;

        coinCount++;
        Debug.Log("Coin Collected! Total Coins: " + coinCount + "/" + coinTotal);

        // Hide the coin and stop it from triggering again
        GetComponent<Collider>().enabled = false;
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        // Play the sound, then destroy the coin once it has finished
        audioSource.Play();
        Destroy(gameObject, audioSource.clip.length);
    }
}