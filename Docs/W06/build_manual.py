"""Build the editable Korean W06 manual and a five-page PDF.

Update verification.json with actual Play evidence, then rerun this file.
Only Docs/W06 is written. Game source is read solely to reflect concrete edge color.
"""
from pathlib import Path
import json
import sys
from html import escape
from reportlab.pdfgen import canvas
from reportlab.platypus import (
    SimpleDocTemplate, Paragraph, Spacer, Table, TableStyle, PageBreak, KeepTogether
)
from reportlab.lib import colors
from reportlab.lib.styles import ParagraphStyle
from reportlab.lib.enums import TA_LEFT
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.lib.pagesizes import A4

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
OUT = HERE / "W06_흔적_제작매뉴얼_황승경_202202842.pdf"
MD = HERE / "W06_흔적_제작매뉴얼_황승경_202202842.md"
STATE = json.loads((HERE / "verification.json").read_text(encoding="utf-8"))
graph_code = (ROOT / "Assets/W06_HW/Editor/W06GraphTools.cs").read_text(encoding="utf-8")
edge_is_black = "Color.black" in graph_code or "new Color(0f, 0f, 0f" in graph_code
CONCRETE_EDGE = "검정 RGB (0, 0, 0)" if edge_is_black else "파편색과 동일한 일반 색상"

FONT = Path("C:/Windows/Fonts")
pdfmetrics.registerFont(TTFont("Malgun", str(FONT / "malgun.ttf")))
pdfmetrics.registerFont(TTFont("MalgunBold", str(FONT / "malgunbd.ttf")))
pdfmetrics.registerFontFamily("Malgun", normal="Malgun", bold="MalgunBold", italic="Malgun", boldItalic="MalgunBold")
NAVY = colors.HexColor("#102733")
INK = colors.HexColor("#213844")
MUTED = colors.HexColor("#526775")
TEAL = colors.HexColor("#167C86")
PALE = colors.HexColor("#EAF3F4")
LINE = colors.HexColor("#D9E3E7")
AMBER = colors.HexColor("#B56826")

styles = {
    "kicker": ParagraphStyle("kicker", fontName="MalgunBold", fontSize=9, leading=13, textColor=TEAL, spaceAfter=8),
    "title": ParagraphStyle("title", fontName="MalgunBold", fontSize=25, leading=34, textColor=NAVY, spaceAfter=11, wordWrap="CJK"),
    "lead": ParagraphStyle("lead", fontName="Malgun", fontSize=11.1, leading=18, textColor=INK, spaceAfter=15, wordWrap="CJK"),
    "h": ParagraphStyle("h", fontName="MalgunBold", fontSize=12, leading=18, textColor=NAVY, spaceBefore=12, spaceAfter=7, wordWrap="CJK"),
    "body": ParagraphStyle("body", fontName="Malgun", fontSize=9.8, leading=16, textColor=INK, spaceAfter=8, wordWrap="CJK"),
    "small": ParagraphStyle("small", fontName="Malgun", fontSize=8.4, leading=13, textColor=MUTED, spaceAfter=6, wordWrap="CJK"),
    "cell": ParagraphStyle("cell", fontName="Malgun", fontSize=9, leading=14, textColor=INK, wordWrap="CJK"),
    "cellcompact": ParagraphStyle("cellcompact", fontName="Malgun", fontSize=8.7, leading=12.6, textColor=INK, wordWrap="CJK"),
    "th": ParagraphStyle("th", fontName="MalgunBold", fontSize=9, leading=14, textColor=colors.white, wordWrap="CJK"),
    "callout": ParagraphStyle("callout", fontName="MalgunBold", fontSize=10, leading=16, textColor=TEAL, wordWrap="CJK"),
}

def P(text, kind="body"):
    return Paragraph(escape(str(text)).replace("\n", "<br/>"), styles[kind])

