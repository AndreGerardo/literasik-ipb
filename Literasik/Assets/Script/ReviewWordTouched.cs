using UnityEngine;
using TMPro;

public class ReviewWordTouched : MonoBehaviour
{
    [Header("TextMeshPro Reference")]
    [SerializeField] private TMP_Text articleText;
    [SerializeField] private TMP_Text errorGeneratedText;
    [SerializeField] private int selectedWord = -1;

    [Header("Game Reference")]
    [SerializeField] private LevelErrorGenerator EG;
    [SerializeField] private LevelManager LM;
    [SerializeField] private float deltaTouchErrorMagnitude = 0.25f;
    [SerializeField] private GameObject reviewGroup;
    Vector3 lastMousePos;

    [Header("Correction Reference")]
    [SerializeField] private CanvasGroup correctionPanel;
    [SerializeField] private TMP_Text errorTypeText, correctionText;
    bool canTouch = true;
    private AudioSource correctionAudio;
    private Camera cam;

    public void InitializeReview()
    {
        cam = Camera.main;

        articleText.text = errorGeneratedText.text;
        articleText.ForceMeshUpdate();
        // Thread.Sleep(100);
        UpdateWordColor();
        // articleText.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
        reviewGroup.GetComponent<CanvasGroup>().alpha = 0f;
        reviewGroup.GetComponent<CanvasGroup>().blocksRaycasts = false;
        reviewGroup.GetComponent<CanvasGroup>().interactable = false;
        // articleText.ForceMeshUpdate();
        // UpdateWordColor();
    }

    void OnEnable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ON_TEXT_CHANGED);
    }
    void OnDisable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(ON_TEXT_CHANGED);
    }


    void ON_TEXT_CHANGED(Object obj)
    {
        UpdateWordColor();
        EraseChar();
    }

    private void Update()
    {

        //Mouse Input
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            float deltapos = Vector3.Distance(lastMousePos, Input.mousePosition);
            if (deltapos < deltaTouchErrorMagnitude) PickWord(Input.mousePosition);
        }


        //Touch Input
        if (Input.touchCount > 0)
        {
            Touch currentTouch = Input.GetTouch(0);
            if (currentTouch.phase == TouchPhase.Ended && currentTouch.deltaPosition.magnitude < deltaTouchErrorMagnitude)
            {
                PickWord(currentTouch.position);
            }
        }
    }

    // Fungsi untuk mengambil kata yang ditekan
    private void PickWord(Vector3 touchPos)
    {
        // Indeks kata yang ditekan dari teks
        int wordIndex = TMP_TextUtilities.FindIntersectingWord(articleText, touchPos, cam);

        // Mengecek kalau belum ada kata yang ditekan
        if (wordIndex != -1)
        {

            selectedWord = wordIndex;

            TMP_WordInfo wInfo = articleText.textInfo.wordInfo[wordIndex];

            if (EG.textErrorIndexes.Contains(wordIndex) && canTouch)
            {
                canTouch = false;
                Debug.Log("Error Word : " + wInfo.GetWord() + ", correct word : " + EG.correctTextIndexs[EG.textErrorIndexes.IndexOf(wordIndex)] + ", error type : " + EG.errorTypesIndexes[EG.textErrorIndexes.IndexOf(wordIndex)]);
                CorrectionFeedbackPrompt(EG.errorTextList[EG.textErrorIndexes.IndexOf(wordIndex)], EG.correctTextIndexs[EG.textErrorIndexes.IndexOf(wordIndex)], EG.errorTypesIndexes[EG.textErrorIndexes.IndexOf(wordIndex)]);

            }
            else
            {
                // Mengoutput kata yang terambil beserta indeksnya ke console
                Debug.Log("Clicked Text : " + wInfo.GetWord() + ", at index : " + wordIndex);

            }
        }
    }


    private void CorrectionFeedbackPrompt(string errorWord, string correctWord, ErrorType errType)
    {
        string errTypeStr = " ";
        switch (errType)
        {
            case ErrorType.TYPO: errTypeStr = "Salah Ketik"; break;
            case ErrorType.KAPITAL: errTypeStr = "Kapitalisasi Kata"; break;
            case ErrorType.TITIK_KOMA: errTypeStr = "Penggunaan Tanda Baca"; break;
        }

        errorTypeText.text = errTypeStr;
        correctionText.text = errorWord + " > " + correctWord;

        LeanTween.alphaCanvas(correctionPanel, 1f, 0.5f)
            .setEaseInOutSine();
        LeanTween.alphaCanvas(correctionPanel, 0f, 0.5f)
            .setDelay(1.5f)
            .setEaseInOutSine()
            .setOnComplete(() => canTouch = true);
    }

    public void UpdateWordColor()
    {
        // Debug.Log("Changed Color!");

        foreach (var wordIndex in EG.textErrorIndexes)
        {
            TMP_WordInfo info = articleText.textInfo.wordInfo[wordIndex];
            // Debug.Log("UpdatedColor: " + info.GetWord());
            for (int i = 0; i < info.characterCount; ++i)
            {
                int charIndex = info.firstCharacterIndex + i;
                int meshIndex = articleText.textInfo.characterInfo[charIndex].materialReferenceIndex;
                int vertexIndex = articleText.textInfo.characterInfo[charIndex].vertexIndex;

                Color32[] vertexColors = articleText.textInfo.meshInfo[meshIndex].colors32;
                Color32 myColor32 = new Color32(60, 200, 82, 255);
                vertexColors[vertexIndex + 0] = myColor32;
                vertexColors[vertexIndex + 1] = myColor32;
                vertexColors[vertexIndex + 2] = myColor32;
                vertexColors[vertexIndex + 3] = myColor32;

                articleText.UpdateVertexData(TMP_VertexDataUpdateFlags.All);

            }
        }
        // Thread.Sleep(100);
        articleText.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
    }

    private void EraseChar()
    {
        // Debug.Log("Changed Color!");
        if (EG.eraseCharList.Count > 0)
        {
            foreach (var charIndex in EG.eraseCharList)
            {
                int meshIndex = articleText.textInfo.characterInfo[charIndex].materialReferenceIndex;
                int vertexIndex = articleText.textInfo.characterInfo[charIndex].vertexIndex;

                Color32[] vertexColors = articleText.textInfo.meshInfo[meshIndex].colors32;
                Color32 myColor32 = new Color32(0, 0, 0, 0);
                vertexColors[vertexIndex + 0] = myColor32;
                vertexColors[vertexIndex + 1] = myColor32;
                vertexColors[vertexIndex + 2] = myColor32;
                vertexColors[vertexIndex + 3] = myColor32;
            }
            articleText.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
        }
    }

}
