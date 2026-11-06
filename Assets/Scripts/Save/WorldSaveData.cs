using System;
using System.Collections.Generic;

[Serializable]
public class WorldSaveData
{
    // Constants

    // Local classes / structs

    public int version = 1;
    public List<SavedObject> objects = new List<SavedObject>();

    [Serializable]
    public class SavedObject
    {
        public string id;
        public int x;
        public int y;
        public bool isFlipped;
    }

    // Enums

    // Variables

    // Methods
}