using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PhysicsToggle : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    Text _title;

    private void Awake()
    {
        Time.timeScale = 1f;
        TogglePhysics();
    }

    public void TogglePhysics()
    {
        if (Time.timeScale == 1f)
        {
            Time.timeScale = 0f;
            _title.text = "START";
        }
        else
        {
            Time.timeScale = 1f;
            _title.text = "PAUSE";
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        TogglePhysics();
    }
}
