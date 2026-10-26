# W06 흔적 - GLASS / WATER

황승경 · 202202842 · Unity 6000.3.23f1 / URP 17.3.0

`Assets/W06_HW/W06_HW.unity`를 열고 Play를 누릅니다. 교수자의 산업단지 맵에서 **유리 구슬과 물 구슬**을 쏘며, 단단한 표면에 남는 균열과 유동하는 표면의 흔들림을 비교합니다.

| 대상 | 피탄 반응 | 표현 의도 |
|---|---|---|
| 유리 구슬 | 맞은 지점에서 긴 가지 모양의 금이 구면을 따라 남고, 각진 유리 파편이 튑니다. | 빠른 파편과 지속되는 균열로 유리의 단단함과 취성을 표현합니다. |
| 물 구슬 | 평소에도 표면이 천천히 움직이며, 맞으면 큰 파동이 퍼지고 물방울이 튑니다. | 충격 뒤 출렁임이 점차 가라앉는 움직임으로 액체를 표현합니다. |

수업 시연 위치에서 동쪽으로 약 50 m 떨어진 앞마당 `(-75, 0, 310)`을 사용합니다. 원본 배경의 지형·건물·소품·조명은 유지했습니다. 이전 금속·목재·콘크리트 작업은 저장소 이력과 기존 애셋으로 남아 있으며, 현재 씬의 시연 대상은 두 구슬입니다.

## 실행

1. Unity Hub에서 이 README가 있는 폴더를 기존 프로젝트로 추가합니다.
2. Unity **6000.3.23f1**로 엽니다. 첫 실행에는 패키지와 애셋을 가져오는 시간이 필요합니다.
3. `Assets/W06_HW/W06_HW.unity`를 열고 Play합니다. 소총이 자동으로 장착됩니다.

| 조작 | 기능 |
|---|---|
| WASD / 마우스 이동 | 이동 / 조준 |
| 마우스 왼쪽 클릭 | 한 발 발사 |
| R | 구슬의 균열과 피탄 반응 초기화 |
| F6 | 두 구슬의 자동 시연 시작·종료 |
| Esc | 커서 해제 / 시연 중이면 직접 조작으로 복귀 |

## 제작 구조

총알의 `Bullet` 태그를 `Trigger_OnTriggerTag`가 감지합니다. Inspector의 `OnEnter` 이벤트는 `W06OrbHitGate.TryHit`에서 총알의 경로가 본체 표면을 지나는지 먼저 확인합니다. 확인된 `OnConfirmedHit` 이벤트가 `Actor_Instantiate`로 파편 프리팹을 생성하고, `W06ReactiveOrb.ReceiveHit`로 구슬 표면의 반응을 갱신합니다. 감지 영역만 스치는 빗맞음에는 효과를 만들지 않습니다. 수업의 Trigger와 Actor 원본 스크립트는 수정하지 않았습니다.

| 파일 | 역할 |
|---|---|
| `Assets/W06_HW/Shaders/W06GlassOrb.shader` | 배경 굴절, 반사와 가장자리 밝기로 유리 표면 표현 |
| `Assets/W06_HW/Shaders/W06WaterOrb.shader` | 평상시 흔들림, 피탄 파동과 굴절을 표현 |
| `Assets/W06_HW/Shaders/W06ImpactParticle.shader` | 파편과 물방울이 배경을 비추도록 굴절·반사 표현 |
| `Assets/W06_HW/Scripts/W06OrbHitGate.cs` | 총알 경로와 본체 표면의 교차를 확인하여 빗맞음 제외 |
| `Assets/W06_HW/Scripts/W06ReactiveOrb.cs` | 구면의 균열 생성, 물 셰이더에 충격 위치·시간 전달 |
| `Assets/W06_HW/Scripts/W06OrbBurst.cs` | 각진 유리 조각과 둥근 물방울의 단발 Particle System |
| `Assets/W06_HW/Editor/W06OrbSceneBuilder.cs` | 두 구슬, 재질·프리팹·이벤트를 Unity 에디터 API로 구성 |

이번 구슬 버전의 표면은 **URP HLSL 셰이더**, 균열은 구면에 붙는 메시, 파편은 **Particle System**으로 제작했습니다. 기존 과제 그래프는 프로젝트 안에 남아 있습니다.

직접 사격은 `Single` 모드로 설정했습니다. 제공 총의 `Auto` 모드에서 누름과 Update가 겹쳐 총알 두 발이 생성되던 문제를 피하고, 클릭당 한 발이 나가게 했습니다. 물리 간격 `0.01초`도 프로젝트 설정에 저장했습니다.

## 제출 자료와 확인 기록

- 씬·재질·프리팹·전용 코드: `Assets/W06_HW/`
- PDF 및 편집용 매뉴얼: `Docs/W06/`
- 새 구슬 버전의 검증 여부·녹화 파일·Push 상태: `Docs/W06/verification.json`
- 제출 저장소: `ikayaki1202/VFX_202202842`, 브랜치 `W06`
- 제출기한: **2026년 10월 31일(토)**. 영상과 매뉴얼은 LMS에 별도로 제출합니다.

자동 시연은 두 구슬 각 5회, 총 10회 명중을 녹화했습니다. 별도의 실제 마우스 입력 검사에서 Single 발사 4회(유리 3회·물 1회)를 확인했습니다. 빗맞은 물리 총알 2발의 추가 명중은 0회였으며, 초기화는 Input System의 R 키 이벤트로 확인했습니다. 자동 시연과 실제 클릭 사격 검증은 별도 기록으로 구분합니다. `Docs/W06/direct_fire/`의 금속·목재·콘크리트 화면은 이전 버전의 증거입니다. 첫 구슬 클릭 화면도 입자 재질 조정 전 기록이 포함되어 있으므로 최종 파편 외형은 `Docs/W06/orbs/video_*`와 새 MP4를 기준으로 봅니다.

## 출처

- [6주차 수업 자료](https://docs.google.com/presentation/d/1MXeQCkpXs_B87I_nM_-FYUNbqKFaTL3E2YQaEtwUuCs)
- [5주차 수업 자료](https://docs.google.com/presentation/d/1y9TF_Vt_eOuZOgNpOAbq7n4n-BkedC4VmFDR3bX-71k)
- [수업 원본 저장소](https://github.com/EDU-KNU-VFX-2026Q3/KNU_VFX_2026Q3/tree/W06_TraceAndMark)

수업 제공 모델·텍스처·셰이더·프레임워크의 권리는 원저작자에게 있습니다. 원본 산업단지 씬 파일과 제공 맵 애셋은 변경하지 않았습니다.
