# W06 매뉴얼 갱신

build_manual.py는 같은 폴더의 PDF와 Markdown을 함께 갱신한다.

1. verification.json에 **실제로 확인한** 상태와 증거를 적는다.
2. 검증 전 항목은 검증 대기를 유지한다. checks의 한 행은 확인 항목 / 상태 / 증거 순서다.
3. video_file에는 실제 녹화 파일명을, submission_state에는 실제 Push 상태를 적는다.
4. 아래 명령으로 다시 만든다.

    & 'C:\Users\게임-532-159\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe' 'C:\Users\게임-532-159\Desktop\codex_VFX_W06\Docs\W06\build_manual.py'

수치나 제작 방식이 바뀌면 build_manual.py의 해당 표·설명도 함께 수정한다. 콘크리트 외곽색의 검정 여부는 W06GraphTools.cs를 읽어 자동 반영한다. 그 외 수치는 작성 시점의 W06GraphTools.cs, W06SceneBuilder.cs, W06LabSession.cs를 읽고 기재한 값이다.

PDF를 다시 만들면 다음 명령으로 렌더하고 모든 페이지의 표·한글·바닥글을 확인한다.

    & 'C:\Users\게임-532-159\.cache\codex-runtimes\codex-primary-runtime\dependencies\native\poppler\Library\bin\pdftoppm.exe' -r 100 -png 'C:\Users\게임-532-159\Desktop\codex_VFX_W06\Docs\W06\W06_흔적_제작매뉴얼_황승경_202202842.pdf' 'C:\Users\게임-532-159\Desktop\codex_VFX_W06\Docs\W06\qa\page'

qa/의 PNG는 배치 검사용 중간 결과이며, LMS에는 최종 PDF와 실제 녹화 영상만 제출하면 된다.
