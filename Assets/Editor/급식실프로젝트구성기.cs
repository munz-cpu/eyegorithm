#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class 급식실프로젝트구성기
{
    private const string 씬경로 = "Assets/Scenes/SampleScene.unity";
    private const string 학생프리팹경로 = "Assets/Prefabs/학생.prefab";
    private const string 시스템이름 = "급식실 시뮬레이션";
    private static readonly Color 진한색 = new(0.12f, 0.15f, 0.2f, 0.96f);
    private static readonly Color 강조색 = new(0.25f, 0.58f, 0.92f, 1f);
    private static readonly Color 보조색 = new(0.22f, 0.26f, 0.34f, 1f);

    static 급식실프로젝트구성기()
    {
        EditorApplication.delayCall += 자동구성;
    }

    [MenuItem("Eyegorithm/급식실 시뮬레이션 구성 또는 복구")]
    public static void 수동구성()
    {
        구성(true);
    }

    public static void 배치구성()
    {
        EditorSceneManager.OpenScene(씬경로, OpenSceneMode.Single);
        구성(false);
    }

    // 처음 스크립트가 컴파일됐을 때 현재 SampleScene에 영구 오브젝트를 한 번 구성한다.
    private static void 자동구성()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            return;

        Scene 현재씬 = SceneManager.GetActiveScene();
        if (현재씬.path != 씬경로 || GameObject.Find(시스템이름) != null)
            return;

        구성(false);
    }

    private static void 구성(bool 기존구성복구)
    {
        Scene 현재씬 = SceneManager.GetActiveScene();
        if (현재씬.path != 씬경로)
        {
            EditorUtility.DisplayDialog("급식실 구성", "SampleScene을 연 뒤 다시 실행해 주세요.", "확인");
            return;
        }

        학생프리팹구성();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        GameObject 기존루트 = GameObject.Find(시스템이름);
        if (기존루트 != null)
        {
            if (!기존구성복구)
                return;

            if (!EditorUtility.DisplayDialog("급식실 구성", "기존 '급식실 시뮬레이션' 오브젝트를 지우고 다시 구성할까요?", "다시 구성", "취소"))
                return;

            Undo.DestroyObjectImmediate(기존루트);
        }

        GameObject 루트 = 새오브젝트(시스템이름, null);
        GameObject 시스템 = 새오브젝트("시스템", 루트.transform);
        GameObject 월드 = 새오브젝트("월드 위치와 생성 학생", 루트.transform);
        GameObject 학생부모 = 새오브젝트("생성된 학생들", 월드.transform);
        GameObject 위치들 = 새오브젝트("이동 위치", 월드.transform);

        AudioSource 배경음 = 시스템.AddComponent<AudioSource>();
        배경음.loop = true;
        배경음.playOnAwake = true;

        설정관리자 설정 = 시스템.AddComponent<설정관리자>();
        급식실관리자 급식실 = 시스템.AddComponent<급식실관리자>();
        학생소환기 소환기 = 시스템.AddComponent<학생소환기>();

        Transform 소환위치 = 위치만들기("학생 소환 위치", 위치들.transform, new Vector3(-7f, -3.1f));
        Transform 줄시작 = 위치만들기("줄 시작 위치", 위치들.transform, new Vector3(-4.2f, -2.2f));
        Transform 입구 = 위치만들기("급식실 입구", 위치들.transform, new Vector3(-4.88f, 1.54f));
        Transform 입구안쪽 = 위치만들기("급식실 입구 안쪽", 위치들.transform, new Vector3(-4.88f, 2.13f));
        Transform 출구 = 위치만들기("급식실 출구", 위치들.transform, new Vector3(4.45f, 1.54f));
        Transform 퇴장 = 위치만들기("식사 완료 퇴장 위치", 위치들.transform, new Vector3(4.45f, -4.33f));
        Transform[] 좌석 = 좌석만들기(위치들.transform);

        Collider2D 이동범위 = GameObject.Find("학생이동범위")?.GetComponent<Collider2D>();
        학생정보 학생프리팹 = AssetDatabase.LoadAssetAtPath<GameObject>(학생프리팹경로)?.GetComponent<학생정보>();
        TextAsset 이름파일 = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/studentName.json");
        Sprite 기본학생이미지 = 학생프리팹 != null ? 학생프리팹.GetComponentInChildren<SpriteRenderer>()?.sprite : null;

        GameObject UI루트 = UI구성(루트.transform, 설정, 소환기, 급식실, out 경고메시지UI 경고UI);
        UI루트.transform.SetAsLastSibling();

        설정연결(설정, 배경음);
        급식실연결(급식실, 설정, 이동범위, 경고UI, 줄시작, 입구, 입구안쪽, 출구, 퇴장, 좌석);
        소환기연결(소환기, 설정, 급식실, 학생프리팹, 학생부모.transform, 소환위치, 이동범위, 이름파일, 기본학생이미지);

        EditorSceneManager.MarkSceneDirty(현재씬);
        EditorSceneManager.SaveScene(현재씬);
        Selection.activeGameObject = 루트;
        Debug.Log("급식실 시뮬레이션 씬과 학생 프리팹 구성이 완료되었습니다.");
    }

    // 학생 프리팹에 정보 컴포넌트와 이름/남은 시간 표시를 실제 자식 오브젝트로 추가한다.
    private static void 학생프리팹구성()
    {
        GameObject 루트 = PrefabUtility.LoadPrefabContents(학생프리팹경로);
        if (루트 == null)
            return;

        학생정보 정보 = 루트.GetComponent<학생정보>() ?? 루트.AddComponent<학생정보>();
        SpriteRenderer 학생이미지 = 루트.transform.Find("학생생김새")?.GetComponent<SpriteRenderer>() ?? 루트.GetComponentInChildren<SpriteRenderer>();
        TMP_FontAsset 폰트 = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/SeoulHangangB SDF.asset");

        TextMeshPro 이름표 = 월드텍스트만들기(루트.transform, "학생 이름과 계급", new Vector3(0f, 0.82f, 0f), 3.2f, 폰트);
        TextMeshPro 남은시간 = 월드텍스트만들기(루트.transform, "남은 실행 시간", new Vector3(0f, -0.78f, 0f), 2.8f, 폰트);

        SerializedObject 직렬화 = new(정보);
        객체설정(직렬화, "학생이미지", 학생이미지);
        객체설정(직렬화, "이름표", 이름표);
        객체설정(직렬화, "남은시간표", 남은시간);
        직렬화.ApplyModifiedPropertiesWithoutUndo();

        Rigidbody2D 몸체 = 루트.GetComponent<Rigidbody2D>();
        if (몸체 != null)
        {
            몸체.gravityScale = 0f;
            몸체.freezeRotation = true;
            몸체.interpolation = RigidbodyInterpolation2D.Interpolate;
            몸체.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        PrefabUtility.SaveAsPrefabAsset(루트, 학생프리팹경로);
        PrefabUtility.UnloadPrefabContents(루트);
    }

    private static TextMeshPro 월드텍스트만들기(Transform 부모, string 이름, Vector3 위치, float 크기, TMP_FontAsset 폰트)
    {
        Transform 기존 = 부모.Find(이름);
        GameObject 오브젝트 = 기존 != null ? 기존.gameObject : 새오브젝트(이름, 부모);
        TextMeshPro 텍스트 = 오브젝트.GetComponent<TextMeshPro>() ?? 오브젝트.AddComponent<TextMeshPro>();
        텍스트.font = 폰트;
        텍스트.fontSize = 크기;
        텍스트.alignment = TextAlignmentOptions.Center;
        텍스트.color = Color.white;
        텍스트.sortingOrder = 20;
        텍스트.text = 이름;
        텍스트.rectTransform.sizeDelta = new Vector2(4f, 0.6f);
        텍스트.transform.localPosition = 위치;
        텍스트.transform.localScale = Vector3.one * 0.25f;
        return 텍스트;
    }

    private static GameObject UI구성(Transform 부모, 설정관리자 설정, 학생소환기 소환기, 급식실관리자 급식실, out 경고메시지UI 경고UI)
    {
        GameObject UI루트 = 새오브젝트("급식실 UI", 부모, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas 캔버스 = UI루트.GetComponent<Canvas>();
        캔버스.renderMode = RenderMode.ScreenSpaceOverlay;
        캔버스.sortingOrder = 100;
        CanvasScaler 스케일러 = UI루트.GetComponent<CanvasScaler>();
        스케일러.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        스케일러.referenceResolution = new Vector2(1920f, 1080f);
        스케일러.matchWidthOrHeight = 0.5f;

        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject 이벤트 = 새오브젝트("EventSystem", 부모, typeof(EventSystem), typeof(InputSystemUIInputModule));
            이벤트.transform.SetAsLastSibling();
        }

        Button 열기버튼 = 버튼만들기(UI루트.transform, "급식실 OPEN CLOSE 버튼", "급식실 OPEN", new Vector2(32f, 108f), new Vector2(250f, 58f), new Vector2(0f, 0f), 강조색);
        Button 소환버튼 = 버튼만들기(UI루트.transform, "학생 소환 버튼", "학생 소환", new Vector2(32f, 34f), new Vector2(250f, 58f), new Vector2(0f, 0f), 강조색);
        TMP_Dropdown 계급드롭다운 = 드롭다운만들기(UI루트.transform, "소환 계급 드롭다운", new[] { "1학년", "2학년", "3학년", "학생회장", "교장선생님" }, new Vector2(300f, 34f), new Vector2(230f, 58f));
        Button 설정버튼 = 버튼만들기(UI루트.transform, "설정 열기 버튼", "설정", new Vector2(-32f, 34f), new Vector2(110f, 58f), new Vector2(1f, 0f), 보조색);

        GameObject 경고 = 패널만들기(UI루트.transform, "경고 메시지", new Vector2(0f, -38f), new Vector2(760f, 64f), new Vector2(0.5f, 1f), new Color(0.65f, 0.12f, 0.12f, 0.94f));
        CanvasGroup 경고그룹 = 경고.AddComponent<CanvasGroup>();
        TMP_Text 경고문구 = 텍스트만들기(경고.transform, "경고 문구", "경고", 28f, TextAlignmentOptions.Center);
        전체채우기(경고문구.rectTransform, new Vector2(18f, 8f));
        경고UI = 경고.AddComponent<경고메시지UI>();
        직렬화연결(경고UI, new Dictionary<string, Object>
        {
            ["경고문구"] = 경고문구,
            ["캔버스그룹"] = 경고그룹
        });

        GameObject 설정패널 = 설정패널만들기(UI루트.transform, 설정, 설정버튼, out 설정창UI 설정UI);

        급식실상태UI 상태UI = UI루트.AddComponent<급식실상태UI>();
        직렬화연결(상태UI, new Dictionary<string, Object>
        {
            ["설정"] = 설정,
            ["열기닫기버튼"] = 열기버튼,
            ["버튼문구"] = 열기버튼.GetComponentInChildren<TMP_Text>()
        });

        학생소환UI 소환UI = UI루트.AddComponent<학생소환UI>();
        직렬화연결(소환UI, new Dictionary<string, Object>
        {
            ["소환기"] = 소환기,
            ["소환버튼"] = 소환버튼,
            ["계급드롭다운"] = 계급드롭다운
        });

        설정패널.SetActive(false);
        return UI루트;
    }

    private static GameObject 설정패널만들기(Transform 부모, 설정관리자 설정, Button 열기버튼, out 설정창UI 설정UI)
    {
        GameObject 패널 = 패널만들기(부모, "설정 패널", new Vector2(-32f, 108f), new Vector2(560f, 690f), new Vector2(1f, 0f), 진한색);
        TMP_Text 제목 = 텍스트만들기(패널.transform, "제목", "시뮬레이션 설정", 34f, TextAlignmentOptions.Center);
        배치(제목.rectTransform, new Vector2(0f, -38f), new Vector2(500f, 52f), new Vector2(0.5f, 1f));
        Button 닫기 = 버튼만들기(패널.transform, "닫기 버튼", "닫기", new Vector2(-18f, -18f), new Vector2(92f, 44f), new Vector2(1f, 1f), 보조색);

        TMP_Dropdown 큐 = 드롭다운만들기(패널.transform, "큐 종류", new[] { "일반", "FCFS", "라운드 로빈", "최소 실행시간 우선", "우선순위" }, new Vector2(250f, -116f), new Vector2(270f, 48f), new Vector2(0f, 1f));
        텍스트행(패널.transform, "큐 종류", -116f);

        Slider 배경음 = 슬라이더만들기(패널.transform, "BGM 볼륨", 0f, 1f, 0.7f, -186f, false);
        텍스트행(패널.transform, "BGM", -186f);
        Slider 효과음 = 슬라이더만들기(패널.transform, "학생 효과음 볼륨", 0f, 1f, 1f, -256f, false);
        텍스트행(패널.transform, "학생 효과음", -256f);
        Slider 속도 = 슬라이더만들기(패널.transform, "틱 속도", 0.25f, 3f, 1f, -326f, false);
        텍스트행(패널.transform, "Speed", -326f);
        TMP_Text 속도값 = 값텍스트(패널.transform, "틱 속도 값", -326f);
        Slider 최대인원 = 슬라이더만들기(패널.transform, "최대 사람 수", 1f, 15f, 15f, -396f, true);
        텍스트행(패널.transform, "최대 사람 수", -396f);
        TMP_Text 최대인원값 = 값텍스트(패널.transform, "최대 인원 값", -396f);
        Slider 자리수 = 슬라이더만들기(패널.transform, "자리 수", 1f, 5f, 1f, -466f, true);
        텍스트행(패널.transform, "자리 수", -466f);
        TMP_Text 자리수값 = 값텍스트(패널.transform, "자리 수 값", -466f);
        Toggle 남은시간 = 토글만들기(패널.transform, "남은 실행시간 표시", true, -536f);
        텍스트행(패널.transform, "남은 실행시간 표시", -536f);

        설정UI = 부모.gameObject.AddComponent<설정창UI>();
        직렬화연결(설정UI, new Dictionary<string, Object>
        {
            ["설정"] = 설정,
            ["열기버튼"] = 열기버튼,
            ["닫기버튼"] = 닫기,
            ["설정패널"] = 패널,
            ["큐드롭다운"] = 큐,
            ["배경음슬라이더"] = 배경음,
            ["학생효과음슬라이더"] = 효과음,
            ["틱속도슬라이더"] = 속도,
            ["최대인원슬라이더"] = 최대인원,
            ["자리수슬라이더"] = 자리수,
            ["남은시간토글"] = 남은시간,
            ["틱속도값"] = 속도값,
            ["최대인원값"] = 최대인원값,
            ["자리수값"] = 자리수값
        });

        return 패널;
    }

    private static Transform[] 좌석만들기(Transform 부모)
    {
        Vector3[] 위치 =
        {
            new(-1.1f, 2.13f), new(1.15f, 2.13f), new(3.3f, 2.13f),
            new(0f, 3.1f), new(2.2f, 3.1f)
        };
        Transform[] 결과 = new Transform[위치.Length];

        for (int i = 0; i < 위치.Length; i++)
            결과[i] = 위치만들기($"급식 자리 {i + 1}", 부모, 위치[i]);

        return 결과;
    }

    private static void 설정연결(설정관리자 설정, AudioSource 배경음)
    {
        SerializedObject 직렬화 = new(설정);
        객체설정(직렬화, "배경음소스", 배경음);
        직렬화.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void 급식실연결(급식실관리자 급식실, 설정관리자 설정, Collider2D 이동범위, 경고메시지UI 경고, Transform 줄시작, Transform 입구, Transform 입구안쪽, Transform 출구, Transform 퇴장, Transform[] 좌석)
    {
        SerializedObject 직렬화 = new(급식실);
        객체설정(직렬화, "설정", 설정);
        객체설정(직렬화, "학생이동범위", 이동범위);
        객체설정(직렬화, "경고UI", 경고);
        객체설정(직렬화, "줄시작점", 줄시작);
        객체설정(직렬화, "급식실입구", 입구);
        객체설정(직렬화, "급식실입구안쪽", 입구안쪽);
        객체설정(직렬화, "급식실출구", 출구);
        객체설정(직렬화, "퇴장위치", 퇴장);
        SerializedProperty 배열 = 직렬화.FindProperty("자리위치");
        배열.arraySize = 좌석.Length;
        for (int i = 0; i < 좌석.Length; i++)
            배열.GetArrayElementAtIndex(i).objectReferenceValue = 좌석[i];
        직렬화.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void 소환기연결(학생소환기 소환기, 설정관리자 설정, 급식실관리자 급식실, 학생정보 프리팹, Transform 학생부모, Transform 소환위치, Collider2D 이동범위, TextAsset 이름파일, Sprite 기본이미지)
    {
        SerializedObject 직렬화 = new(소환기);
        객체설정(직렬화, "설정", 설정);
        객체설정(직렬화, "급식실", 급식실);
        객체설정(직렬화, "학생프리팹", 프리팹);
        객체설정(직렬화, "학생부모", 학생부모);
        객체설정(직렬화, "소환위치", 소환위치);
        객체설정(직렬화, "이동범위", 이동범위);
        객체설정(직렬화, "학생이름파일", 이름파일);
        SerializedProperty 목록 = 직렬화.FindProperty("계급별이미지");
        목록.arraySize = 5;
        for (int i = 0; i < 5; i++)
        {
            SerializedProperty 항목 = 목록.GetArrayElementAtIndex(i);
            항목.FindPropertyRelative("계급").enumValueIndex = i;
            항목.FindPropertyRelative("이미지").objectReferenceValue = 기본이미지;
        }
        직렬화.ApplyModifiedPropertiesWithoutUndo();
    }

    private static GameObject 새오브젝트(string 이름, Transform 부모, params System.Type[] 컴포넌트)
    {
        GameObject 오브젝트 = 컴포넌트.Length == 0 ? new GameObject(이름) : new GameObject(이름, 컴포넌트);
        Undo.RegisterCreatedObjectUndo(오브젝트, 이름);
        if (부모 != null)
            오브젝트.transform.SetParent(부모, false);
        return 오브젝트;
    }

    private static Transform 위치만들기(string 이름, Transform 부모, Vector3 위치)
    {
        Transform 결과 = 새오브젝트(이름, 부모).transform;
        결과.position = 위치;
        return 결과;
    }

    private static GameObject 패널만들기(Transform 부모, string 이름, Vector2 위치, Vector2 크기, Vector2 앵커, Color 색)
    {
        GameObject 패널 = 새오브젝트(이름, 부모, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        패널.GetComponent<Image>().color = 색;
        배치(패널.GetComponent<RectTransform>(), 위치, 크기, 앵커);
        return 패널;
    }

    private static Button 버튼만들기(Transform 부모, string 이름, string 문구, Vector2 위치, Vector2 크기, Vector2 앵커, Color 색)
    {
        GameObject 오브젝트 = TMP_DefaultControls.CreateButton(기본리소스());
        오브젝트.name = 이름;
        오브젝트.transform.SetParent(부모, false);
        오브젝트.GetComponent<Image>().color = 색;
        TMP_Text 텍스트 = 오브젝트.GetComponentInChildren<TMP_Text>();
        텍스트.text = 문구;
        텍스트.fontSize = 24f;
        텍스트.color = Color.white;
        폰트설정(텍스트);
        배치(오브젝트.GetComponent<RectTransform>(), 위치, 크기, 앵커);
        return 오브젝트.GetComponent<Button>();
    }

    private static TMP_Dropdown 드롭다운만들기(Transform 부모, string 이름, string[] 항목, Vector2 위치, Vector2 크기, Vector2? 앵커값 = null)
    {
        GameObject 오브젝트 = TMP_DefaultControls.CreateDropdown(기본리소스());
        오브젝트.name = 이름;
        오브젝트.transform.SetParent(부모, false);
        TMP_Dropdown 드롭다운 = 오브젝트.GetComponent<TMP_Dropdown>();
        드롭다운.ClearOptions();
        드롭다운.AddOptions(new List<string>(항목));
        드롭다운.RefreshShownValue();
        foreach (TMP_Text 텍스트 in 오브젝트.GetComponentsInChildren<TMP_Text>(true))
        {
            텍스트.fontSize = 20f;
            폰트설정(텍스트);
        }
        배치(오브젝트.GetComponent<RectTransform>(), 위치, 크기, 앵커값 ?? Vector2.zero);
        return 드롭다운;
    }

    private static Slider 슬라이더만들기(Transform 부모, string 이름, float 최소, float 최대, float 값, float y, bool 정수)
    {
        GameObject 오브젝트 = DefaultControls.CreateSlider(기본UGUI리소스());
        오브젝트.name = 이름;
        오브젝트.transform.SetParent(부모, false);
        Slider 슬라이더 = 오브젝트.GetComponent<Slider>();
        슬라이더.minValue = 최소;
        슬라이더.maxValue = 최대;
        슬라이더.wholeNumbers = 정수;
        슬라이더.value = 값;
        배치(오브젝트.GetComponent<RectTransform>(), new Vector2(250f, y), new Vector2(250f, 34f), new Vector2(0f, 1f));
        return 슬라이더;
    }

    private static Toggle 토글만들기(Transform 부모, string 이름, bool 값, float y)
    {
        GameObject 오브젝트 = DefaultControls.CreateToggle(기본UGUI리소스());
        오브젝트.name = 이름;
        오브젝트.transform.SetParent(부모, false);
        Toggle 토글 = 오브젝트.GetComponent<Toggle>();
        토글.isOn = 값;
        TMP_Text 기본문구 = 오브젝트.GetComponentInChildren<TMP_Text>();
        if (기본문구 != null)
            기본문구.text = string.Empty;
        Text 일반문구 = 오브젝트.GetComponentInChildren<Text>();
        if (일반문구 != null)
            일반문구.text = string.Empty;
        배치(오브젝트.GetComponent<RectTransform>(), new Vector2(250f, y), new Vector2(48f, 48f), new Vector2(0f, 1f));
        return 토글;
    }

    private static TMP_Text 텍스트만들기(Transform 부모, string 이름, string 문구, float 크기, TextAlignmentOptions 정렬)
    {
        GameObject 오브젝트 = 새오브젝트(이름, 부모, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        TMP_Text 텍스트 = 오브젝트.GetComponent<TMP_Text>();
        텍스트.text = 문구;
        텍스트.fontSize = 크기;
        텍스트.alignment = 정렬;
        텍스트.color = Color.white;
        폰트설정(텍스트);
        return 텍스트;
    }

    private static void 텍스트행(Transform 부모, string 문구, float y)
    {
        TMP_Text 텍스트 = 텍스트만들기(부모, $"{문구} 라벨", 문구, 23f, TextAlignmentOptions.MidlineLeft);
        배치(텍스트.rectTransform, new Vector2(28f, y), new Vector2(205f, 44f), new Vector2(0f, 1f));
    }

    private static TMP_Text 값텍스트(Transform 부모, string 이름, float y)
    {
        TMP_Text 텍스트 = 텍스트만들기(부모, 이름, "-", 19f, TextAlignmentOptions.MidlineRight);
        배치(텍스트.rectTransform, new Vector2(-22f, y), new Vector2(88f, 40f), new Vector2(1f, 1f));
        return 텍스트;
    }

    private static void 배치(RectTransform 사각형, Vector2 위치, Vector2 크기, Vector2 앵커)
    {
        사각형.anchorMin = 앵커;
        사각형.anchorMax = 앵커;
        사각형.pivot = 앵커;
        사각형.anchoredPosition = 위치;
        사각형.sizeDelta = 크기;
    }

    private static void 전체채우기(RectTransform 사각형, Vector2 여백)
    {
        사각형.anchorMin = Vector2.zero;
        사각형.anchorMax = Vector2.one;
        사각형.offsetMin = 여백;
        사각형.offsetMax = -여백;
    }

    private static void 폰트설정(TMP_Text 텍스트)
    {
        TMP_FontAsset 폰트 = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/SeoulHangangB SDF.asset");
        if (폰트 != null)
            텍스트.font = 폰트;
    }

    private static TMP_DefaultControls.Resources 기본리소스()
    {
        return new TMP_DefaultControls.Resources
        {
            standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),
            background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"),
            inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd"),
            knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"),
            checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd"),
            dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/DropdownArrow.psd"),
            mask = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd")
        };
    }

    private static DefaultControls.Resources 기본UGUI리소스()
    {
        return new DefaultControls.Resources
        {
            standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),
            background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"),
            inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd"),
            knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"),
            checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd"),
            dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/DropdownArrow.psd")
        };
    }

    private static void 직렬화연결(Object 대상, Dictionary<string, Object> 값들)
    {
        SerializedObject 직렬화 = new(대상);
        foreach ((string 이름, Object 값) in 값들)
            객체설정(직렬화, 이름, 값);
        직렬화.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void 객체설정(SerializedObject 직렬화, string 이름, Object 값)
    {
        SerializedProperty 속성 = 직렬화.FindProperty(이름);
        if (속성 != null)
            속성.objectReferenceValue = 값;
    }
}
#endif
