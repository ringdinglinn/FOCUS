using UnityEngine;
using UnityEngine.EventSystems;

public class RestartLevel : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(gameObject.scene.buildIndex, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
