using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("Game Reference")]
    [SerializeField] private LevelManager LM;
    [SerializeField] private ReviewWordTouched RM;
    [SerializeField] private GameObject articleScrollView;
    private string[] gameModes = { "Mudah", "Sedang", "Sulit" };
    private int currentGameModeIndex = 0;


    [Header("UI Reference")]
    [SerializeField] private RectTransform Logo;
    [SerializeField] private RectTransform MainMenuGroup, TopBarGroup;
    [SerializeField] private CanvasGroup MainMenuCG, articleCG;
    [SerializeField] private TMP_Text modeText, articleText;

    [Header("Tweening Reference")]
    private int animID;


    private void Start()
    {
        animID = LeanTween.moveY(Logo, -150f, 1.5f)
                    .setLoopPingPong()
                    .setEaseInOutSine()
                    .id;
    }

    public void PlaySequence()
    {
        //MoveMenuOut
        LeanTween.cancel(animID);
        LeanTween.moveY(MainMenuGroup, -1200f, 1f)
            .setEaseInOutSine();
        LeanTween.alphaCanvas(MainMenuCG, 0f, 1f)
            .setEaseInOutSine();
        LeanTween.moveY(Logo, 600f, 1f)
            .setEaseInOutSine()
            .setOnComplete(() =>
            {
                articleScrollView.SetActive(true);
                LM.PlayGame(currentGameModeIndex);
                // RM.InitializeReview();
            });

        //MoveGameIn
        LeanTween.moveY(TopBarGroup, 0f, 1f)
            .setDelay(1f)
            .setEaseInOutSine()
            .setOnComplete(() => LM.isPlaying = true);
        LeanTween.alphaCanvas(articleCG, 1f, 1f)
            .setDelay(1f)
            .setEaseInOutSine();
    }

    public void PracticeSequence()
    {
        //MoveMenuOut
        LeanTween.cancel(animID);
        LeanTween.moveY(MainMenuGroup, -1200f, 1f)
            .setEaseInOutSine();
        LeanTween.alphaCanvas(MainMenuCG, 0f, 1f)
            .setEaseInOutSine();
    }

    public void OptionSequence()
    {

    }

    public void ChangeGameMode(bool isRight)
    {
        if (isRight)
        {
            currentGameModeIndex++;
            if (currentGameModeIndex == gameModes.Length) currentGameModeIndex = 0;
        }
        else
        {
            currentGameModeIndex--;
            if (currentGameModeIndex == -1) currentGameModeIndex = gameModes.Length - 1;
        }

        modeText.text = gameModes[currentGameModeIndex];
    }
}
