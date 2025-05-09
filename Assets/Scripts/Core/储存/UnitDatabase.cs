using System.Collections.Generic;
using UnityEngine;

public class UnitDatabase : MonoBehaviour
{
    public static UnitDatabase Instance;

    [System.Serializable]
    public struct UnitEntry
    {
        public string unitName;
        public GameObject unitPrefab;
    }

    public List<UnitEntry> units = new List<UnitEntry>();

    private Dictionary<string, GameObject> unitDictionary = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // ��ʼ���ֵ�
            foreach (UnitEntry entry in units)
            {
                if (!unitDictionary.ContainsKey(entry.unitName))
                {
                    unitDictionary.Add(entry.unitName, entry.unitPrefab);
                }
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GameObject GetUnitPrefabByName(string unitName)
    {
        if (unitDictionary.ContainsKey(unitName))
        {
            return unitDictionary[unitName];
        }

        Debug.LogWarning($"��Ա���� {unitName} �����������ݿ��У�");
        return null;
    }
}
