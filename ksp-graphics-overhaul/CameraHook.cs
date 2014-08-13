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

        public void Awake()
        {
            targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
            this.gameObject.camera.targetTexture = targetTexture;
            this.gameObject.camera.clearFlags = CameraClearFlags.Nothing;

            if(this.gameObject.GetComponent("BloomAndLensFlares") != null)
            {
               Destroy(this.gameObject.GetComponent("BloomAndLensFlares"));
            }

            this.gameObject.camera.depthTextureMode = DepthTextureMode.Depth;
//           this.gameObject.camera.renderingPath = RenderingPath.DeferredLighting;
        }

        public void Update()
        {
        }

        public void OnDestroy()
        {
        }

        void OnPreRender()
        {
            foreach (Renderer renderer in GameObject.FindObjectsOfType<Renderer>())
            {
                var shaderName = renderer.material.shader.name;
                if(Shaders.Replacements.ContainsKey(shaderName))
                {
                    renderer.material.shader = Shaders.GetShader(Shaders.Replacements[shaderName]);
                }
            }

            RenderTexture.active = targetTexture;
            GL.Clear(true, true, new Color(1, 0, 1, 0));
        }

        void OnPostRender()
        {
        }

    }
}