def table(rows, widths=None, compact=False):
    usable = A4[0] - 84
    if widths is None:
        widths = [usable * .25, usable * .30, usable * .45]
    cells = [[P(c, "th" if n == 0 else ("cellcompact" if compact else "cell")) for c in row] for n, row in enumerate(rows)]
    t = Table(cells, colWidths=widths, repeatRows=1, hAlign="LEFT")
    t.setStyle(TableStyle([
        ("BACKGROUND", (0,0), (-1,0), NAVY),
        ("ROWBACKGROUNDS", (0,1), (-1,-1), [colors.white, colors.HexColor("#F3F7F8")]),
        ("VALIGN", (0,0), (-1,-1), "TOP"),
        ("LEFTPADDING", (0,0), (-1,-1), 9),
        ("RIGHTPADDING", (0,0), (-1,-1), 9),
        ("TOPPADDING", (0,0), (-1,-1), 5 if compact else 6),
        ("BOTTOMPADDING", (0,0), (-1,-1), 5 if compact else 6),
        ("LINEBELOW", (0,0), (-1,0), .6, NAVY),
        ("LINEBELOW", (0,1), (-1,-1), .35, LINE),
    ]))
    return t

def callout(text):
    t = Table([[P(text, "callout")]], colWidths=[A4[0] - 84])
    t.setStyle(TableStyle([
        ("BACKGROUND", (0,0), (-1,-1), PALE),
        ("BOX", (0,0), (-1,-1), .6, LINE),
        ("LEFTPADDING", (0,0), (-1,-1), 12),
        ("RIGHTPADDING", (0,0), (-1,-1), 12),
        ("TOPPADDING", (0,0), (-1,-1), 10),
        ("BOTTOMPADDING", (0,0), (-1,-1), 10),
    ]))
    return t

