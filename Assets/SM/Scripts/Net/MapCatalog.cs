using UnityEngine;

namespace SM.Net
{
    /// <summary>
    /// Inspector-driven catalog of maps. No autoscan — author explicitly.
    /// </summary>
    [CreateAssetMenu(fileName = "MapCatalog", menuName = "SM/Map Catalog")]
    public class MapCatalog : ScriptableObject
    {
        [System.Serializable]
        public class MapEntry
        {
            [Tooltip("Human-friendly name for UI.")]
            public string DisplayName = "Surf Intro";
            [Tooltip("Exact scene name as in Build Settings.")]
            public string SceneName = "Surf_Intro";
            [Tooltip("Default match time in seconds.")]
            public int DefaultMatchSeconds = 180;
            [Tooltip("Name of StartZone GameObject in the scene.")]
            public string StartZoneName = "StartZone";
        }

        [Tooltip("Playable maps list for dropdown.")]
        public MapEntry[] Maps;
    }
}
