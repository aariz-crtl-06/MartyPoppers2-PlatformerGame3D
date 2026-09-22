using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    [SerializeField] BossAI boss;
    [SerializeField] Image fill;
    [SerializeField] GameObject barRoot;          // the whole bar, so it can be hidden
    [SerializeField] float drainSpeed = 1f;       // bar units per second (1 = full bar in 1s)

    float displayed = 1f;

    void Update()
    {
        if (fill == null) return;

        // If the boss has been destroyed, treat it as 0 health
        float target = 0f;
        if (boss != null && boss.MaxHealth > 0)
            target = Mathf.Clamp01((float)boss.Health / boss.MaxHealth);

        // Slide toward the real value so hits drain the bar instead of snapping it
        displayed = Mathf.MoveTowards(displayed, target, drainSpeed * Time.deltaTime);
        fill.fillAmount = displayed;

        // Once the boss is gone and the bar has drained, hide it
        if (boss == null && displayed <= 0f && barRoot != null)
            barRoot.SetActive(false);
    }
}