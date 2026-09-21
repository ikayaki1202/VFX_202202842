using UnityEngine;

public interface IThrowableHandler
{
    void OnThrown(GameObject thrower, Vector3 velocity);
}