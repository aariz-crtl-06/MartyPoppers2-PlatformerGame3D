using UnityEngine;
using System.Collections;
public class PlayerEquipment : MonoBehaviour
{
    public GameObject umbrella;
    public float growShrinkDuration = 0.4f;

    private Vector3 umbrellaFullScale;
    private Coroutine scaleRoutine;

    void Awake()
    {
        if (umbrella != null)
        {
            umbrellaFullScale = umbrella.transform.localScale;
            umbrella.transform.localScale = Vector3.zero;
            umbrella.SetActive(false);
        }
    }

    public void ShowUmbrella()
    {
        if (umbrella == null) return;
        umbrella.SetActive(true);
        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        scaleRoutine = StartCoroutine(ScaleTo(umbrellaFullScale, disableAfter: false));
    }

    public void HideUmbrella()
    {
        if (umbrella == null) return;
        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        scaleRoutine = StartCoroutine(ScaleTo(Vector3.zero, disableAfter: true));
    }

    private IEnumerator ScaleTo(Vector3 target, bool disableAfter)
    {
        Vector3 start = umbrella.transform.localScale;
        float t = 0f;
        while (t < growShrinkDuration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / growShrinkDuration);
            normalized = 1f - Mathf.Pow(1f - normalized, 3f); // ease-out, feels more like a "pop"
            umbrella.transform.localScale = Vector3.Lerp(start, target, normalized);
            yield return null;
        }
        umbrella.transform.localScale = target;
        if (disableAfter) umbrella.SetActive(false);
    }
}
