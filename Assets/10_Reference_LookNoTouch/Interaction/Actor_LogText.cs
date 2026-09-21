using UnityEngine;

/// <summary>
/// 콘솔에 텍스트를 출력한다. 디버깅/테스트용으로 가장 기본이 되는 Actor.
/// </summary>
public class Actor_LogText : MonoBehaviour
{
    [TextArea]
    public string Message = "Hello!";

    // 고정 문구 (Trigger_On*처럼 GameObject만 넘기는 이벤트와 연결)
    public void Log(GameObject sender)
    {
        Debug.Log($"[{gameObject.name}] {Message}");
    }

    // 실행 시점에 넘어오는 문구 (StringEvent를 쏘는 Trigger/Connector와 연결)
    public void Log(string message)
    {
        Debug.Log($"[{gameObject.name}] {message}");
    }
}