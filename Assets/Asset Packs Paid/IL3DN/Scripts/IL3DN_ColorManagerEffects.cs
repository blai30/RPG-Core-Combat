namespace IL3DN
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Helping class to associate a name with a color
    /// </summary>
    [System.Serializable]
    public class ColorProperty
    {
        public Color color;
        public string name;

        public ColorProperty(Color color, string name)
        {
            this.color = color;
            this.name = name;
        }
    }

    /// <summary>
    /// Helping class to associate multiple colors to the same material
    /// </summary>
    [System.Serializable]
    public class MaterialColors
    {
        public List<ColorProperty> colors;

        public MaterialColors(Material material)
        {
            colors = new List<ColorProperty>();
#if UNITY_EDITOR
            Shader shader = material.shader;
            int nrOfProperties = UnityEditor.ShaderUtil.GetPropertyCount(shader);
            for (int i = 0; i < nrOfProperties; i++)
            {
                if (UnityEditor.ShaderUtil.GetPropertyType(shader, i) == UnityEditor.ShaderUtil.ShaderPropertyType.Color)
                {
                    string propertyName = UnityEditor.ShaderUtil.GetPropertyName(shader, i);
                    Color color = material.GetColor(propertyName);
                    Debug.Log(propertyName + " " + color);
                    colors.Add(new ColorProperty(color, propertyName));
                }
            }
#endif
        }
    }

    /// <summary>
    /// Helping class to display multiple colors for a material
    /// </summary>
    [System.Serializable]
    public class MultipleColorProperties
    {
        public Material meterial;
        public List<MaterialColors> properties;
        public int selectedProperty;

        public MultipleColorProperties(Material material)
        {
            this.meterial = material;
            properties = new List<MaterialColors>();
            properties.Add(new MaterialColors(material));
        }
    }

    /// <summary>
    /// Displays a list of materials with multiple colors for customization 
    /// </summary>
    public class IL3DN_ColorManagerEffects : MonoBehaviour
    {
        public List<MultipleColorProperties> materials = new List<MultipleColorProperties>();
    }
}