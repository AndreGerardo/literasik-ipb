using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

public class CheckWordTouched : MonoBehaviour
{

    [Header("TextMeshPro Reference")]
    private TMP_Text articleText;
    [SerializeField] private int selectedWord = -1;

    [Header("Game Reference")]
    [SerializeField] private LevelErrorGenerator EG;
    [SerializeField] private LevelManager LM;
    [SerializeField] private float deltaTouchErrorMagnitude = 0.25f;

    private List<int> wordTouched = new List<int>();
    Vector3 lastMousePos;
    [Header("Correction Reference")]
    [SerializeField] private CanvasGroup correctionPanel;
    [SerializeField] private TMP_Text errorTypeText, correctionText;
    private AudioSource correctionAudio;
    [SerializeField] private AudioClip[] correctionSounds;
    private Camera cam;

    // void OnEnable()
    // {
    //     TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ON_TEXT_CHANGED);
    // }
    void OnDisable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(ON_TEXT_CHANGED);
    }


    void ON_TEXT_CHANGED(Object obj)
    {
        UpdateWordColor();
    }

    private void Start()
    {
        articleText = GetComponent<TMP_Text>();
        cam = Camera.main;
        correctionAudio = GetComponent<AudioSource>();
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ON_TEXT_CHANGED);

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
            // UpdateWordColor();
        }


        //Touch Input
        if (Input.touchCount > 0)
        {
            Touch currentTouch = Input.GetTouch(0);
            if (currentTouch.phase == TouchPhase.Ended && currentTouch.deltaPosition.magnitude < deltaTouchErrorMagnitude)
            {
                PickWord(currentTouch.position);
                // UpdateWordColor();
            }
        }

        // Debugging
        if (Input.GetKeyDown(KeyCode.E) && LM.isPlaying) LM.timeRemaining -= 120f;
    }

    // Fungsi untuk mengambil kata yang ditekan
    private void PickWord(Vector3 touchPos)
    {
        // Indeks kata yang ditekan dari teks
        int wordIndex = TMP_TextUtilities.FindIntersectingWord(articleText, touchPos, cam);

        // Mengecek kalau belum ada kata yang ditekan
        if (wordIndex != -1 && wordIndex != selectedWord)
        {
            selectedWord = wordIndex;

            TMP_WordInfo wInfo = articleText.textInfo.wordInfo[wordIndex];

            if (EG.textErrorIndexes.Contains(wordIndex) && !wordTouched.Contains(wordIndex))
            {
                LM.errorFound++;
                wordTouched.Add(wordIndex);

                Debug.Log("Error Word : " + wInfo.GetWord() + ", correct word : " + EG.correctTextIndexs[EG.textErrorIndexes.IndexOf(wordIndex)] + ", error type : " + EG.errorTypesIndexes[EG.textErrorIndexes.IndexOf(wordIndex)]);
                CorrectionFeedbackPrompt(wInfo.GetWord(), EG.correctTextIndexs[EG.textErrorIndexes.IndexOf(wordIndex)], EG.errorTypesIndexes[EG.textErrorIndexes.IndexOf(wordIndex)]);

                if (EG.errorTypesIndexes[EG.textErrorIndexes.IndexOf(wordIndex)] == ErrorType.TITIK_KOMA)
                {
                    ErrorCorrection(EG.articleErrorTextArray[wordIndex], wordIndex);
                }
                else if (EG.errorTypesIndexes[EG.textErrorIndexes.IndexOf(wordIndex)] == ErrorType.KAPITAL)
                {

                    StringBuilder sb = new StringBuilder(articleText.text);
                    char checkCapital = articleText.text[wInfo.firstCharacterIndex];
                    sb[wInfo.firstCharacterIndex] = char.ToUpper(checkCapital);
                    articleText.text = sb.ToString();
                    articleText.ForceMeshUpdate();

                }
                else
                {
                    ErrorCorrection(wInfo.GetWord(), wordIndex);
                }

                // UpdateWordColor();
                articleText.ForceMeshUpdate();

                correctionAudio.clip = correctionSounds[0];
                correctionAudio.Play();

            }
            else
            {
                // Mengoutput kata yang terambil beserta indeksnya ke console
                Debug.Log("Clicked Text : " + wInfo.GetWord() + ", at index : " + wordIndex);

                //Kurangi waktu
                LM.timeRemaining -= 5f;

                correctionAudio.clip = correctionSounds[1];
                correctionAudio.Play();
            }
        }
    }

    private void ErrorCorrection(string oldString, int wordIndex)
    {
        Debug.Log("Corrected Error!");
        articleText.text = articleText.text.Replace(oldString, EG.correctTextIndexs[EG.textErrorIndexes.IndexOf(wordIndex)]);
        articleText.ForceMeshUpdate();

        UpdateWordColor();
        articleText.ForceMeshUpdate();
        // articleText.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
    }

    private void UpdateWordColor()
    {
        // Debug.Log("Changed Color!");

        foreach (var wordIndex in wordTouched)
        {
            TMP_WordInfo info = articleText.textInfo.wordInfo[wordIndex];
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

                // articleText.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
            }
        }
        articleText.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
        // Thread.Sleep(100);
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
            .setEaseInOutSine();
    }

}
