using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace ksp_graphics_overhaul
{

    [KSPAddon(KSPAddon.Startup.Flight, true)]
    public class GraphicsOverhaul : MonoBehaviour
    {

        private XMLConfigReader configReader;

        private List<Camera> cameras = new List<Camera>();
        private Dictionary<string, CameraHook> cameraHooks = new Dictionary<string, CameraHook>();
        private Camera dummyCamera = null;

        public void Awake()
        {
            print("ksp-graphics-overhaul: initialized");

            configReader = new XMLConfigReader();
            configReader.FromFile("GameData/ksp-graphics-overhaul/ksp-graphics-overhaul.xml");

            Shaders.LoadShaders(configReader.ShaderPaths, configReader.ShaderReplacements);

            dummyCamera = this.gameObject.AddComponent<Camera>();
            dummyCamera.depth = 10;

            foreach(string hookName in configReader.CameraHooks)
            {
                foreach (Camera camera in GameObject.FindObjectsOfType<Camera>())
                {
                    if (hookName == camera.gameObject.name)
                    {
                        if (camera.gameObject.GetComponent<CameraHook>() == null)
                        {
                            var hook = camera.gameObject.AddComponent<CameraHook>();
                            cameraHooks.Add(camera.gameObject.name, hook);
                            print("ksp-graphics-overhaul: Hooking " + camera.gameObject.name);
                        }
                    }
                }
            }
        }

        public void OnDestroy()
        {
            Shaders.ClearAll();
            print("ksp-graphics-overhaul: destroyed");
        }

        void OnGUICamera(Camera camera)
        {
            GUILayout.BeginHorizontal();

            if(camera == null)
            {
                GUILayout.Label("null");
            }
            else
            {
                GUILayout.Label(camera.gameObject.name);
                camera.enabled = GUILayout.Toggle(camera.enabled, "");
                GUILayout.Label("near: " + camera.nearClipPlane + " far: " + camera.farClipPlane);
                GUILayout.Label("path: " + camera.actualRenderingPath.ToString() + " depth mode: " + camera.depthTextureMode.ToString());
                GUILayout.Label("order: " + camera.depth);
                GUILayout.Label("target tex: " + camera.targetTexture);
                GUILayout.Label("pos: " + camera.transform.position.ToString());
               
                foreach(MonoBehaviour mono in camera.gameObject.GetComponents<MonoBehaviour>())
                {
                    GUILayout.Label("[mono] " + mono.GetType().ToString());
                }
            }

            GUILayout.EndHorizontal();
        }

        private bool renderDepth = false;

        void DoCamerasWindow(int index)
        {
            renderDepth = GUILayout.Toggle(renderDepth, "");

            int h = 0;
            foreach(var hook in cameraHooks)
            {
                GUILayout.Label(hook.Value.gameObject.name);
                GUI.DrawTexture(new Rect(16, 16 + h, 320, 180), hook.Value.targetTexture);
                h += 184;
            }
        }

        void OnGUI()
        {
            GUI.Window(0, new Rect(32, 32, 400, 600), DoCamerasWindow, "Cameras");
        }

        void OnPostRender()
        {
           RenderTexture.active = null;

           GL.Clear(true, true, new Color(0, 0, 0, 1.0f));

           print("start frame");

            foreach(var hook in cameraHooks)
            {
                print("blitting " + hook.Value.gameObject.name);
                if(renderDepth)
                {
                    Graphics.Blit(hook.Value.targetTexture, (RenderTexture)null, Shaders.GetMaterial("RenderDepth"));
                }
                else
                {
                    Graphics.Blit(hook.Value.targetTexture, (RenderTexture)null);
                }
            }
        }

    }

}
