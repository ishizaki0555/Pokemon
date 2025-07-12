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
            // �����f�[�^�̍쐬
            Data defaultData = new Data
            {
                BGM = 0.5f,
                SE = 0.5f
            };

            string defaultJson = JsonUtility.ToJson(defaultData, true);
            File.WriteAllText(filePath, defaultJson);
        }

        // JSON�t�@�C����ǂݍ���Ńf�[�^�ɔ��f
        string json = File.ReadAllText(filePath);
        return JsonUtility.FromJson<Data>(json);
    }

    public void SaveData()
    {
        //�t�@�C���̏㏑��
        string jsonstr = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, jsonstr);
    }

    public void ValueData()
    {
        //�{�����[���̏����l�̐ݒ�
        float bgmVolume = Mathf.Log10(Mathf.Clamp(data.BGM, 0.0001f, 1f)) * 20;
        float seVolume = Mathf.Log10(Mathf.Clamp(data.SE, 0.0001f, 1f)) * 20;

        myMixer.SetFloat("BGM", bgmVolume);
        myMixer.SetFloat("SFX", seVolume);
    }
}
