using UnityEngine;

public class Actor_ToggleGameObject : MonoBehaviour
{
    public GameObject Target;

    public void Activate(GameObject sender) => Target.SetActive(true);
    public void Deactivate(GameObject sender) => Target.SetActive(false);
    public void Toggle(GameObject sender) => Target.SetActive(!Target.activeSelf);
}