# W06 매뉴얼 갱신

build_manual.py는 같은 폴더의 PDF와 Markdown을 함께 갱신한다. 현재 내용은 유리 구슬과 물 구슬 버전이다.

1. verification.json에 **실제로 확인한** 상태와 증거를 적는다.
2. 새 구슬 버전의 검증 결과에는 `variant: "glass_water"`를 넣는다. 이 표식이 없으면 이전 3재질 통과 기록을 가져오지 않고 새 구슬 검증 대기로 출력한다.
3. checks의 각 행은 확인 항목 / 상태 / 증거 순서다. 실제 마우스 클릭과 자동 시연 결과를 구분한다.
4. video_file에는 두 구슬 버전의 실제 녹화 파일을, submission_state에는 실제 Push 상태를 적는다.
5. 아래 명령으로 다시 만든다.

    & 'C:\Users\게임-532-159\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe' 'C:\Users\게임-532-159\Desktop\codex_VFX_W06\Docs\W06\build_manual.py'

수치나 제작 방식이 바뀌면 build_manual.py의 표·설명도 함께 수정한다. 현재 수치는 W06OrbSceneBuilder.cs, W06ReactiveOrb.cs, W06OrbBurst.cs와 두 구슬 셰이더를 읽고 기록했다. 표면은 URP HLSL, 균열은 메시, 파편은 Particle System이며 이전 Shader Graph·VFX Graph와 구분한다.

PDF를 다시 만들면 다음 명령으로 렌더하고 모든 페이지의 표·한글·바닥글을 확인한다.

    & 'C:\Users\게임-532-159\.cache\codex-runtimes\codex-primary-runtime\dependencies\native\poppler\Library\bin\pdftoppm.exe' -r 100 -png 'C:\Users\게임-532-159\Desktop\codex_VFX_W06\Docs\W06\W06_흔적_제작매뉴얼_황승경_202202842.pdf' 'C:\Users\게임-532-159\Desktop\codex_VFX_W06\Docs\W06\qa\orb-manual'

qa/의 PNG는 배치 검사용 중간 결과다. LMS에는 최종 PDF와 실제 녹화 영상을 제출한다. 기존 direct_fire/의 금속·목재·콘크리트 화면은 이전 버전의 검증 기록으로 유지한다.
