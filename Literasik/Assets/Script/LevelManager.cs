using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;


public class LevelManager : MonoBehaviour
{
    [Header("Game Reference")]
    private LevelErrorGenerator EG;
    [SerializeField] private GameObject articleScrollView;
    public bool isPlaying = false;
    public float _timeRemaining = 120f;
    public float timeRemaining
    {
        get { return _timeRemaining; }
        set { _timeRemaining = value; DisplayTime(_timeRemaining); }
    }
    private int errorCount, _errorFound;
    public int errorFound
    {
        get { return _errorFound; }
        set { _errorFound = value; errorText.text = _errorFound + " / " + errorCount; }
    }

    [Header("TopBar Reference")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text errorText;

    [Header("EndCard Reference")]
    [SerializeField] private TMP_Text endErrorText;
    [SerializeField] private TMP_Text endTitleText;
    [SerializeField] private GameObject endCardGroup;


    private void Start()
    {
        EG = GetComponent<LevelErrorGenerator>();

        //--DEBUG ONLY-- PlayGame();
    }


    private void Update()
    {
        if (isPlaying)
        {
            if (timeRemaining > 0 && errorCount != errorFound)
            {
                timeRemaining -= Time.deltaTime;
            }
            else
            {
                EndGame();
            }

        }
    }

    public void PlayGame()
    {
        EG.InitializeErrorGeneration();

        errorCount = EG.entireErrorCount;
        errorFound = 0;

        timeRemaining = 120f;

        //isPlaying = true;
    }

    public void RestartGame()
    {
        EG.InitializeErrorGeneration();

        errorCount = EG.entireErrorCount;
        errorFound = 0;

        timeRemaining = 120f;
        isPlaying = true;
        endCardGroup.SetActive(false);
    }

    public void ExitGame()
    {
        endCardGroup.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void EndGame()
    {
        isPlaying = false;
        articleScrollView.SetActive(false);
        endCardGroup.SetActive(true);
        endTitleText.text = EG.articleTitle.text;
        endErrorText.text = _errorFound + "/" + errorCount;
        // endTitleText.text = "\"" + EG
    }

    private void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;

        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

}