pages = [
[
    ("kicker", "W06 / 3D 디지털 이펙트 / 2026.10.26"),
    ("title", "흔적\nTRACE · IMPACT LAB"),
    ("lead", "황승경_202202842\n움직임은 빛을, 충격은 표면의 자국을 남긴다."),
    ("callout", "설정과 검증 결과는 실제 에디터 애셋·코드·녹화 파일을 기준으로 기록했다. 확인 범위와 제출 상태는 마지막 페이지에 표시한다."),
    ("h", "제작 목표와 새 구역"),
    ("body", "금속·목재·콘크리트의 세 레인을 가진 사격 실험실을 구성한다. 수업 데모의 게임플레이 구역은 대략 (-125, 0, 315), 새 실험실의 기준점은 (24, 0, -36)이다. 기존 데모의 위치를 재사용하지 않고 새 공간에서 표적과 피탄 효과를 재현한다."),
    ("table", [
        ["대상", "표현", "선택한 이유"],
        ["금속", "스파크 + 작은 탄흔", "밝고 빠른 반응으로 단단한 표면을 구분한다."],
        ["목재", "긴 나무 조각 + 불씨 디졸브 + 탄흔", "길쭉한 실루엣과 타는 가장자리로 목재의 결을 표현한다."],
        ["콘크리트", "굵고 무거운 회색 파편 + 큰 탄흔", "목재와 다른 조각 SG·VFX 값을 사용해 무게감과 부스러짐을 구분한다."],
    ]),
    ("h", "열기와 조작"),
    ("body", "Unity 6000.3.23f1에서 프로젝트를 열고, Project 창의 Assets > W06_HW > W06_HW 씬을 연다. Play를 시작하면 수업용 총이 자동 장착된다."),
    ("table", [
        ["입력", "기능", "사용 시점"],
        ["WASD / 마우스", "이동 / 시점·사격", "표적의 여러 지점을 직접 쏜다."],
        ["R / F6 / Esc", "탄흔 지우기 / 자동 시연 / 커서 해제", "비교 장면을 정리하거나 세 재질의 반응을 순서대로 본다."],
    ]),
],
[
    ("kicker", "01 / WOOD"),
    ("title", "조각의 모양과 타는 시간"),
    ("lead", "셰이더가 입자 하나의 윤곽을, VFX Graph가 개수·속도·수명을 결정한다."),
    ("body", "SG_WoodChip_HW와 VFX_HitWood_HW를 과제 폴더에 복사해 독립적으로 조정했다. Simple Noise와 원형 마스크의 Multiply 결과를 Alpha에 사용하고, 입자 나이 비율을 Dissolve_Amount에 연결한다."),
    ("table", [
        ["항목", "설정값", "기대 효과"],
        ["SG / Noise Scale", "4.2", "크게 뭉친 얼룩으로 목재 조각의 윤곽을 만든다."],
        ["SG / Alpha Clip 시작", "0.16", "밝은 노이즈 조각만 남겨 둥근 외곽을 끊는다."],
        ["디졸브 연결", "Age Over Lifetime -> Dissolve_Amount", "개별 입자의 수명에 따라 잘려 나가는 범위가 커진다."],
        ["파편색 / 가장자리", "갈색 RGB (0.30, 0.105, 0.026)\n주황 HDR 2.4 / 폭 0.035", "갈색 중심과 좁은 불씨 띠를 구분한다."],
        ["Spawn", "단발 Count 27\nLoop Duration 1 / Count 1", "짧은 충격 한 번에 여러 조각이 퍼진다."],
        ["Capacity / Bounds", "96 / Manual, 크기 (7, 7, 7)", "여유 입자 수와 이동 영역을 확보한다."],
        ["수명", "0.90-1.45초", "타들어 가는 변화를 볼 시간을 준다."],
        ["Size / Scale", "0.065 / (0.55, 1.70, 1)", "잘려 나온 긴 나뭇조각이 보일 크기를 확보한다."],
        ["Initialize / angleZ", "Uniform Random 0-360도", "파편마다 각도를 달리해 반복되는 모양을 줄인다."],
        ["초기 방향 / 속도", "로컬 위쪽 / 2.8-5.8", "충돌 표면 바깥쪽으로 비교적 가볍게 튄다."],
        ["중력 / Drag", "(0, -10.8, 0) / 0.65", "상승 뒤 낙하하는 곡선을 만든다."],
        ["액터 생성 수명", "3초 / Align To Surface 켬", "그래프 수명보다 오래 프리팹을 유지하고 표면 방향에 맞춘다."],
    ]),
    ("h", "값을 고른 이유"),
    ("body", "초기 시연에서 낮은 Clip 값이 매끈한 타원 외곽을 남겼다. 기준값을 0.16으로 올려 불규칙한 부분을 분리하고, 줄어든 윤곽이 보이도록 Size를 0.065로 키웠다. 무작위 각도와 긴 비율, 27개의 단발 입자로 목재의 가벼운 흩어짐을 설계했다."),
],
[
    ("kicker", "02 / CONCRETE + METAL"),
    ("title", "재질에 따라 다른 충격"),
    ("lead", "콘크리트는 목재 SG와 VFX의 복사본을 각각 바꿨다. 색상만 바꾸는 방식으로 끝내지 않았다."),
    ("compacttable", [
        ["항목", "콘크리트 설정값", "목재와 다른 기대 효과"],
        ["SG / Noise Scale", "9", "더 잘게 끊어진 불규칙한 돌 윤곽."],
        ["SG / Alpha Clip 시작", "0.26", "원형 외곽 대신 분리된 돌 부스러기 윤곽."],
        ["디졸브", "나이 연결 해제 / Amount 0", "시간에 따라 타는 표현을 사용하지 않는다."],
        ["파편색 / 가장자리", "RGB (0.36, 0.39, 0.41)\n" + CONCRETE_EDGE, "회색 파편에 목재의 주황 발광이 섞이지 않는다."],
        ["Spawn / Capacity", "단발 Count 16 / 64\nLoop Duration 1 / Count 1", "큰 조각이 적게 튀는 반응."],
        ["Bounds", "Manual / 크기 (7, 7, 7)", "설정한 이동 범위를 포함한다."],
        ["수명", "0.55-0.85초", "목재보다 짧고 빠르게 정리되는 충격."],
        ["Size / Scale", "0.105 / (1, 0.80, 1)", "목재보다 굵고 덩어리진 조각."],
        ["Initialize / angleZ", "Uniform Random 0-360도", "고정 방향의 점 모양 반복을 줄인다."],
        ["초기 속도", "1.5-3.8", "더 좁고 둔하게 퍼지는 파편."],
        ["중력 / Drag", "(0, -13, 0) / 1.15", "더 무겁게 가라앉는 움직임."],
    ]),
    ("h", "금속 표적"),
    ("body", "금속 피탄은 W05의 VFX_HitMetal_Spark를 FX_HW_Metal로 복사해 그래프 내부 수치를 유지했다. 세 표적 표면은 모두 URP Lit으로 구성했다."),
    ("compacttable", [
        ["항목", "설정값", "기대 효과"],
        ["금속 표면", "RGB (0.23, 0.31, 0.36)\nMetallic 0.92 / Smoothness 0.67", "차가운 강철색과 매끈한 반사."],
        ["목재 표면", "제공 목재 텍스처 x RGB (0.52, 0.30, 0.13)\nMetallic 0 / Smoothness 0.16", "목재의 결을 보존하고 반짝임을 낮춤."],
        ["콘크리트 표면", "제공 Rough + Normal\nRGB (0.70, 0.72, 0.74)\nTiling (2,1) / Normal 0.38", "Wall_Concrete_Rough 텍스처와 노멀로 돌의 거친 표면 표현.\nMetallic 0.03 / Smoothness 0.18."],
        ["피탄 프리팹 유지", "세 재질 모두 3초", "짧은 파편·스파크가 끝난 뒤 생성물을 정리."],
    ]),
    ("body", "값을 고른 이유: 초기의 점처럼 보이는 외곽을 줄이려고 Clip을 0.26, Size를 0.105로 높였다. 큰 조각, 느린 속도, 짧은 수명, 강한 중력·저항으로 목재와 실루엣·리듬을 구분했다."),
],
[
    ("kicker", "03 / TRAIL + DECAL"),
    ("title", "지나간 자리와 맞은 자리"),
    ("lead", "총알의 이동은 가는 빛으로, 표면의 충격은 재질별 크기가 다른 탄흔으로 남긴다."),
    ("table", [
        ["항목", "설정값", "기대 효과"],
        ["총알 속도 / 발사 간격", "32 / 0.19초, 자동 사격", "짧은 간격으로 세 표적을 비교."],
        ["Trail Time / Width", "0.22초 / 0.035", "표적을 가리지 않는 짧고 가는 궤적."],
        ["Trail 폭 커브", "시작 1 -> 끝 0", "꼬리 끝이 가늘어짐."],
        ["Trail 색·Alpha", "밝은 노랑 -> 주황\nAlpha 1 -> 0", "이동 뒤쪽으로 자연스럽게 사라짐."],
        ["Min Vertex Distance", "0.05", "리본 궤적을 촘촘하게 연결."],
        ["Trail 머티리얼", "제공 Mat_Trail 복사\nBase Color (3.2, 2.45, 0.8)", "따뜻한 발광색과 Bloom의 번짐."],
        ["금속 탄흔 W x H", "0.13 x 0.13", "작고 집중된 자국."],
        ["목재 탄흔 W x H", "0.17 x 0.22", "세로로 약간 긴 목재 자국."],
        ["콘크리트 탄흔 W x H", "0.28 x 0.28", "더 넓게 부스러진 표면 자국."],
        ["디컬 투영", "Depth 0.32 / Pivot (0,0,0)\nDraw Distance 50", "곡면 충돌 지점에서 투영 범위를 확보."],
        ["탄흔 수명·방향", "18초 / Align To Surface 켬", "여러 발의 자국이 잠시 남고 표면 안쪽으로 투영."],
        ["렌더링", "URP Decal Feature / HDR 켬\nBloom 0.32, Threshold 1.05", "디컬 표시와 밝은 궤적·불씨 번짐 지원."],
    ]),
    ("h", "연결과 선택 이유"),
    ("body", "Bullet 태그의 총알 -> 감지 Trigger -> Actor_Instantiate 목록 -> 피탄 프리팹과 디컬 프리팹을 함께 생성한다. 본체 SphereCollider와 반경 0.7의 감지용 Trigger를 분리했다. 표면 방향 정렬은 기존 수업 액터를 사용한다."),
    ("body", "속도 32에서 0.22초 동안 이동하는 거리는 약 7.04 m다. 실제 보이는 길이는 충돌·소멸과 시점에 따라 줄어든다. 탄흔은 같은 텍스처를 사용하되 크기를 달리해 선택 도전의 재질별 자국 차이를 시도했다."),
],
[
    ("kicker", "04 / EXPERIMENT + DELIVERY"),
    ("title", "시도한 것과 확인할 것"),
    ("h", "설정 확인과 수정 기록"),
    ("compacttable", [
        ["시도·발견", "변경·대안", "이유"],
        ["제공 총알의 Trail Time이 5초", "과제 복사본에서 0.22초로 변경", "지나치게 긴 흔적을 줄이고 표적의 피탄 반응에 시선을 모으기 위해."],
        ["낮은 Clip이 목재의 매끈한 타원·콘크리트의 점 외곽을 남김", "Clip: 목재 0.035 -> 0.16, 돌 0.10 -> 0.26\nSize: 0.065 / 0.105\nangleZ: 무작위 0-360도", "노이즈의 불규칙한 조각을 드러내고, 잘린 조각을 볼 크기와 방향 변화를 확보하기 위해."],
        ["목재 그래프를 콘크리트용으로 복사", "나이 기반 디졸브 연결 해제, Amount 0\n외곽색: " + CONCRETE_EDGE, "돌이 불타는 표현이 섞이지 않도록 목재의 소멸 방식과 분리."],
        ["같은 탄흔을 모든 표적에 적용", "금속·목재·콘크리트의 W/H 차별화", "스크립트를 바꾸지 않고 자국의 면적으로 재질 반응을 구분하기 위해."],
    ]),
    ("h", "검증 상태: " + STATE["status_label"]),
    ("small", "실제 실행에서 확보한 증거와 추가 확인 항목을 구분한다."
     + ((" 확인일: " + STATE["verification_date"]) if STATE["verification_date"] else "")),
    ("checktable", [["확인 항목", "상태", "증거·후속 확인"]] + STATE["checks"]),
    ("small", "영상: " + STATE["video_file"] + "\nGit 제출: " + STATE["submission_state"]),
    ("h", "제출 파일과 출처"),
    ("body", "W06_HW 씬·애셋을 VFX_202202842 저장소의 W06 브랜치로 Push하고, Unity Recorder 영상과 이 매뉴얼을 LMS에 제출한다. 제출기한은 2026년 10월 31일(토)이다."),
    ("small", "제작 기반: 황주선 교수자의 KNU_VFX_2026Q3 수업 저장소. 제공 프레임워크·모델·텍스처·그래프의 권리는 각 원저작자에게 있다. 과제용 복사본과 새 실험실 구성을 별도 폴더에 보관한다."),
    ("sources", [
        ("6주차 수업자료", "https://docs.google.com/presentation/d/1MXeQCkpXs_B87I_nM_-FYUNbqKFaTL3E2YQaEtwUuCs"),
        ("5주차 참고자료", "https://docs.google.com/presentation/d/1y9TF_Vt_eOuZOgNpOAbq7n4n-BkedC4VmFDR3bX-71k"),
        ("교수자 원본 저장소", "https://github.com/EDU-KNU-VFX-2026Q3/KNU_VFX_2026Q3"),
    ]),
]
]
if STATE["additional_observations"]:
    pages[4].insert(-3, ("small", "실행 관찰: " + " / ".join(STATE["additional_observations"])))

