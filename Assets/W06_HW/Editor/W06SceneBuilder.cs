using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;

namespace W06Homework
{
    public static class W06SceneBuilder
    {
        const string Root = "Assets/W06_HW/";
        static readonly Vector3 Origin = new Vector3(24, 0, -36);
        static readonly Color[] Accents = { new Color(.32f,.77f,.94f), new Color(1,.58f,.22f), new Color(.65f,.78f,.57f) };
        static Material floor, wall, dark, lightMat;
        static TMP_FontAsset font;
        static Transform environment;

        static T Load<T>(string path) where T:UnityEngine.Object
        { var v=AssetDatabase.LoadAssetAtPath<T>(path); if(!v) throw new Exception("Missing asset: "+path); return v; }
        static void Copy(string source,string destination)
        { if(!AssetDatabase.LoadMainAssetAtPath(destination) && !AssetDatabase.CopyAsset(source,destination)) throw new Exception("Copy failed "+source); }

        public static string BuildAssets()
        {
            foreach(var folder in new[]{"Graphs","Prefabs","Materials","Render"})
                if(!AssetDatabase.IsValidFolder(Root+folder)) AssetDatabase.CreateFolder(Root.TrimEnd('/'),folder);
            Copy("Assets/W06_TraceAndMark/SG_WoodChip.shadergraph",Root+"Graphs/SG_WoodChip_HW.shadergraph");
            Copy("Assets/W06_TraceAndMark/SG_WoodChip.shadergraph",Root+"Graphs/SG_ConcreteChip_HW.shadergraph");
            W06GraphTools.TuneShader(Root+"Graphs/SG_WoodChip_HW.shadergraph",4.2f,.16f);
            W06GraphTools.TuneShader(Root+"Graphs/SG_ConcreteChip_HW.shadergraph",9f,.26f);
            Copy("Assets/W06_TraceAndMark/VFX_HitWood_Chips.vfx",Root+"Graphs/VFX_HitWood_HW.vfx");
            Copy("Assets/W06_TraceAndMark/VFX_HitWood_Chips.vfx",Root+"Graphs/VFX_HitConcrete_HW.vfx");
            W06GraphTools.TuneVfx(Root+"Graphs/VFX_HitWood_HW.vfx",Root+"Graphs/SG_WoodChip_HW.shadergraph",false);
            W06GraphTools.TuneVfx(Root+"Graphs/VFX_HitConcrete_HW.vfx",Root+"Graphs/SG_ConcreteChip_HW.shadergraph",true);
            FxPrefab("Assets/W06_TraceAndMark/VFX_HitWood_Chips.prefab","FX_HW_Wood",Root+"Graphs/VFX_HitWood_HW.vfx");
            FxPrefab("Assets/W06_TraceAndMark/VFX_HitWood_Chips.prefab","FX_HW_Concrete",Root+"Graphs/VFX_HitConcrete_HW.vfx");
            FxPrefab("Assets/W05_HitEffect/VFX_HitMetal_Spark.prefab","FX_HW_Metal",null);
            for(int i=0;i<3;i++)
            {
                string name=new[]{"Metal","Wood","Concrete"}[i];
                var prefab=PrefabUtility.LoadPrefabContents("Assets/W06_TraceAndMark/Decal_BulletHole.prefab");
                prefab.name="Decal_HW_"+name;
                var decal=prefab.GetComponentInChildren<DecalProjector>();
                decal.size=new Vector3(i==0?.13f:i==1?.17f:.28f,i==0?.13f:i==1?.22f:.28f,.32f);
                decal.pivot=Vector3.zero;
                decal.drawDistance=50;
                PrefabUtility.SaveAsPrefabAsset(prefab,Root+"Prefabs/Decal_HW_"+name+".prefab");
                PrefabUtility.UnloadPrefabContents(prefab);
            }
            Copy("Assets/W06_TraceAndMark/Mat_Trail.mat",Root+"Materials/Mat_Trail_HW.mat");
            var trailMat=Load<Material>(Root+"Materials/Mat_Trail_HW.mat");
            if(trailMat.HasProperty("_BaseColor")) trailMat.SetColor("_BaseColor",new Color(3.2f,2.45f,.8f,1));
            EditorUtility.SetDirty(trailMat);
            var bullet=PrefabUtility.LoadPrefabContents("Assets/20_TuneOnly/Items/Prefabs/Projectile_Bullet.prefab");
            bullet.name="Projectile_Bullet_HW"; bullet.tag="Bullet";
            var tr=bullet.GetComponent<TrailRenderer>()??bullet.AddComponent<TrailRenderer>();
            tr.sharedMaterial=trailMat; tr.time=.22f; tr.widthMultiplier=.035f; tr.minVertexDistance=.05f;
            tr.widthCurve=AnimationCurve.Linear(0,1,1,0);
            var gradient=new Gradient(); gradient.SetKeys(new[]{new GradientColorKey(new Color(1,.88f,.45f),0),new GradientColorKey(new Color(1,.46f,.12f),1)},new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(0,1)}); tr.colorGradient=gradient;
            var rb=bullet.GetComponent<Rigidbody>(); rb.useGravity=false; rb.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
            PrefabUtility.SaveAsPrefabAsset(bullet,Root+"Prefabs/Projectile_Bullet_HW.prefab"); PrefabUtility.UnloadPrefabContents(bullet);
            var rifle=PrefabUtility.LoadPrefabContents("Assets/20_TuneOnly/Items/Prefabs/Usable_Rifle.prefab");
            rifle.name="Rifle_HW"; var firearm=rifle.GetComponent<Usable_Firearm>(); firearm.BulletPrefab=Load<GameObject>(Root+"Prefabs/Projectile_Bullet_HW.prefab").GetComponent<Projectile_Bullet>(); firearm.BulletSpeed=32; firearm.Mode=FireMode.Auto; firearm.FireRate=.19f;
            PrefabUtility.SaveAsPrefabAsset(rifle,Root+"Prefabs/Rifle_HW.prefab"); PrefabUtility.UnloadPrefabContents(rifle);
            Copy("Assets/20_TuneOnly/Items/ScriptableObjects/SO_ItemData_Rifle.asset",Root+"Prefabs/SO_Rifle_HW.asset");
            var item=Load<SO_ItemData>(Root+"Prefabs/SO_Rifle_HW.asset"); item.WorldPrefab=Load<GameObject>(Root+"Prefabs/Rifle_HW.prefab"); item.ItemName="Range rifle"; EditorUtility.SetDirty(item);
            Copy("Assets/W06_TraceAndMark/Mat_SG_WoodTarget.mat",Root+"Materials/Mat_Wood_HW.mat");
            Copy("Assets/W05_HitEffect/Mat_SG_MetalTarget.mat",Root+"Materials/Mat_Metal_HW.mat");
            var wood=Load<Material>(Root+"Materials/Mat_Wood_HW.mat"); wood.SetFloat("_Smoothness",.16f); EditorUtility.SetDirty(wood);
            var metal=Load<Material>(Root+"Materials/Mat_Metal_HW.mat"); metal.SetFloat("_Metallic",.92f); metal.SetFloat("_Smoothness",.67f); EditorUtility.SetDirty(metal);
            Mat("Mat_Concrete_HW",new Color(.39f,.42f,.44f),.03f,.18f);
            PolishMaterials();
            AssetDatabase.SaveAssets();
            return "Homework graphs, projectile, rifle, three effects and three decals saved.";
        }

