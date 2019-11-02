namespace IL3DN
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Helping class to associate a color with a material
    /// </summary>
    [System.Serializable]
    public class ShaderProperties
    {
        public Color color;
        public Texture2D mainTex;
        public ShaderProperties(Color color, Texture2D mainTex)
        {
            this.color = color;
            this.mainTex = mainTex;
        }
    }

    /// <summary>
    /// Helping class to display multiple set of properties on the same material
    /// </summary>
    [System.Serializable]
    public class MaterialProperties
    {
        public Material meterial;
        public List<ShaderProperties> properties;
        public int selectedProperty;

        public MaterialProperties(Material material)
        {
            this.meterial = material;
            properties = new List<ShaderProperties>();
        }
    }

    /// <summary>
    /// Display a list of materials with a single color to customize
    /// </summary>
    public class IL3DN_ColorManagerTextures : MonoBehaviour
    {
        public List<MaterialProperties> materials = new List<MaterialProperties>();
    }
}