class NumberedCanvas(canvas.Canvas):
    def __init__(self, *args, **kwargs):
        canvas.Canvas.__init__(self, *args, **kwargs)
        self.saved_pages = []
    def showPage(self):
        self.saved_pages.append(dict(self.__dict__))
        self._startPage()
    def save(self):
        total = len(self.saved_pages)
        for state in self.saved_pages:
            self.__dict__.update(state)
            self.setFillColor(MUTED)
            self.setFont("Malgun", 7.5)
            self.drawRightString(A4[0]-42, 25, f"{self._pageNumber} / {total}")
            canvas.Canvas.showPage(self)
        canvas.Canvas.save(self)

def footer(c, doc):
    c.saveState()
    width, height = A4
    c.setFillColor(NAVY)
    c.rect(0, height-12, width, 12, fill=1, stroke=0)
    c.setFont("MalgunBold", 8)
    c.setFillColor(MUTED)
    c.drawString(42, height-42, "TRACE / IMPACT LAB")
    c.setFont("Malgun", 8)
    c.drawRightString(width-42, height-42, "황승경_202202842  |  W06 흔적")
    c.setStrokeColor(LINE)
    c.line(42, 39, width-42, 39)
    c.setFont("Malgun", 7.5)
    c.drawString(42, 25, "3D 디지털 이펙트  /  제작 설정과 선택 이유")
    c.restoreState()

