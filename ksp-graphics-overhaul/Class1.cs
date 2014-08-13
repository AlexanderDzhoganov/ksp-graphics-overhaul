using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace ksp_graphics_overhaul
{

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class Class1 : MonoBehaviour
    {

        private List<Camera> cameras = new List<Camera>();
        private List<CameraHook> cameraHooks = new List<CameraHook>();
        private Camera dummyCamera = null;
        private Material renderDepthMaterial = null;
        private int hookid = 0;

        public void Awake()
        {
            print("ksp-graphics-overhaul: initialized");

            dummyCamera = this.gameObject.AddComponent<Camera>();
            dummyCamera.depth = 10;

            renderDepthMaterial = new Material(Shaders.RenderDepth2);

            /*int id = 0;

            foreach (Camera camera in GameObject.FindObjectsOfType(typeof(Camera)))
            {
                if(camera.gameObject.name == "Camera 01")
                {
                    var hook = camera.gameObject.AddComponent<CameraHook>();
                    hook.hookId = id;
                    cameraHooks.Add(hook);
                    id++;
                }
            }*/

            foreach(Shader s in Resources.FindObjectsOfTypeAll<Shader>())
            {
                print("shader: " + s.name);
            }
        }

        public void OnDestroy()
        {
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

                if(GUILayout.Button("add hook"))
                {
                    var hook = camera.gameObject.AddComponent<CameraHook>();
                    cameraHooks.Add(hook);
                }
            }

            GUILayout.EndHorizontal();
        }

        private bool renderDepth = false;
        private Vector2 cameraScrollPos;

        void DoCamerasWindow(int index)
        {
            renderDepth = GUILayout.Toggle(renderDepth, "");

            cameraScrollPos = GUILayout.BeginScrollView(cameraScrollPos);

            foreach (Camera camera in cameras)
            {
                OnGUICamera(camera);
            }

            if (GUILayout.Button("Refresh"))
            {
                cameras.Clear();

                foreach (Camera camera in GameObject.FindObjectsOfType(typeof(Camera)))
                {
                    cameras.Add(camera);
                }
            }

            foreach(CameraHook hook in cameraHooks)
            {
                if(GUILayout.Button("destroy"))
                {
                    Destroy(hook);
                    break;
                }
            }

            GUILayout.EndScrollView();
        }

        void OnGUI()
        {
            GUI.Window(0, new Rect(32, 32, 800, 1000), DoCamerasWindow, "Cameras");
        }

        void OnPostRender()
        {
            RenderTexture.active = null;

            GL.Clear(true, true, new Color(0, 0, 0, 1.0f));

            foreach(CameraHook hooks in cameraHooks)
            {
                if(renderDepth)
                {
                    Graphics.Blit(hooks.targetTexture, (RenderTexture)null, renderDepthMaterial);
                }
                else
                {
                    Graphics.Blit(hooks.targetTexture, (RenderTexture)null);
                }
            }
        }

    }

}
