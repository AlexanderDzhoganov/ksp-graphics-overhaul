using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace ksp_graphics_overhaul
{
    class CameraHook : MonoBehaviour
    {

        public RenderTexture targetTexture = null;
        private Material diffuseMaterial = null;

        public void Awake()
        {
            diffuseMaterial = new Material(Shaders.DiffuseDefault);

            targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
            this.gameObject.camera.targetTexture = targetTexture;
            this.gameObject.camera.clearFlags = CameraClearFlags.Color | CameraClearFlags.Depth;

            if(this.gameObject.GetComponent("BloomAndLensFlares") != null)
            {
               Destroy(this.gameObject.GetComponent("BloomAndLensFlares"));
            }

           this.gameObject.camera.depthTextureMode = DepthTextureMode.Depth;
        }

        public void Update()
        {
        }

        public void OnDestroy()
        {
        }

        private HashSet<string> replacedShaders = new HashSet<string>();
        private HashSet<string> vanillaShaders = new HashSet<string>();

        private Vector2 shadersScrollPos;

        void DoDofWindow(int index)
        {
            shadersScrollPos = GUILayout.BeginScrollView(shadersScrollPos);

            GUILayout.Label("vanilla");
            foreach(string sh in vanillaShaders)
            {
                if(GUILayout.Button(sh))
                {
                    replacedShaders.Add(sh);
                    vanillaShaders.Remove(sh);
                    return;
                }
            }

            GUILayout.Label("replaced");
            foreach(string sh in replacedShaders)
            {
                if(GUILayout.Button(sh))
                {
                    vanillaShaders.Add(sh);
                    replacedShaders.Remove(sh);
                    return;
                }
            }

            GUILayout.EndScrollView();
        }

        void OnGUI()
        {
            print("gui");
            GUI.Window(1, new Rect(Screen.width - 512, 32, 512, 300), DoDofWindow, "shader replacement");
        }

        void OnPreRender()
        {
            foreach (Renderer renderer in GameObject.FindObjectsOfType<Renderer>())
            {
                var name = renderer.material.shader.name;

                if(replacedShaders.Contains(name))
                {
                    renderer.material.shader = diffuseMaterial.shader;
                }
                else if(!vanillaShaders.Contains(name))
                {
                    vanillaShaders.Add(name);
                }
            }
        }

        void OnPostRender()
        {
        }

    }
}
