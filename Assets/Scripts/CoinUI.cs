using UnityEngine;
using TMPro;

public class CoinUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI coinText;

    public GameObject crown;
    public GameObject glasses;
    public GameObject confetti;
    public AudioSource victory;

    private bool victoryTriggered = false;

    void Awake()
    {
        if (coinText == null)
            coinText = GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        CoinCollectables.coinCount = 0;
    }

    void Update()
    {
        coinText.text = "Juice Boxes: " + CoinCollectables.coinCount + " / " + CoinCollectables.coinTotal;

        if (!victoryTriggered && CoinCollectables.coinCount >= CoinCollectables.coinTotal)
        {
            victoryTriggered = true;
            crown.SetActive(true);
            glasses.SetActive(true);
            confetti.SetActive(true);
            victory.PlayOneShot(victory.clip);
        }
    }
}