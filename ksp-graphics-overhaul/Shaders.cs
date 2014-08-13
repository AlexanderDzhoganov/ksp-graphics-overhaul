using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using UnityEngine;

namespace ksp_graphics_overhaul
{
    static class Shaders
    {

        public static void LoadShaders(Dictionary<string, string> paths, Dictionary<string, string> replacements)
        {
            foreach(KeyValuePair<string, string> shader in paths)
            {
                var name = shader.Key;
                Console.WriteLine("ADDING SHADER " + name);
                var path = KSPUtil.ApplicationRootPath + shader.Value;

                var source = System.IO.File.ReadAllText(path);
                m_Materials[name] = new Material(source);
            }

            m_Replacements = replacements;
        }

        public static Shader GetShader(string name)
        {
            return m_Materials[name].shader;
        }

        public static Material GetMaterial(string name)
        {
            return m_Materials[name];
        }

        public static Dictionary<string, string> Replacements
        {
            get
            {
                return m_Replacements;
            }
        }

        public static void ClearAll()
        {
            m_Materials.Clear();
            m_Replacements.Clear();
        }

        private static Dictionary<string, Material> m_Materials = new Dictionary<string, Material>();
        private static Dictionary<string, string> m_Replacements = new Dictionary<string, string>();

    }
}
