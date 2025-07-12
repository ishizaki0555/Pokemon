using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioController : MonoBehaviour
{
    [SerializeField] private JsonLoader loader;
    [SerializeField] private AudioMixer myMixer;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider SFXSlider;
    [SerializeField] GameObject parentObj;

    private void Start()
    {
        LoadValue();
    }
    public void SetMusicVolume()
    {
        float volume = bgmSlider.value;
        myMixer.SetFloat("BGM", Mathf.Log10(volume) * 20);
        loader.data.BGM = volume;
        loader.SaveData();
    }

    public void SetSFXVolume()
    {
        float volume = SFXSlider.value;
        myMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
        loader.data.SE = volume;
        loader.SaveData();
    }

    private void LoadValue()
    {
        bgmSlider.value = loader.data.BGM;
        SFXSlider.value = loader.data.SE;

        SetMusicVolume();
        SetSFXVolume();
    }
}
