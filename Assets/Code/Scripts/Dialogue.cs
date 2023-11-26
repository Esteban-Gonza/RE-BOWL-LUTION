using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Dialogue : MonoBehaviour{

    [SerializeField] private GameObject textMark;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject nameText;
    [SerializeField] private Image charImage;
    [SerializeField] private Sprite charSprite;
    [SerializeField] private Sprite emptySprite;
    [SerializeField, TextArea(4, 6)] private string[] dialogueLines;

    private int lineIndex;
    private float typingTime = 0.05f;
    private bool isPlayerInRange;
    [SerializeField] private bool isBeginingText;
    private bool didDialogueStart;

    private void Start(){
        charImage.sprite = emptySprite;

        if(isBeginingText == true){
            if (!didDialogueStart){
                StartDialogue();
            }else if (dialogueText.text == dialogueLines[lineIndex]){
                NextDialogueLine();
            }else{
                StopAllCoroutines();
                dialogueText.text = dialogueLines[lineIndex];
            }
        }
    }

    private void Update(){

        if(isBeginingText == false){
            if (isPlayerInRange && Input.GetKeyDown(KeyCode.E)){
                if (!didDialogueStart){
                    StartDialogue();
                }else if (dialogueText.text == dialogueLines[lineIndex]){
                    NextDialogueLine();
                }else{
                    StopAllCoroutines();
                    dialogueText.text = dialogueLines[lineIndex];
                }
            }
        }
    }

    public void StartDialogue(){
        didDialogueStart = true;
        dialoguePanel.SetActive(true);
        nameText.SetActive(true);
        textMark.SetActive(false);
        charImage.sprite = charSprite;
        lineIndex = 0;
        Time.timeScale = 0;

        StartCoroutine(ShowLine());
    }

    public void NextDialogueLine(){
        lineIndex++;
        if(lineIndex < dialogueLines.Length){
            StartCoroutine(ShowLine());
        }else{
            didDialogueStart = false;
            dialoguePanel.SetActive(false);
            textMark.SetActive(true);
            charImage.sprite = emptySprite;
            Time.timeScale = 1;
            nameText.SetActive(false);

            if(isBeginingText == false)
                GameManager.instance.GoToScene("Beta");
        }
    }

    IEnumerator ShowLine(){
        dialogueText.text = string.Empty;

        foreach(char ch in dialogueLines[lineIndex]){
            dialogueText.text += ch;
            yield return new WaitForSecondsRealtime(typingTime);
        }
    }

    private void OnTriggerEnter(Collider other){
        if (other.gameObject.CompareTag("Player")){
            isPlayerInRange = true;
            textMark.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other){
        if (other.gameObject.CompareTag("Player")){
            isPlayerInRange = true;
            textMark.SetActive(true);
        }
    }
}