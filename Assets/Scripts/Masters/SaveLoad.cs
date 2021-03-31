using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.SceneManagement;

public static class SaveLoad
{
    public static void Save()
    {
        BinaryFormatter bf = new BinaryFormatter();
        Debug.Log("Saving in " + Application.persistentDataPath + "/savedGames.gd");
        FileStream file = File.Create(Application.persistentDataPath + "/savedGames.gd");

        PlayerData data = new PlayerData();
        PlayerStats stats = PlayerStats.instance;
        data.completedTutorial = stats.completedTutorial;
        data.konamiBossHealth = Data.Instance.konamiBossHealth;
        Debug.Log(Data.Instance.konamiBossHealth);

        bf.Serialize(file, data);
        file.Close();
    }

    public static void Load()
    {
        if (File.Exists(Application.persistentDataPath + "/savedGames.gd"))
        {
            Debug.Log("Loading from in " + Application.persistentDataPath + "/savedGames.gd");
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/savedGames.gd", FileMode.Open);
            PlayerData data = (PlayerData)bf.Deserialize(file);

            file.Close();

            PlayerStats playerStats = PlayerStats.instance;
            Debug.Log(data.konamiBossHealth);
            Data.Instance.konamiBossHealth = data.konamiBossHealth;
            playerStats.completedTutorial = data.completedTutorial;
        }
    }
}

[System.Serializable]
class PlayerData
{
    public bool completedTutorial;
    public int konamiBossHealth;
}
