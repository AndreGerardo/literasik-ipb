using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
using TMPro;
using SimpleJSON;

public enum ErrorType
{
    TYPO, TITIK_KOMA, KAPITAL
}

public class LevelErrorGenerator : MonoBehaviour
{
    [Header("TextMeshPro Reference")]
    public TMP_Text articleText;
    public TMP_Text articleTitle;
    public List<string> articleTextArray, articleErrorTextArray;
    private Dictionary<string, string> articleDictionary = new Dictionary<string, string>();
    private List<int> articleJSONIndexList = new List<int>();

    [Header("Error Generation")]
    public int articlePickIndex = 0;
    public List<int> textErrorIndexes = new List<int>();
    public List<ErrorType> errorTypesIndexes = new List<ErrorType>();
    public List<string> correctTextIndexs = new List<string>();
    public List<string> errorTextList = new List<string>();
    public int entireErrorCount;

    [Header("Typo Error Configuration")]
    [SerializeField][TextArea] private string typoIgnoreWordInput;
    private List<string> typoIgnoreWordList = new List<string>();
    [SerializeField] private float typoErrorRate = 0.1f;
    [SerializeField] private int typoErrorCount;
    private int currentTypoErrorCount = 0;

    [Header("Capital Error Config")]
    [SerializeField] private List<string> capitalIgnoreWordList = new List<string>();
    [SerializeField] private float capitalErrorRate = 0.1f;
    [SerializeField] private int capitalErrorCount;
    private int currentCapitalErrorCount = 0;

    [Header("Titik Koma Error Config")]
    [SerializeField] private List<int> tikomaCharErrorLocation = new List<int>();
    [SerializeField] private float tikomaErrorRate = 0.1f;
    [SerializeField] private int tikomaErrorCount;
    public List<int> tikomaErrorIndex = new List<int>();
    private int currentTikomaErrorCount = 0;
    /*[HideInInspector]*/
    public List<int> eraseCharList = new List<int>();
    [HideInInspector] public List<int> undoEraseCharList = new List<int>();


