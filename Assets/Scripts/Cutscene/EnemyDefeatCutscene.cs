using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class EnemyDefeatCutscene : MonoBehaviour
{
    [Header("References")]
    public Camera cam1; // enemy camera
    public Camera cam2; // main character camera
    public GameObject enemyDisappearEffect; // object that becomes active when enemy disappears
    public GameObject enemy;
    public GameObject mainCharacter;
    public Animator mainCharacterAnimator;
    public AudioSource thumpSound;
    public AudioSource explosionSound;
    public AudioSource applaudSound;
    public AudioSource endMusicSound;

    [Header("Settings")]
    public float waitBeforeDisappear = 3f;
    public float waitForExplosion = 1.5f; // time for the particle effect to play before cutting
    public float jumpHeight = 2f;
    public float jumpDuration = 0.5f;

    [Header("Iris Wipe")]
    public Image irisImage;
    public float wipeDuration = 1f;
    public float waitBeforeSceneLoad = 5f;
    public string mainMenuSceneName = "MainMenu";

    private Material irisMaterial;

    void Start()
    {
        StartCoroutine(CutsceneSequence());
    }

    private IEnumerator CutsceneSequence()
    {
        // 1. enemy stands still for 3 seconds
        yield return new WaitForSeconds(1f);
        thumpSound.PlayOneShot(thumpSound.clip);
        yield return new WaitForSeconds(1f);
        explosionSound.PlayOneShot(explosionSound.clip);
        yield return new WaitForSeconds(1f);



        // 2. object becomes active, enemy disappears
        enemyDisappearEffect.SetActive(true);
        
        enemy.SetActive(false);

        // wait for the explosion effect to play out
        yield return new WaitForSeconds(waitForExplosion);

        applaudSound.PlayOneShot(applaudSound.clip);
        endMusicSound.PlayOneShot(endMusicSound.clip);

        // 3. cut to camera 2
        cam1.gameObject.SetActive(false);
        cam2.gameObject.SetActive(true);

        // 4. main character jumps up and freezes mid-air

        Vector3 startPos = mainCharacter.transform.position;
        Vector3 jumpTarget = startPos + Vector3.up * jumpHeight;

        mainCharacterAnimator.SetBool("isCheering", true); // trigger cheering

        float elapsed = 0f;
        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / jumpDuration;
            float easedT = 1f - Mathf.Pow(1f - t, 3f); // ease-out, fast rise then settles

            mainCharacter.transform.position = Vector3.Lerp(startPos, jumpTarget, easedT);

            yield return null;
        }

        mainCharacter.transform.position = jumpTarget;
        // he now stays here, frozen
        
        // iris wipe closes in
        irisMaterial = new Material(irisImage.material);
        irisImage.material = irisMaterial;
        irisMaterial.SetFloat("_AspectRatio", (float)Screen.width / Screen.height);

        float startRadius = 0.75f;
        float irisElapsed = 0f;

        while (irisElapsed < wipeDuration)
        {
            irisElapsed += Time.deltaTime;
            float t = irisElapsed / wipeDuration;
            float radius = Mathf.Lerp(startRadius, 0f, t);
            irisMaterial.SetFloat("_Radius", radius);
            yield return null;
        }

        irisMaterial.SetFloat("_Radius", 0f);

        yield return new WaitForSeconds(waitBeforeSceneLoad);

        SceneManager.LoadScene(mainMenuSceneName);
    }
}