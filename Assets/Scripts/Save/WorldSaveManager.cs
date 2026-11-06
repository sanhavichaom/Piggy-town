using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldSaveManager : MonoBehaviour
{
    // Constants

    const string SAVE_FILE_NAME = "world.json";

    // Variables

    [Header("References")]
    [SerializeField] Tilemap _floorTilemap;
    [SerializeField] GridOccupancy _gridOccupancy;
    [SerializeField] ZoneManager _zoneManager;
    [SerializeField] PlaceableObject _placeablePrefab;

    [Header("Object Database")]
    [SerializeField] PlaceableObjectData[] _objectDatabase;

    string _savePath;
    bool _isReady;

    // Methods

    void Awake()
    {
        _savePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
    }

    IEnumerator Start()
    {
        if (_floorTilemap == null ||
            _gridOccupancy == null ||
            _zoneManager == null ||
            _placeablePrefab == null ||
            _objectDatabase == null ||
            _objectDatabase.Length == 0)
        {
            Debug.LogError("World Save Manager references are missing.", this);
            yield break;
        }

        yield return null;

        LoadWorld();
    }

    public void SaveWorld()
    {
        if (!_isReady)
            return;

        WorldSaveData saveData = new WorldSaveData();

        foreach (PlaceableObject placedObject in _gridOccupancy.PlacedObjects)
        {
            if (placedObject == null || placedObject.Data == null)
                continue;

            WorldSaveData.SavedObject savedObject = new WorldSaveData.SavedObject();

            savedObject.id = placedObject.Data.Id;
            savedObject.x = placedObject.OriginCell.x;
            savedObject.y = placedObject.OriginCell.y;
            savedObject.isFlipped = placedObject.IsFlipped;

            saveData.objects.Add(savedObject);
        }

        try
        {
            string json = JsonUtility.ToJson(saveData, true);

            string temporaryPath = _savePath + ".tmp";
            string backupPath = _savePath + ".bak";

            File.WriteAllText(temporaryPath, json);

            if (File.Exists(_savePath))
            {
                File.Copy(_savePath, backupPath, true);
            }

            File.Copy(temporaryPath, _savePath, true);
            File.Delete(temporaryPath);

            Debug.Log("World saved: " + saveData.objects.Count + " objects.");
        }
        catch (Exception exception)
        {
            Debug.LogError("Failed to save world: " + exception.Message);
        }
    }

    void LoadWorld()
    {
        string backupPath = _savePath + ".bak";

        if (!File.Exists(_savePath) && !File.Exists(backupPath))
        {
            _isReady = true;
            return;
        }

        WorldSaveData saveData = ReadSaveFile(_savePath);

        if (saveData == null)
            saveData = ReadSaveFile(backupPath);

        if (saveData == null)
        {
            Debug.LogError("World save could not be loaded.");
            return;
        }

        int loadedCount = 0;

        for (int i = 0; i < saveData.objects.Count; i++)
        {
            if (RestoreObject(saveData.objects[i]))
                loadedCount++;
        }

        _isReady = true;

        Debug.Log("World loaded: " + loadedCount + " objects.");
    }

    WorldSaveData ReadSaveFile(string path)
    {
        if (!File.Exists(path))
            return null;

        try
        {
            string json = File.ReadAllText(path);

            WorldSaveData data = JsonUtility.FromJson<WorldSaveData>(json);

            if (data == null || data.version != 1 || data.objects == null)
                return null;

            return data;
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Cannot read save file: " + exception.Message);
            return null;
        }
    }

    bool RestoreObject(WorldSaveData.SavedObject savedObject)
    {
        if (savedObject == null)
            return false;

        PlaceableObjectData data = FindObjectData(savedObject.id);

        if (data == null)
        {
            Debug.LogWarning("Unknown object ID: " + savedObject.id);
            return false;
        }

        Vector2Int originCell = new Vector2Int(savedObject.x, savedObject.y);

        Vector2Int footprint = data.GetFootprint(savedObject.isFlipped);

        if (!_zoneManager.CanPlaceInZone(data.Category, originCell, footprint))
        {
            Debug.LogWarning("Invalid saved zone: " + savedObject.id);
            return false;
        }

        if (!_gridOccupancy.IsAreaFree(originCell, footprint))
        {
            Debug.LogWarning("Saved object overlaps: " + savedObject.id);
            return false;
        }

        Vector3 firstCell = _floorTilemap.GetCellCenterWorld(new Vector3Int(originCell.x, originCell.y, 0));

        Vector3 lastCell = _floorTilemap.GetCellCenterWorld(new Vector3Int(originCell.x + footprint.x - 1, originCell.y + footprint.y - 1, 0));

        Vector3 worldPosition = (firstCell + lastCell) * 0.5f;

        PlaceableObject placedObject = Instantiate(_placeablePrefab, worldPosition, Quaternion.identity);

        placedObject.Initialize(data, originCell);
        placedObject.SetFlipped(savedObject.isFlipped);

        if (!_gridOccupancy.TryOccupy(placedObject, originCell, footprint))
        {
            Destroy(placedObject.gameObject);
            return false;
        }

        return true;
    }

    PlaceableObjectData FindObjectData(string id)
    {
        for (int i = 0; i < _objectDatabase.Length; i++)
        {
            PlaceableObjectData data = _objectDatabase[i];

            if (data != null && data.Id == id)
                return data;
        }

        return null;
    }
    void OnApplicationPause(bool isPaused)
    {
        if (isPaused)
        {
            SaveWorld();
        }
    }

    void OnApplicationQuit()
    {
        SaveWorld();
    }

#if UNITY_EDITOR
    [ContextMenu("Save World (Test)")]
    void TestSaveWorld()
    {
        if (!Application.isPlaying)
            return;

        SaveWorld();
    }
#endif
}