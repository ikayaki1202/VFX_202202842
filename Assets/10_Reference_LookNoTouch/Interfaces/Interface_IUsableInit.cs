using UnityEngine;

public interface IUsableInit
{
    void OnEquipped(GameObject user);
    void OnUnequipped(GameObject user);
}