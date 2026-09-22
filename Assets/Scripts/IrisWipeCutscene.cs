using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class IrisWipeCutscene : MonoBehaviour
{
    [Header("References")]
    public Image irisImage;

    [Header("Settings")]
    public float wipeDuration = 1f;
    public float waitBeforeSceneLoad = 5f;
    public string mainMenuSceneName = "MainMenu";

    private Material irisMaterial;

    void Start()
    {
        irisMaterial = irisImage.material; // instance, not the shared asset
        irisMaterial.SetFloat("_AspectRatio", (float)Screen.width / Screen.height);
        StartCoroutine(CloseIris());
    }

    private IEnumerator CloseIris()
    {
        float startRadius = 0.75f;
        float elapsed = 0f;

        while (elapsed < wipeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / wipeDuration;
            float radius = Mathf.Lerp(startRadius, 0f, t);
            irisMaterial.SetFloat("_Radius", radius);
            yield return null;
        }

        irisMaterial.SetFloat("_Radius", 0f); // fully closed, all black

        yield return new WaitForSeconds(waitBeforeSceneLoad);

        SceneManager.LoadScene(mainMenuSceneName);
    }
}