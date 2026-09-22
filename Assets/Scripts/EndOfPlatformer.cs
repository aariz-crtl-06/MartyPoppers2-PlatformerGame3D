using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class EndOfPlatformer : MonoBehaviour
{
    public string nextSceneName = "BossCutscene";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
