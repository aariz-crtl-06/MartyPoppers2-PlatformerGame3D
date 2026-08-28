using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EquippableItem
{
    public string itemName;
    public GameObject itemObject;
    [HideInInspector] public Vector3 fullScale;
    [HideInInspector] public Coroutine scaleRoutine;
}

public class PlayerEquipment : MonoBehaviour
{
    public List<EquippableItem> items = new List<EquippableItem>();
    public float growShrinkDuration = 0.3f;

    private Dictionary<string, EquippableItem> lookup;

    void Awake()
    {
        lookup = new Dictionary<string, EquippableItem>();
        foreach (var item in items)
        {
            if (item.itemObject == null) continue;
            item.fullScale = item.itemObject.transform.localScale;
            item.itemObject.transform.localScale = Vector3.zero;
            item.itemObject.SetActive(false);
            lookup[item.itemName] = item;
        }
    }

    public void ShowItem(string name)
    {
        if (!lookup.TryGetValue(name, out var item) || item.itemObject == null) return;
        item.itemObject.SetActive(true);
        if (item.scaleRoutine != null) StopCoroutine(item.scaleRoutine);
        item.scaleRoutine = StartCoroutine(ScaleTo(item, item.fullScale, false));
    }

    public void HideItem(string name)
    {
        if (!lookup.TryGetValue(name, out var item) || item.itemObject == null) return;
        if (item.scaleRoutine != null) StopCoroutine(item.scaleRoutine);
        item.scaleRoutine = StartCoroutine(ScaleTo(item, Vector3.zero, true));
    }

    private IEnumerator ScaleTo(EquippableItem item, Vector3 target, bool disableAfter)
    {
        Vector3 start = item.itemObject.transform.localScale;
        float t = 0f;
        while (t < growShrinkDuration)
        {
            t += Time.deltaTime;
            float n = 1f - Mathf.Pow(1f - Mathf.Clamp01(t / growShrinkDuration), 3f);
            item.itemObject.transform.localScale = Vector3.Lerp(start, target, n);
            yield return null;
        }
        item.itemObject.transform.localScale = target;
        if (disableAfter) item.itemObject.SetActive(false);
    }
}