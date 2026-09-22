using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroScene : MonoBehaviour
{
    [Header("References")]
    public GameObject marty;
    public GameObject umbrella;
    public GameObject portal;
    public Image fadeImage;
    public Camera cam1;
    public Camera cam2;
    public GameObject buttons;
    public Animator animator;
    public AudioSource floatSound;
    public GameObject creditsPanel;
    public GameObject quitPanel;

    [Header("Settings")]
    public string gameplaySceneName = "GameplayScene";
    public float portalPopDuration = 0.3f;
    public float suckInDuration = 1.5f;
    public float cameraSwitchPoint = 0.4f; // 0-1, fraction of suck-in when camera cuts
    public float fadeDuration = 1f;

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    void Update()
    {

    }

    public void StartGame()
    {
        StartCoroutine(IntroSceneSequence());
        buttons.SetActive(false); // hide buttons during cutscene
    }

    public void ShowCredits()
    {
        creditsPanel.SetActive(true);
    }

    public void HideCredits()
    {
        creditsPanel.SetActive(false);
    }

    public void QuitPanel()
    {
        quitPanel.SetActive(true);
    }
    
    public void HideQuitPanel()
    {
        quitPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private IEnumerator IntroSceneSequence()
    {
        // 0. Make sure cam1 is active, cam2 off, portal off, marty/umbrella idle
        cam1.gameObject.SetActive(true);
        cam2.gameObject.SetActive(false);
        portal.SetActive(false);

        // 1. Idle for a few seconds
        yield return new WaitForSeconds(4f);

        // 2. Portal pops in, scaling from tiny to full size fast
        Vector3 portalFullScale = portal.transform.localScale; // grab its real scale first
        portal.SetActive(true);
        portal.transform.localScale = Vector3.zero;

        float popElapsed = 0f;
        while (popElapsed < portalPopDuration)
        {
            popElapsed += Time.deltaTime;
            float t = popElapsed / portalPopDuration;
            portal.transform.localScale = Vector3.Lerp(Vector3.zero, portalFullScale, t);
            yield return null;
        }
        portal.transform.localScale = portalFullScale;

        // 3. Suck marty and umbrella into the portal
        Vector3 martyStartPos = marty.transform.position;
        Vector3 martyStartScale = marty.transform.localScale;
        Vector3 umbrellaStartPos = umbrella.transform.position;
        Vector3 umbrellaStartScale = umbrella.transform.localScale;
        Vector3 portalPos = portal.transform.position;

        // float in place for 3 seconds, rising up off the couch, before the pull/shrink/rotate kicks in
        animator.SetBool("isFloating", true);
        floatSound.PlayOneShot(floatSound.clip); // play the float sound effect

        float floatUpAmount = 2f; // how high marty rises, tweak to taste
        Vector3 martyFloatTarget = martyStartPos + Vector3.up * floatUpAmount;
        Vector3 umbrellaFloatTarget = umbrellaStartPos + Vector3.up * floatUpAmount;

        float floatElapsed = 0f;
        float floatDuration = 3f;
        while (floatElapsed < floatDuration)
        {
            floatElapsed += Time.deltaTime;
            float floatT = floatElapsed / floatDuration;

            marty.transform.position = Vector3.Lerp(martyStartPos, martyFloatTarget, floatT);
            umbrella.transform.position = Vector3.Lerp(umbrellaStartPos, umbrellaFloatTarget, floatT);

            yield return null;
        }

        bool hasSwitchedCamera = false;
        float elapsed = 0f;

        martyStartPos = martyFloatTarget;
        umbrellaStartPos = umbrellaFloatTarget;

        while (elapsed < suckInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / suckInDuration;
            float easedT = t * t;

            marty.transform.position = Vector3.Lerp(martyStartPos, portalPos, easedT);
            marty.transform.localScale = Vector3.Lerp(martyStartScale, Vector3.zero, easedT);
            marty.transform.Rotate(Vector3.up, 360f * Time.deltaTime, Space.Self);

            umbrella.transform.position = Vector3.Lerp(umbrellaStartPos, portalPos, easedT);
            umbrella.transform.localScale = Vector3.Lerp(umbrellaStartScale, Vector3.zero, easedT);
            if (!hasSwitchedCamera && t >= cameraSwitchPoint)
            {
                cam1.gameObject.SetActive(false);
                cam2.gameObject.SetActive(true);
                hasSwitchedCamera = true;
            }

            yield return null;
        }

        marty.SetActive(false);
        umbrella.SetActive(false);

        // 4. Fade to black
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

        // 5. Load gameplay scene
        SceneManager.LoadScene(gameplaySceneName);
    }
}