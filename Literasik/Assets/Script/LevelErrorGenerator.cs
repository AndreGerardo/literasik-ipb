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
    [SerializeField] private TMP_Text articleText;
    [SerializeField] private TMP_Text articleTitle;
    public List<string> articleTextArray, articleErrorTextArray;
    private Dictionary<string, string> articleDictionary = new Dictionary<string, string>();
    private List<int> articleJSONIndexList = new List<int>();

    [Header("Error Generation")]
    public int articlePickIndex = 0;
    public List<int> textErrorIndexes = new List<int>();
    public List<ErrorType> errorTypesIndexes = new List<ErrorType>();
    public List<string> correctTextIndexs = new List<string>();
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
    [SerializeField] private List<string> tikomaIgnoreWordList = new List<string>();
    [SerializeField] private float tikomaErrorRate = 0.1f;
    [SerializeField] private int tikomaErrorCount;
    private int currentTikomaErrorCount = 0;

    public void InitializeErrorGeneration()
    {
        //Pick article from JSON
        LoadDictionary("ArticleDictionaryJSON", articleDictionary);
        for (int i = 0; i < articleDictionary.Count; i++) articleJSONIndexList.Add(i);

        PickArticle(articlePickIndex);

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
        //TikomaErrorGenerator();

        entireErrorCount = typoErrorCount + capitalErrorCount + tikomaErrorCount;

        //Insert error article texts to list
        articleTexttemp = articleText.text.Split(' ');
        articleErrorTextArray = new List<string>(articleTexttemp);
        articleErrorTextArray.Insert(0, "-DEBUG-");
        for (int i = 0; i < articleErrorTextArray.Count; i++)
        {
            if (articleErrorTextArray[i] == string.Empty) articleErrorTextArray.RemoveAt(i);
        }
    }


    private void PickArticle(int index)
    {
        int rdm = Random.Range(0, articleJSONIndexList.Count);

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
        int counterTemp = articleText.textInfo.wordCount;

        for (int i = 1; i < counterTemp; i++)
        {
            float rdm = Random.Range(0f, 1f);
            string currentWord = articleTextArray[i];
            TMP_WordInfo currentwInfo = articleText.textInfo.wordInfo[i];
            if (!textErrorIndexes.Contains(i) && (currentWord.Contains(".") || currentWord.Contains(",")) && !tikomaIgnoreWordList.Contains(currentWord) && rdm > 1f - tikomaErrorRate)
            {
                //Error Algorithm
                correctTextIndexs.Add(articleTextArray[i]);
                textErrorIndexes.Add(i);
                errorTypesIndexes.Add(ErrorType.TITIK_KOMA);

                int tokenPosition = currentWord.Contains(".") ? currentWord.IndexOf('.') : currentWord.IndexOf(',');
                // sb[tokenPosition + currentwInfo.firstCharacterIndex] = '&';
                articleText.text = articleText.text.Remove(tokenPosition + currentwInfo.firstCharacterIndex, 1);
                currentTikomaErrorCount++;
                articleText.ForceMeshUpdate();

                if (articleText.textInfo.wordCount != counterTemp)
                {
                    counterTemp = articleText.textInfo.wordCount;
                    i = 0;
                }
            }

            if (i == counterTemp - 1 && currentTikomaErrorCount < tikomaErrorCount) i = 0;
            if (currentTikomaErrorCount == tikomaErrorCount) break;

        }
        // articleText.text = sb.ToString();

        articleText.ForceMeshUpdate();
    }


}