    void OnDisable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(ON_TEXT_CHANGED);
    }


    void ON_TEXT_CHANGED(Object obj)
    {
        EraseChar(); UndoEraseChar();
    }

    public void InitializeErrorGeneration(int gameDifficulty)
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ON_TEXT_CHANGED);
        //Choose Difficulty
        DifficultySelection(gameDifficulty);

        //Pick article from JSON
        LoadDictionary("ArticleDictionaryJSON", articleDictionary);
        for (int i = 0; i < articleDictionary.Count; i++) articleJSONIndexList.Add(i);

        //Pick random article
        articlePickIndex = Random.Range(0, articleJSONIndexList.Count);
        PickArticle(articlePickIndex);
        articleText.ForceMeshUpdate();

        //Insert article texts to list
        string[] articleTexttemp = articleText.text.Split(' ');
        articleTextArray = new List<string>(articleTexttemp);
        articleTextArray.Insert(0, "-DEBUG-");

        //Insert Ignore Word List
        articleTexttemp = typoIgnoreWordInput.Split(' ');
        typoIgnoreWordList = new List<string>(articleTexttemp);

        //Error Generation
        TypoGeneration();
        CapitalErrorGeneration();
        TikomaErrorGenerator();

        entireErrorCount = textErrorIndexes.Count;

        //Insert String Text Error
        for (int i = 0; i < entireErrorCount; i++)
        {
            int tmp = textErrorIndexes[i];
            errorTextList.Add(articleText.textInfo.wordInfo[tmp].GetWord());
        }

        //Insert error article texts to list
        articleTexttemp = articleText.text.Split(' ');
        articleErrorTextArray = new List<string>(articleTexttemp);
        articleErrorTextArray.Insert(0, "-DEBUG-");
        for (int i = 0; i < articleErrorTextArray.Count; i++)
        {
            if (articleErrorTextArray[i] == string.Empty) articleErrorTextArray.RemoveAt(i);
        }
    }

    private void DifficultySelection(int diffIndex)
    {
        switch (diffIndex)
        {
            case 0: typoErrorCount = 2; capitalErrorCount = 2; tikomaErrorCount = 1; break; // DEFAULT 2 2 1
            case 1: typoErrorCount = 4; capitalErrorCount = 4; tikomaErrorCount = 2; break; // DEFAULT 4 4 2
            case 2: typoErrorCount = 7; capitalErrorCount = 4; tikomaErrorCount = 3; break; // DEFAULT 7 4 3
        }
    }


    private void PickArticle(int index)
    {
        articleTitle.text = articleDictionary.ElementAt(articleJSONIndexList[index]).Key;
        articleText.text = articleDictionary.ElementAt(articleJSONIndexList[index]).Value;
        articleText.ForceMeshUpdate();
    }
    private void LoadDictionary(string dictFileName, Dictionary<string, string> outputDict)
    {
        TextAsset ta = Resources.Load(dictFileName) as TextAsset; //Resources.Load(dictFileName) as TextAsset;
        JSONObject jsonObj = (JSONObject)JSON.Parse(ta.text);
        foreach (var key in jsonObj.GetKeys()) { outputDict[key] = jsonObj[key]; }
    }


    private void TypoGeneration()
    {
        articleText.ForceMeshUpdate();
        StringBuilder sb = new StringBuilder(articleText.text);

        for (int i = 0; i < articleText.textInfo.wordCount; i++)
        {
            // Debug.Log("Run");
            float rdm = Random.Range(0f, 1f);
            TMP_WordInfo currentwInfo = articleText.textInfo.wordInfo[i];
            if (currentwInfo.characterCount > 4 && rdm > 1f - typoErrorRate && !typoIgnoreWordList.Contains(currentwInfo.GetWord()) && !textErrorIndexes.Contains(i))
            {
                //Debug.Log("Error Generated at : " + currentwInfo.GetWord());
                correctTextIndexs.Add(currentwInfo.GetWord());
                textErrorIndexes.Add(i);
                errorTypesIndexes.Add(ErrorType.TYPO);

                //Error Algorithm
                int charSelect = Random.Range(currentwInfo.firstCharacterIndex + 1, currentwInfo.lastCharacterIndex);
                char temp = articleText.text[charSelect];
                sb[charSelect] = articleText.text[charSelect + 1];
                sb[charSelect + 1] = temp;
                currentTypoErrorCount++;
            }

            if (i == articleText.textInfo.wordCount - 1 && currentTypoErrorCount < typoErrorCount) i = 0;
            if (currentTypoErrorCount == typoErrorCount) break;

        }

        articleText.text = sb.ToString();

        articleText.ForceMeshUpdate();
    }


    private void CapitalErrorGeneration()
    {
        articleText.ForceMeshUpdate();
        StringBuilder sb = new StringBuilder(articleText.text);

        for (int i = 0; i < articleText.textInfo.wordCount; i++)
        {
            float rdm = Random.Range(0f, 1f);
            TMP_WordInfo currentwInfo = articleText.textInfo.wordInfo[i];
            if (!textErrorIndexes.Contains(i) && !capitalIgnoreWordList.Contains(currentwInfo.GetWord()))
            {
                //Error Algorithm
                char checkCapital = articleText.text[currentwInfo.firstCharacterIndex];
                if (char.IsUpper(checkCapital) && rdm > 1f - capitalErrorRate)
                {
                    correctTextIndexs.Add(currentwInfo.GetWord());
                    textErrorIndexes.Add(i);
                    errorTypesIndexes.Add(ErrorType.KAPITAL);

                    sb[currentwInfo.firstCharacterIndex] = char.ToLower(checkCapital);
                    currentCapitalErrorCount++;
                }
            }

            if (i == articleText.textInfo.wordCount - 1 && currentCapitalErrorCount < capitalErrorCount) i = 0;
            if (currentCapitalErrorCount == capitalErrorCount) break;

        }
        articleText.text = sb.ToString();

        articleText.ForceMeshUpdate();

    }


    private void TikomaErrorGenerator()
    {

        articleText.ForceMeshUpdate();
        // StringBuilder sb = new StringBuilder(articleText.text);

        for (int i = 0; i < articleText.textInfo.wordCount; i++)
        {
            float rdm = Random.Range(0f, 1f);

            TMP_WordInfo currentwInfo = articleText.textInfo.wordInfo[i];

            if (!tikomaErrorIndex.Contains(i) && !tikomaCharErrorLocation.Contains(i))
            {
                char tikomaChecker = articleText.text[currentwInfo.lastCharacterIndex + 1];
                char tokenChecker = ' ';
                if (i != articleText.textInfo.wordCount - 1) tokenChecker = articleText.text[currentwInfo.lastCharacterIndex + 2];

                if ((tikomaChecker.CompareTo('.') == 0 || tikomaChecker.CompareTo(',') == 0) && rdm > 1f - tikomaErrorRate)
                {

                    correctTextIndexs.Add(currentwInfo.GetWord() + tikomaChecker);
                    textErrorIndexes.Add(i);
                    errorTypesIndexes.Add(ErrorType.TITIK_KOMA);
                    if (tokenChecker.CompareTo(' ') == 0)
                    {
                        tikomaErrorIndex.Add(i);
                        tikomaErrorIndex.Add(i + 1);
                    }
                    else
                    {
                        tikomaErrorIndex.Add(i);
                    }

                    eraseCharList.Add(currentwInfo.lastCharacterIndex + 1);
                    tikomaCharErrorLocation.Add(currentwInfo.lastCharacterIndex + 1);
                    // Debug.Log("TIKOMA CHECK: \"" + tikomaChecker + "\" at index: " + currentwInfo.lastCharacterIndex + 1);

                    EraseChar();

                    currentTikomaErrorCount++;
                    articleText.ForceMeshUpdate();

                }

            }

            if (i == articleText.textInfo.wordCount - 1 && currentTikomaErrorCount < tikomaErrorCount) i = 0;
            if (currentTikomaErrorCount == tikomaErrorCount) break;

        }
        // articleText.text = sb.ToString();

        articleText.ForceMeshUpdate();

    }



    private void EraseChar()
    {
        // Debug.Log("Changed Color!");
        if (eraseCharList.Count > 0)
        {
            foreach (var charIndex in eraseCharList)
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

    private void UndoEraseChar()
    {
        // Debug.Log("Changed Color!");
        if (undoEraseCharList.Count > 0)
        {
            foreach (var charIndex in undoEraseCharList)
            {
                int meshIndex = articleText.textInfo.characterInfo[charIndex].materialReferenceIndex;
                int vertexIndex = articleText.textInfo.characterInfo[charIndex].vertexIndex;

                Color32[] vertexColors = articleText.textInfo.meshInfo[meshIndex].colors32;
                Color32 myColor32 = new Color32(60, 200, 82, 255);
                vertexColors[vertexIndex + 0] = myColor32;
                vertexColors[vertexIndex + 1] = myColor32;
                vertexColors[vertexIndex + 2] = myColor32;
                vertexColors[vertexIndex + 3] = myColor32;
            }
            articleText.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
        }
    }



}