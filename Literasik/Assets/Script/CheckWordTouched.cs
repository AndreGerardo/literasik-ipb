using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.EventSystems;
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

    private Camera cam;

    private void Start()
    {
        articleText = GetComponent<TMP_Text>();
        cam = Camera.main;
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

            if (EG.textErrorIndexes.Contains(wordIndex) && !wordTouched.Contains(wordIndex))
            {
                LM.errorFound++;
                wordTouched.Add(wordIndex);

                if (EG.errorTypesIndexes[EG.textErrorIndexes.IndexOf(wordIndex)] == ErrorType.TITIK_KOMA)
                {
                    ErrorCorrection(EG.articleErrorTextArray[wordIndex], wordIndex);
                }
                else
                {
                    ErrorCorrection(wInfo.GetWord(), wordIndex);
                }

                Debug.Log("Error Word : " + wInfo.GetWord() + ", correct word : " + EG.correctTextIndexs[EG.textErrorIndexes.IndexOf(wordIndex)] + ", error type : " + EG.errorTypesIndexes[EG.textErrorIndexes.IndexOf(wordIndex)]);
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
    }

}
