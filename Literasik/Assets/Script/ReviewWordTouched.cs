using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Text;
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
    Vector3 lastMousePos;
    [SerializeField] private List<string> errorTextList = new List<string>();

    [Header("Correction Reference")]
    [SerializeField] private CanvasGroup correctionPanel;
    [SerializeField] private TMP_Text errorTypeText, correctionText;
    private AudioSource correctionAudio;
    private Camera cam;

    public void OnEnable()
    {
        cam = Camera.main;

        articleText.text = errorGeneratedText.text;
        articleText.ForceMeshUpdate();
        foreach (var errorIndex in EG.textErrorIndexes)
        {
            articleText.ForceMeshUpdate();
            errorTextList.Add(articleText.textInfo.wordInfo[errorIndex].GetWord());
            if (EG.errorTypesIndexes[EG.textErrorIndexes.IndexOf(errorIndex)] == ErrorType.TITIK_KOMA)
            {
                ErrorCorrection(EG.articleErrorTextArray[errorIndex], errorIndex);
            }
            else if (EG.errorTypesIndexes[EG.textErrorIndexes.IndexOf(errorIndex)] == ErrorType.KAPITAL)
            {
                articleText.ForceMeshUpdate();
                StringBuilder sb = new StringBuilder(articleText.text);
                char checkCapital = articleText.text[articleText.textInfo.wordInfo[errorIndex].firstCharacterIndex];
                sb[articleText.textInfo.wordInfo[errorIndex].firstCharacterIndex] = char.ToUpper(checkCapital);
                articleText.text = sb.ToString();
                articleText.ForceMeshUpdate();
            }
            else
            {
                ErrorCorrection(articleText.textInfo.wordInfo[errorIndex].GetWord(), errorIndex);
            }
            UpdateWordColor();
        }
        // Thread.Sleep(100);
        // UpdateWordColor();
        // articleText.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
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
        if (wordIndex != -1 && wordIndex != selectedWord)
        {
            selectedWord = wordIndex;

            TMP_WordInfo wInfo = articleText.textInfo.wordInfo[wordIndex];

            if (EG.textErrorIndexes.Contains(wordIndex))
            {

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

    private void ErrorCorrection(string oldString, int wordIndex)
    {
        Debug.Log("Corrected Error!");
        articleText.text = articleText.text.Replace(oldString, EG.correctTextIndexs[EG.textErrorIndexes.IndexOf(wordIndex)]);

        articleText.ForceMeshUpdate();
        UpdateWordColor();
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

    public void UpdateWordColor()
    {
        // Debug.Log("Changed Color!");

        foreach (var wordIndex in EG.textErrorIndexes)
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
            }
        }
        // Thread.Sleep(100);
        articleText.UpdateVertexData();

    }
}
