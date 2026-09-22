using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class CameraZoomIn : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public Camera cam2;
    public Transform target; // the character to zoom toward
    public GameObject objectToActivate;
    public Image fadeImage; // full-screen white Image, alpha starts at 0
    public AudioSource zoom;
    public AudioSource power;

    [Header("Settings")]
    public float waitBeforeZoom = 3f;
    public float zoomDuration = 0.8f;
    public float waitBeforeCameraCut = 2f;
    public float waitBeforeFade = 1f;
    public float fadeDuration = 1f;
    public string nextSceneName = "BossFight";
    public Vector3 targetOffset = new Vector3(0f, 1.6f, 1f); // offset from character so cam stops near their face, not inside them

    void Start()
    {
        StartCoroutine(ZoomSequence());
    }

    private IEnumerator ZoomSequence()
    {
        yield return new WaitForSeconds(waitBeforeZoom);

        Vector3 startPos = cam.transform.position;
        Vector3 endPos = target.position + targetOffset;

        float elapsed = 0f;
        zoom.PlayOneShot(zoom.clip);
        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / zoomDuration;
            float easedT = 1f - Mathf.Pow(1f - t, 3f); // ease-out, fast start then settles

            cam.transform.position = Vector3.Lerp(startPos, endPos, easedT);

            yield return null;
        }

        cam.transform.position = endPos;

        // wait, then cut to camera 2
        yield return new WaitForSeconds(waitBeforeCameraCut);

        cam.gameObject.SetActive(false);
        cam2.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        // activate the object
        power.PlayOneShot(power.clip);
        objectToActivate.SetActive(true);

        // wait, then fade to white
        yield return new WaitForSeconds(waitBeforeFade);

        fadeImage.gameObject.SetActive(true);
        Color c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;

        float fadeElapsed = 0f;
        while (fadeElapsed < fadeDuration)
        {
            fadeElapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(fadeElapsed / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }

        // load the boss fight scene
        SceneManager.LoadScene(nextSceneName);
    }
}