using System.Collections;
using TMPro;
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea(2, 4)]
    public string text;
    [Tooltip("Temps d'affichage après la fin de l'écriture (0 = calcul auto selon la longueur du texte)")]
    public float displayDuration = 0f;
}

public class DialogueSystem : MonoBehaviour
{
    [Header("Références UI")]
    public GameObject dialogueBox;
    public TMP_Text dialogueText;
    public TMP_Text nameText;

    [Header("Séquence de dialogue")]
    public DialogueLine[] lines;
    public float startDelay = 3f;

    [Header("Réglages")]
    public float charsPerSecond = 30f;
    public bool useTypewriterEffect = true;

    [Header("Avance automatique")]
    public float minDisplayDuration = 1.5f;   // temps mini affiché même pour un texte très court
    public float secondsPerCharacter = 0.06f; // temps de lecture ajouté par caractère si displayDuration = 0

    void Awake()
    {
        if (dialogueBox != null) dialogueBox.SetActive(false);
    }

    void Start()
    {
        if (lines != null && lines.Length > 0)
            StartCoroutine(RunDialogueSequence());
    }

    IEnumerator RunDialogueSequence()
    {
        yield return new WaitForSeconds(startDelay);

        foreach (DialogueLine line in lines)
        {
            yield return StartCoroutine(ShowLineAndWait(line));
        }

        Hide();
    }

    IEnumerator ShowLineAndWait(DialogueLine line)
    {
        dialogueBox.SetActive(true);
        if (nameText != null) nameText.text = line.speakerName;

        if (useTypewriterEffect)
            yield return StartCoroutine(TypeText(line.text));
        else
            dialogueText.text = line.text;

        float waitTime = line.displayDuration > 0f
            ? line.displayDuration
            : Mathf.Max(minDisplayDuration, line.text.Length * secondsPerCharacter);

        yield return new WaitForSeconds(waitTime);
    }

    IEnumerator TypeText(string message)
    {
        dialogueText.text = "";
        float delay = 1f / charsPerSecond;

        foreach (char c in message)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(delay);
        }
    }

    public void Hide()
    {
        if (dialogueBox != null) dialogueBox.SetActive(false);
        StopAllCoroutines();
    }
}