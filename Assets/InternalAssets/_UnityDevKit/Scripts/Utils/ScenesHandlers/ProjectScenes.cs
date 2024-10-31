using System;

namespace UnityDevKit.Utils.SceneHandlers
{
    public static class ProjectScenes
    {
        public static SceneInfo SIMULATION => new SceneInfo
        {
            Name = "2021_11_16_URP_Roads", IsMenu = true
        };

        [Serializable]
        public struct SceneInfo
        {
            public string Name;
            public bool IsMenu;
        }

        public static SceneInfo GetSceneByNumber(int sceneNumber)
        {
            return sceneNumber switch
            {
                0 => SIMULATION,
                _ => throw new ArgumentException("Unknown scene number")
            };
        }
    }
}