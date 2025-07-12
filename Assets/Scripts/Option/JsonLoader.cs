using System.IO;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Data
{
    public float BGM;
    public float SE;
}

public class JsonLoader : MonoBehaviour
{
    string folderPath;
    string filePath;
    public Data data;
    [SerializeField] AudioMixer myMixer;

    private void Start()
    {
        folderPath = Path.Combine(Application.persistentDataPath, "VolumeData");
        filePath = Path.Combine(folderPath, "VolumeData.json");

        data = LoadData();
        ValueData();
    }

    public Data LoadData()
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        if (!File.Exists(filePath))
        {
            // 初期データの作成
            Data defaultData = new Data
            {
                BGM = 0.5f,
                SE = 0.5f
            };

            string defaultJson = JsonUtility.ToJson(defaultData, true);
            File.WriteAllText(filePath, defaultJson);
        }

        // JSONファイルを読み込んでデータに反映
        string json = File.ReadAllText(filePath);
        return JsonUtility.FromJson<Data>(json);
    }

    public void SaveData()
    {
        //ファイルの上書き
        string jsonstr = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, jsonstr);
    }

    public void ValueData()
    {
        //ボリュームの初期値の設定
        float bgmVolume = Mathf.Log10(Mathf.Clamp(data.BGM, 0.0001f, 1f)) * 20;
        float seVolume = Mathf.Log10(Mathf.Clamp(data.SE, 0.0001f, 1f)) * 20;

        myMixer.SetFloat("BGM", bgmVolume);
        myMixer.SetFloat("SFX", seVolume);
    }
}
