using UnityEngine;
public interface IHoverable
{
    void OnHoverEnter(GameObject sender); // 아웃라인 켜기
    void OnHoverExit(GameObject sender);  // 아웃라인 끄기
}