story, md_lines = [], ["# W06 흔적 - TRACE · IMPACT LAB", "", "**황승경_202202842**", ""]
for n, blocks in enumerate(pages):
    if n:
        story.append(PageBreak())
        md_lines += ["", "---", ""]
    for kind, value in blocks:
        if kind in ("table", "checktable", "compacttable"):
            usable = A4[0] - 84
            widths = [usable*.25, usable*.15, usable*.60] if kind == "checktable" else None
            story += [table(value, widths, compact=kind in ("compacttable","checktable")), Spacer(1, 6)]
            md_lines += ["| " + " | ".join(value[0]) + " |", "| " + " | ".join("---" for _ in value[0]) + " |"]
            md_lines += ["| " + " | ".join(str(x).replace("\n", "<br>") for x in row) + " |" for row in value[1:]]
            md_lines.append("")
        elif kind == "sources":
            links = []
            for label, url in value:
                links.append(f'<link href="{escape(url)}" color="#167C86">{escape(label)}</link>')
                md_lines += [f"- [{label}]({url})"]
            story.append(Paragraph("  /  ".join(links), styles["small"]))
            md_lines.append("")
        elif kind == "callout":
            story += [callout(value), Spacer(1, 6)]
            md_lines += ["> " + value, ""]
        else:
            story.append(P(value, kind))
            prefix = "## " if kind == "title" else "### " if kind == "h" else ""
            md_lines += [prefix + value.replace("\n", "  \n"), ""]

MD.write_text("\n".join(md_lines), encoding="utf-8")
doc = SimpleDocTemplate(str(OUT), pagesize=A4, rightMargin=42, leftMargin=42,
    topMargin=66, bottomMargin=54, title="W06 흔적 제작 매뉴얼 - 황승경_202202842",
    author="황승경_202202842", subject="TRACE · IMPACT LAB 설정값과 제작 이유")
doc.build(story, onFirstPage=footer, onLaterPages=footer, canvasmaker=NumberedCanvas)
print(json.dumps({"pdf":str(OUT), "markdown":str(MD), "concrete_edge":CONCRETE_EDGE}, ensure_ascii=False))