        static void FxPrefab(string source,string name,string graph)
        {
            var go=PrefabUtility.LoadPrefabContents(source); go.name=name;
            if(graph!=null) go.GetComponentInChildren<VisualEffect>().visualEffectAsset=Load<VisualEffectAsset>(graph);
            PrefabUtility.SaveAsPrefabAsset(go,Root+"Prefabs/"+name+".prefab"); PrefabUtility.UnloadPrefabContents(go);
        }

        public static string BuildScene()
        {
            if(UnityEditor.EditorApplication.isPlaying) throw new Exception("Stop Play mode first.");
            font=AssetDatabase.FindAssets("LiberationSans SDF t:TMP_FontAsset").Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<TMP_FontAsset>).FirstOrDefault();
            if(!font) { TMP_PackageResourceImporter.ImportResources(true,false,false); AssetDatabase.Refresh(); font=AssetDatabase.FindAssets("LiberationSans SDF t:TMP_FontAsset").Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<TMP_FontAsset>).FirstOrDefault(); }
            if(!font) throw new Exception("TMP essentials font is not imported yet. Retry after import.");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            environment=new GameObject("East Material Lab (24, 0, -36)").transform; environment.position=Origin;
            floor=Mat("Mat_Lab_Floor",new Color(.075f,.095f,.12f),.45f,.32f);
            wall=Mat("Mat_Lab_Wall",new Color(.12f,.15f,.18f),.25f,.4f);
            dark=Mat("Mat_Lab_Dark",new Color(.035f,.047f,.062f),.4f,.42f);
            lightMat=Mat("Mat_Lab_Light",new Color(.7f,.86f,.92f),.1f,.5f,new Color(.6f,.85f,1)*2);
            Block("Foundation",new Vector3(0,-.28f,1),new Vector3(18,.5f,22),floor);
            Block("Back wall",new Vector3(0,3.3f,9),new Vector3(17,.3f+6.3f,.5f),wall);
            Block("Left wall",new Vector3(-8.7f,2.3f,1),new Vector3(.45f,4.6f,16),dark);
            Block("Right wall",new Vector3(8.7f,2.3f,1),new Vector3(.45f,4.6f,16),dark);
            Block("Header beam",new Vector3(0,5.8f,8.1f),new Vector3(17,.16f,.6f),dark);
            WorldText("TRACE / IMPACT LAB",new Vector3(-7.7f,5.32f,8.65f),.52f,Color.white,TextAlignmentOptions.Left);
            WorldText("MATERIAL RESPONSE STUDY   /   06",new Vector3(-7.65f,4.95f,8.64f),.19f,new Color(.45f,.61f,.7f),TextAlignmentOptions.Left);
            for(int x=-8;x<=8;x+=2) Block("Floor seam",new Vector3(x,-.019f,1),new Vector3(.018f,.012f,21),dark,false);
            for(int z=-8;z<=10;z+=2) Block("Floor seam",new Vector3(0,-.017f,z),new Vector3(17,.012f,.018f),dark,false);
            for(int i=0;i<3;i++)
            {
                float x=(i-1)*4.6f; Color accent=Accents[i];
                var trim=Mat("Mat_Accent_"+i,accent,.25f,.5f,accent*1.9f);
                Block("Bay "+i+" back",new Vector3(x,2.3f,7.9f),new Vector3(4.05f,4.1f,.3f),dark);
                Block("Bay "+i+" header",new Vector3(x,4.45f,7.5f),new Vector3(4.05f,.035f,.05f),trim,false);
                Block("Lane "+i,new Vector3(x,.005f,1),new Vector3(.025f,.015f,12.5f),trim,false);
                Block("Bay "+i+" plinth",new Vector3(x,.35f,5.8f),new Vector3(2.8f,.7f,2.2f),wall);
                Block("Bay "+i+" plinth trim",new Vector3(x,.72f,5.8f),new Vector3(2.7f,.025f,2.1f),trim,false);
                var pedestal=GameObject.CreatePrimitive(PrimitiveType.Cylinder); pedestal.name="Target mount"; pedestal.transform.SetParent(environment); pedestal.transform.localPosition=new Vector3(x,1.0f,5.8f); pedestal.transform.localScale=new Vector3(.12f,.3f,.12f); pedestal.GetComponent<Renderer>().sharedMaterial=dark;
                string label=new[]{"01 / METAL","02 / WOOD","03 / CONCRETE"}[i];
                WorldText(label,new Vector3(x,3.85f,7.66f),.29f,accent,TextAlignmentOptions.Center);
                WorldText(new[]{"SPARK / MARK","CHIP / EMBER","FRAGMENT / MARK"}[i],new Vector3(x,3.38f,7.66f),.15f,new Color(.66f,.72f,.75f),TextAlignmentOptions.Center);
                WorldText("0"+(i+1),new Vector3(x,.39f,4.67f),.24f,accent,TextAlignmentOptions.Center);
                Ring(new Vector3(x,2.15f,7.62f),1.13f,trim);
                LightAt("Bay key",new Vector3(x-1.2f,4.2f,3.9f),new Vector3(x,2.0f,5.8f),new Color(.8f,.9f,1),35,7,LightType.Spot);
                LightAt("Bay rim",new Vector3(x+1.5f,2.7f,7.1f),new Vector3(x,2.1f,5.8f),accent,12,5,LightType.Point);
            }
            for(int n=0;n<6;n++) Block("Ceiling strip",new Vector3((n%3-1)*5,5.9f,-3+(n/3)*8),new Vector3(3,.055f,.25f),lightMat,false);
            LightAt("Soft key",new Vector3(0,5,-1),new Vector3(0,1,5),new Color(.74f,.84f,1),90,20,LightType.Point);
            var sun=new GameObject("Soft directional").AddComponent<Light>(); sun.type=LightType.Directional; sun.color=new Color(.8f,.88f,1); sun.intensity=1.25f; sun.transform.rotation=Quaternion.Euler(42,-30,0); sun.shadows=LightShadows.Soft;
            RenderSettings.ambientMode=AmbientMode.Trilight; RenderSettings.ambientSkyColor=new Color(.3f,.36f,.43f); RenderSettings.ambientEquatorColor=new Color(.18f,.21f,.25f); RenderSettings.ambientGroundColor=new Color(.08f,.09f,.12f); RenderSettings.fog=false; RenderSettings.skybox=null;
            ConfigurePipeline();
            var vol=new GameObject("Lab post processing").AddComponent<Volume>(); vol.isGlobal=true;
            var profile=ScriptableObject.CreateInstance<VolumeProfile>(); AssetDatabase.CreateAsset(profile,Root+"Render/Lab_Volume.asset");
            profile.Add<Bloom>().intensity.Override(.32f); profile.TryGet<Bloom>(out var bloom); bloom.threshold.Override(1.05f); bloom.scatter.Override(.55f);
            profile.Add<Tonemapping>().mode.Override(TonemappingMode.ACES); vol.sharedProfile=profile;
            var session=new GameObject("W06 Lab Session").AddComponent<W06LabSession>(); session.zoneOrigin=Origin;
            var names=new[]{"Metal","Wood","Concrete"}; var targets=new List<Transform>();
            for(int i=0;i<3;i++)
            {
                var go=GameObject.CreatePrimitive(PrimitiveType.Sphere); go.name="Target_"+names[i]+"_HW"; go.transform.position=Origin+new Vector3((i-1)*4.6f,2.15f,5.8f); go.transform.localScale=Vector3.one*1.64f;
                go.GetComponent<Renderer>().sharedMaterial=Load<Material>(Root+"Materials/Mat_"+names[i]+"_HW.mat");
                var actor=go.AddComponent<Actor_Instantiate>(); actor.surfaceCollider=go.GetComponent<SphereCollider>(); actor.attachToThis=false;
                actor.prefabs=new List<Actor_Instantiate.Entry>{new Actor_Instantiate.Entry{prefab=Load<GameObject>(Root+"Prefabs/FX_HW_"+names[i]+".prefab"),lifetime=3,alignToSurface=true},new Actor_Instantiate.Entry{prefab=Load<GameObject>(Root+"Prefabs/Decal_HW_"+names[i]+".prefab"),lifetime=18,alignToSurface=true}};
                var sense=new GameObject("Bullet detection"); sense.transform.SetParent(go.transform,false); var collider=sense.AddComponent<SphereCollider>(); collider.radius=.7f; collider.isTrigger=true;
                var trigger=sense.AddComponent<Trigger_OnTriggerTag>(); trigger.RequiredTag="Bullet"; trigger.OnEnter=new GameObjectEvent(); trigger.OnExit=new GameObjectEvent();
                var meter=go.AddComponent<W06TargetTelemetry>(); meter.materialName=names[i];
                UnityEventTools.AddPersistentListener(trigger.OnEnter,actor.InstantiateAtObject); UnityEventTools.AddPersistentListener(trigger.OnEnter,meter.RegisterHit);
                targets.Add(go.transform);
            }
            session.targets=targets.ToArray(); session.projectile=Load<GameObject>(Root+"Prefabs/Projectile_Bullet_HW.prefab").GetComponent<Projectile_Bullet>(); session.rifle=Load<SO_ItemData>(Root+"Prefabs/SO_Rifle_HW.asset");
            var player=(GameObject)PrefabUtility.InstantiatePrefab(Load<GameObject>("Assets/00_Framework_DoNotOpen/Player/Character_VFX.prefab")); player.name="Player_HW"; player.transform.SetPositionAndRotation(Origin+new Vector3(0,1,-3),Quaternion.identity); session.player=player;
            foreach(var cam in player.GetComponentsInChildren<Camera>(true)) {cam.tag="MainCamera";cam.fieldOfView=58; cam.farClipPlane=80; cam.backgroundColor=new Color(.025f,.035f,.05f); cam.clearFlags=CameraClearFlags.SolidColor; cam.GetUniversalAdditionalCameraData().renderPostProcessing=true;}
            ConfigureMovement(player);
            var dc=new GameObject("Showcase camera").AddComponent<Camera>(); dc.transform.position=Origin+new Vector3(0,3,-5); dc.transform.LookAt(Origin+new Vector3(0,2,6)); dc.fieldOfView=49; dc.farClipPlane=80; dc.clearFlags=CameraClearFlags.SolidColor; dc.backgroundColor=new Color(.025f,.035f,.05f); dc.allowHDR=true; dc.GetUniversalAdditionalCameraData().renderPostProcessing=true; dc.gameObject.SetActive(false); session.showcaseCamera=dc;
            session.statusLabel=BuildHud();
            Time.fixedDeltaTime=.01f; PlayerSettings.runInBackground=true;
            var probe=new GameObject("Lab reflection").AddComponent<ReflectionProbe>(); probe.transform.position=Origin+new Vector3(0,2.5f,4); probe.size=new Vector3(19,8,22); probe.mode=ReflectionProbeMode.Realtime; probe.refreshMode=ReflectionProbeRefreshMode.OnAwake; probe.resolution=128; probe.clearFlags=ReflectionProbeClearFlags.SolidColor; probe.backgroundColor=new Color(.15f,.2f,.25f);
            EditorSceneManager.SaveScene(scene,Root+"W06_HW.unity"); EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Root+"W06_HW.unity",true)};
            AssetDatabase.SaveAssets();
            if(SceneView.lastActiveSceneView) SceneView.lastActiveSceneView.LookAt(Origin+new Vector3(0,2,4),Quaternion.Euler(12,0,0),14);
            return "Saved W06_HW at new lab zone (24,0,-36), three targets and course rifle.";
        }

        public static string Polish()
        {
            PolishMaterials();
            GameObject.Find("TRACE / IMPACT LAB").transform.localPosition=new Vector3(-7.7f,5.32f,8.65f);
            GameObject.Find("MATERIAL RESPONSE STUDY   /   06").transform.localPosition=new Vector3(-7.65f,4.95f,8.64f);
            var player=GameObject.Find("Player_HW"); ConfigureMovement(player);
            foreach(var camera in player.GetComponentsInChildren<Camera>(true)) camera.tag="MainCamera";
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes(); AssetDatabase.SaveAssets(); return "Steel, wood grain, concrete texture and header spacing updated; WASD configured.";
        }
        static void ConfigureMovement(GameObject player)
        {
            foreach(var mover in player.GetComponentsInChildren<Player_MoveController>(true))
            {
                var move=new UnityEngine.InputSystem.InputAction("Move",UnityEngine.InputSystem.InputActionType.Value);
                move.AddCompositeBinding("2DVector").With("Up","<Keyboard>/w").With("Down","<Keyboard>/s").With("Left","<Keyboard>/a").With("Right","<Keyboard>/d");
                mover.MoveAction=new UnityEngine.InputSystem.InputActionProperty(move);
            }
        }
        static void PolishMaterials()
        {
            var lit=Shader.Find("Universal Render Pipeline/Lit");
            var metal=Load<Material>(Root+"Materials/Mat_Metal_HW.mat");metal.shader=lit;metal.SetTexture("_BaseMap",null);metal.SetColor("_BaseColor",new Color(.23f,.31f,.36f));metal.SetFloat("_Metallic",.92f);metal.SetFloat("_Smoothness",.67f);EditorUtility.SetDirty(metal);
            var wood=Load<Material>(Root+"Materials/Mat_Wood_HW.mat");wood.shader=lit;wood.SetTexture("_BaseMap",Load<Texture2D>("Assets/W06_TraceAndMark/Tex_Wood_pexels-wood-1866654_1920.jpg"));wood.SetColor("_BaseColor",new Color(.52f,.3f,.13f));wood.SetFloat("_Metallic",0);wood.SetFloat("_Smoothness",.16f);EditorUtility.SetDirty(wood);
            var concrete=Load<Material>(Root+"Materials/Mat_Concrete_HW.mat");concrete.SetTexture("_BaseMap",Load<Texture2D>("Assets/90_ThirdParty_ReadOnly/Industrial_Zone_Modular_Pack/Textures/Tiles_Walls/Wall_Concrete_Rough/Wall_Concrete_Rough.png"));concrete.SetColor("_BaseColor",new Color(.7f,.72f,.74f));concrete.SetTextureScale("_BaseMap",new Vector2(2,1));concrete.SetTexture("_BumpMap",Load<Texture2D>("Assets/90_ThirdParty_ReadOnly/Industrial_Zone_Modular_Pack/Textures/Tiles_Walls/Wall_Concrete_Rough/Wall_Concrete_Rough_Normal.png"));concrete.EnableKeyword("_NORMALMAP");concrete.SetFloat("_BumpScale",.38f);EditorUtility.SetDirty(concrete);
        }

        static Material Mat(string name,Color color,float metallic,float smoothness,Color? emission=null)
        {
            string path=Root+"Materials/"+name+".mat"; var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!mat){mat=new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(mat,path);}
            mat.SetColor("_BaseColor",color); mat.SetFloat("_Metallic",metallic); mat.SetFloat("_Smoothness",smoothness);
            if(emission.HasValue){mat.EnableKeyword("_EMISSION");mat.SetColor("_EmissionColor",emission.Value);}
            EditorUtility.SetDirty(mat); return mat;
        }
        static GameObject Block(string name,Vector3 pos,Vector3 scale,Material mat,bool collider=true)
        {var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(environment,false);go.transform.localPosition=pos;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=mat;if(!collider)UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());return go;}
        static void WorldText(string text,Vector3 position,float size,Color color,TextAlignmentOptions alignment)
        {var go=new GameObject(text);go.transform.SetParent(environment,false);go.transform.localPosition=position;var t=go.AddComponent<TextMeshPro>();t.font=font;t.text=text;t.fontSize=size*10;t.color=color;t.alignment=alignment;t.rectTransform.sizeDelta=new Vector2(alignment==TextAlignmentOptions.Left?15:4,1);if(alignment==TextAlignmentOptions.Left)t.rectTransform.pivot=new Vector2(0,.5f);t.textWrappingMode=TextWrappingModes.NoWrap;}
        static void Ring(Vector3 center,float radius,Material mat)
        {var go=new GameObject("Impact ring");go.transform.SetParent(environment,false);var line=go.AddComponent<LineRenderer>();line.useWorldSpace=false;line.loop=true;line.positionCount=80;line.widthMultiplier=.018f;line.sharedMaterial=mat;for(int i=0;i<80;i++){float a=i*Mathf.PI*2/80;line.SetPosition(i,center+new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,0));}}
        static void LightAt(string name,Vector3 pos,Vector3 target,Color color,float intensity,float range,LightType type)
        {var light=new GameObject(name).AddComponent<Light>();light.transform.position=Origin+pos;light.transform.LookAt(Origin+target);light.type=type;light.color=color;light.intensity=intensity;light.range=range;light.spotAngle=70;light.shadows=LightShadows.None;}
        static TMP_Text BuildHud()
        {
            var go=new GameObject("Lab HUD",typeof(Canvas),typeof(UnityEngine.UI.CanvasScaler));var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=go.GetComponent<UnityEngine.UI.CanvasScaler>();scaler.uiScaleMode=UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
            var status=HudText(go.transform,"Status",new Vector2(46,-40),new Vector2(1350,48),new Vector2(0,1),28,new Color(.83f,.91f,.95f));status.text="LIVE FIRE / THREE MATERIALS";
            var foot=HudText(go.transform,"Controls",new Vector2(46,30),new Vector2(1600,44),Vector2.zero,21,new Color(.59f,.7f,.77f));foot.text="WASD  MOVE     MOUSE  LOOK / FIRE     R  CLEAR MARKS     F6  SHOWCASE     ESC  CURSOR";
            var cross=HudText(go.transform,"Crosshair",Vector2.zero,new Vector2(40,40),new Vector2(.5f,.5f),28,new Color(.85f,.95f,1,.6f));cross.alignment=TextAlignmentOptions.Center;cross.text="+";return status;
        }
        static TextMeshProUGUI HudText(Transform parent,string name,Vector2 pos,Vector2 size,Vector2 anchor,float fontSize,Color color)
        {var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);var rt=(RectTransform)go.transform;rt.anchorMin=rt.anchorMax=anchor;rt.pivot=anchor;rt.anchoredPosition=pos;rt.sizeDelta=size;var t=go.AddComponent<TextMeshProUGUI>();t.font=font;t.fontSize=fontSize;t.color=color;t.raycastTarget=false;return t;}
        static void ConfigurePipeline()
        {
            var source=(UniversalRenderPipelineAsset)(QualitySettings.renderPipeline?QualitySettings.renderPipeline:GraphicsSettings.defaultRenderPipeline);
            if(!source) throw new Exception("No source URP asset.");
            Copy(AssetDatabase.GetAssetPath(source),Root+"Render/Lab_URP.asset");var urp=Load<UniversalRenderPipelineAsset>(Root+"Render/Lab_URP.asset");
            var so=new SerializedObject(urp);var list=so.FindProperty("m_RendererDataList");var sourceRenderer=(ScriptableRendererData)list.GetArrayElementAtIndex(0).objectReferenceValue;
            Copy(AssetDatabase.GetAssetPath(sourceRenderer),Root+"Render/Lab_Renderer.asset");var renderer=Load<ScriptableRendererData>(Root+"Render/Lab_Renderer.asset");
            if(!renderer.rendererFeatures.Any(x=>x is DecalRendererFeature)) {var feature=ScriptableObject.CreateInstance<DecalRendererFeature>();feature.name="Decals";AssetDatabase.AddObjectToAsset(feature,renderer);renderer.rendererFeatures.Add(feature);}
            renderer.SetDirty();EditorUtility.SetDirty(renderer);list.GetArrayElementAtIndex(0).objectReferenceValue=renderer;so.ApplyModifiedPropertiesWithoutUndo();
            urp.supportsHDR=true;urp.msaaSampleCount=4;urp.shadowDistance=40;urp.supportsCameraDepthTexture=true;EditorUtility.SetDirty(urp);QualitySettings.renderPipeline=urp;GraphicsSettings.defaultRenderPipeline=urp;
        }
    }
}
