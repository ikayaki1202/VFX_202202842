# W06 흔적 — TRACE / IMPACT LAB

황승경 · 202202842 · Unity 6000.3.23f1 / URP 17.3.0

`Assets/W06_HW/W06_HW.unity`를 열고 Play를 누릅니다. 수업 데모와 다른 위치 `(24, 0, -36)`에 새로 구성한 사격장에서 금속·목재·콘크리트의 피탄 효과를 비교합니다.

| 대상 | 제작 내용 |
|---|---|
| 금속 | 강철 표면, 금속 스파크와 작은 탄흔 |
| 목재 | 나뭇결 표면, 길쭉하고 불규칙한 파편, 입자 나이에 따른 디졸브와 불씨 가장자리 |
| 콘크리트 | 거친 표면, 별도 SG와 VFX를 사용하는 묵직한 회색 파편 |
| 총알 | 이동 궤적의 가는 예광탄 트레일 |
| 탄흔 | 표면 법선에 맞춘 디컬, 재질별 크기 차이, 18초 유지 |

## 실행

1. Unity Hub에서 이 README가 있는 폴더를 기존 프로젝트로 추가합니다.
2. Unity **6000.3.23f1**로 엽니다. 첫 실행에는 패키지와 애셋을 가져오는 시간이 필요합니다.
3. `Assets/W06_HW/W06_HW.unity`를 열고 Play합니다. 소총은 자동으로 장착됩니다.

| 조작 | 기능 |
|---|---|
| WASD / 마우스 이동 | 이동 / 조준 |
| 마우스 왼쪽 버튼 | 발사 (누르고 있으면 연사) |
| R | 현재 탄흔 지우기 |
| F6 | 세 표적을 차례로 쏘는 자동 시연 시작·종료 |
| Esc | 커서 해제 / 시연 중이면 직접 조작으로 복귀 |

## 제출 자료

- 씬·전용 그래프·재질·프리팹: `Assets/W06_HW/`
- PDF 및 편집용 매뉴얼: `Docs/W06/`
- Unity Recorder 영상: `Deliverables/W06_202202842.mp4`
- 제출 저장소: `ikayaki1202/VFX_202202842`, 브랜치 `W06`
- 제출기한: **2026년 10월 31일(토)**. 영상과 매뉴얼은 LMS에 별도로 제출합니다.

`Assets/W06_HW/Editor`의 제작·녹화 도구는 Editor 전용이며 게임 빌드에는 포함되지 않습니다. 수업의 Trigger_OnTriggerTag와 Actor_Instantiate 원본 스크립트를 유지하고 Inspector 이벤트 및 프리팹 목록으로 새 효과를 연결했습니다.

## 출처

- [6주차 수업 자료](https://docs.google.com/presentation/d/1MXeQCkpXs_B87I_nM_-FYUNbqKFaTL3E2YQaEtwUuCs)
- [5주차 수업 자료](https://docs.google.com/presentation/d/1y9TF_Vt_eOuZOgNpOAbq7n4n-BkedC4VmFDR3bX-71k)
- [수업 원본 저장소](https://github.com/EDU-KNU-VFX-2026Q3/KNU_VFX_2026Q3/tree/W06_TraceAndMark)

수업 제공 모델·텍스처·셰이더·프레임워크의 권리는 원저작자에게 있습니다. 콘크리트 표면은 제공된 Industrial Zone의 Wall_Concrete_Rough 텍스처와 노멀맵을 사용합니다.
