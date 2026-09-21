using UnityEngine;
using TMPro;

/// <summary>
/// 화면의 TextMeshPro UI에 텍스트를 표시한다.
/// 예: 사다리를 조준하면 "E를 눌러 오르기" 같은 안내 문구를 보여줄 때 사용.
/// </summary>
public class Actor_DisplayText : MonoBehaviour
{
    public TMP_Text TargetText;

    [TextArea]
    public string Message = "여기를 클릭하세요";

    public void Show(GameObject sender)
    {
        if (TargetText == null) return;
        TargetText.text = Message;
        TargetText.gameObject.SetActive(true);
    }

    public void Hide(GameObject sender)
    {
        if (TargetText == null) return;
        TargetText.gameObject.SetActive(false);
    }
}