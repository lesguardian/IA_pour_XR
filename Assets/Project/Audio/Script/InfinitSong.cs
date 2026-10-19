using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class InfinitSong : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioMixer Mixer;
    [SerializeField] private AudioClip Loop;
    [SerializeField] private AudioSource sourceA;
    [SerializeField] private AudioSource sourceB;
    
    [Header("Configuration")]
    [SerializeField] private float PitchShiftA = 1f;
    [SerializeField] private float PitchShiftB = 1f;
    [SerializeField] private float OffsetPitchShift = 0.1f;
    [SerializeField] private int LoopCount = 4;
    [SerializeField] private int LoopBOffset = 2;
    private int LoopAIndex = 0, LoopBIndex = 0;
    
    
    void Start()
    {
        StartCoroutine(TimerA());
        StartCoroutine(TimerB(Loop.length * LoopBOffset));
    }
    
    IEnumerator TimerA()
    {
        if (LoopAIndex == LoopCount)
        {
            PitchShiftA = 1;
            Mixer.SetFloat("PitchA", PitchShiftA);
            LoopAIndex = 0;
        }
        
        PitchShiftA += OffsetPitchShift;
        Mixer.SetFloat("PitchA", PitchShiftA);
        sourceA.PlayOneShot(Loop);

        LoopAIndex++;
        
        yield return new WaitForSeconds(Loop.length);
        StartCoroutine(TimerA());
    }

    IEnumerator TimerB(float startTimer)
    {
        if (startTimer != 0)
            yield return new WaitForSeconds(startTimer);

        if (LoopBIndex == LoopCount)
        {
            PitchShiftB = 1;
            Mixer.SetFloat("PitchB", PitchShiftB);
            LoopBIndex = 0;
        }
        
        PitchShiftB += OffsetPitchShift;
        Mixer.SetFloat("PitchB", PitchShiftB);
        sourceB.PlayOneShot(Loop);
        
        LoopBIndex++;
        
        yield return new WaitForSeconds(Loop.length);
        StartCoroutine(TimerB(0f));
    }
}