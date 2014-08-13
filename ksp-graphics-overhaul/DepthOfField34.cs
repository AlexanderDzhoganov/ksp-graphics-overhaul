using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ksp_graphics_overhaul
{
    // Converted from UnityScript to C# at http://www.M2H.nl/files/js_to_c.php - by Mike Hergaarden
    // Do test the code! You usually need to change a few small bits.

    using UnityEngine;
    using System.Collections;

    enum Dof34QualitySetting
    {
        OnlyBackground = 1,
        BackgroundAndForeground = 2,
    }

    enum DofResolution
    {
        High = 2,
        Medium = 3,
        Low = 4,
    }

    enum DofBlurriness
    {
        Low = 1,
        High = 2,
        VeryHigh = 4,
    }

    enum BokehDestination
    {
        Background = 0x1,
        Foreground = 0x2,
        BackgroundAndForeground = 0x3,
    }
    class PostEffectsBase : MonoBehaviour
    {
        protected bool supportHDRTextures = true;
        protected bool supportDX11 = false;
        protected bool isSupported = true;

        public Material CheckShaderAndCreateMaterial(Shader s, Material m2Create)
        {
            if (!s)
            {
                Debug.Log("Missing shader in " + this.ToString());
                enabled = false;
                return null;
            }

            if (s.isSupported && m2Create && m2Create.shader == s)
                return m2Create;

            if (!s.isSupported)
            {
                NotSupported();
                Debug.Log("The shader " + s.ToString() + " on effect " + this.ToString() + " is not supported on this platform!");
                return null;
            }
            else
            {
                m2Create = new Material(s);
                m2Create.hideFlags = HideFlags.DontSave;
                if (m2Create)
                    return m2Create;
                else return null;
            }
        }

        Material CreateMaterial(Shader s, Material m2Create)
        {
            if (!s)
            {
                Debug.Log("Missing shader in " + this.ToString());
                return null;
            }

            if (m2Create && (m2Create.shader == s) && (s.isSupported))
                return m2Create;

            if (!s.isSupported)
            {
                return null;
            }
            else
            {
                m2Create = new Material(s);
                m2Create.hideFlags = HideFlags.DontSave;
                if (m2Create)
                    return m2Create;
                else return null;
            }
        }

        void OnEnable()
        {
            isSupported = true;
        }

        public bool CheckSupport()
        {
            return CheckSupport(false);
        }

        public virtual bool CheckResources()
        {
            Debug.LogWarning("CheckResources () for " + this.ToString() + " should be overwritten.");
            return isSupported;
        }

        void Start()
        {
            CheckResources();
        }

        bool CheckSupport(bool needDepth)
        {
            isSupported = true;
            supportHDRTextures = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf);
            supportDX11 = SystemInfo.graphicsShaderLevel >= 50 && SystemInfo.supportsComputeShaders;

            if (!SystemInfo.supportsImageEffects || !SystemInfo.supportsRenderTextures)
            {
                NotSupported();
                return false;
            }

            if (needDepth && !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
            {
                NotSupported();
                return false;
            }

            if (needDepth)
                camera.depthTextureMode |= DepthTextureMode.Depth;

            return true;
        }

        public bool CheckSupport(bool needDepth, bool needHdr)
        {
            if (!CheckSupport(needDepth))
                return false;

            if (needHdr && !supportHDRTextures)
            {
                NotSupported();
                return false;
            }

            return true;
        }

        bool Dx11Support()
        {
            return supportDX11;
        }

        public void ReportAutoDisable()
        {
            Debug.LogWarning("The image effect " + this.ToString() + " has been disabled as it's not supported on the current platform.");
        }

        // deprecated but needed for old effects to survive upgrading
        bool CheckShader(Shader s)
        {
            Debug.Log("The shader " + s.ToString() + " on effect " + this.ToString() + " is not part of the Unity 3.2f+ effects suite anymore. For best performance and quality, please ensure you are using the latest Standard Assets Image Effects (Pro only) package.");
            if (!s.isSupported)
            {
                NotSupported();
                return false;
            }
            else
            {
                return false;
            }
        }

        void NotSupported()
        {
            enabled = false;
            isSupported = false;
            return;
        }

        void DrawBorder(RenderTexture dest, Material material)
        {
            float x1;
            float x2;
            float y1;
            float y2;

            RenderTexture.active = dest;
            bool invertY = true; // source.texelSize.y < 0.0ff;
            // Set up the simple Matrix
            GL.PushMatrix();
            GL.LoadOrtho();

            for (int i = 0; i < material.passCount; i++)
            {
                material.SetPass(i);

                float y1_; float y2_;
                if (invertY)
                {
                    y1_ = 1.0f; y2_ = 0.0f;
                }
                else
                {
                    y1_ = 0.0f; y2_ = 1.0f;
                }

                // left	        
                x1 = 0.0f;
                x2 = 0.0f + 1.0f / (dest.width * 1.0f);
                y1 = 0.0f;
                y2 = 1.0f;
                GL.Begin(GL.QUADS);

                GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
                GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
                GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
                GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);

                // right
                x1 = 1.0f - 1.0f / (dest.width * 1.0f);
                x2 = 1.0f;
                y1 = 0.0f;
                y2 = 1.0f;

                GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
                GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
                GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
                GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);

                // top
                x1 = 0.0f;
                x2 = 1.0f;
                y1 = 0.0f;
                y2 = 0.0f + 1.0f / (dest.height * 1.0f);

                GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
                GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
                GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
                GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);

                // bottom
                x1 = 0.0f;
                x2 = 1.0f;
                y1 = 1.0f - 1.0f / (dest.height * 1.0f);
                y2 = 1.0f;

                GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
                GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
                GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
                GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);

                GL.End();
            }

            GL.PopMatrix();
        }
    }

    class DepthOfField34 : PostEffectsBase
    {

        static private int SMOOTH_DOWNSAMPLE_PASS = 6;
        static private float BOKEH_EXTRA_BLUR = 2.0f;

        public Dof34QualitySetting quality = Dof34QualitySetting.OnlyBackground;
        public DofResolution resolution = DofResolution.Low;
        public bool simpleTweakMode = true;

        public float focalPoint = 1.0f;
        public float smoothness = 0.5f;

        public float focalZDistance = 0.0f;
        public float focalZStartCurve = 1.0f;
        public float focalZEndCurve = 1.0f;

        private float focalStartCurve = 2.0f;
        private float focalEndCurve = 2.0f;
        private float focalDistance01 = 0.1f;

        public Transform objectFocus = null;
        public float focalSize = 0.0f;

        public DofBlurriness bluriness = DofBlurriness.High;
        public float maxBlurSpread = 1.75f;

        public float foregroundBlurExtrude = 1.15f;

        public Shader dofBlurShader;
        private Material dofBlurMaterial = null;

        public Shader dofShader;
        private Material dofMaterial = null;

        public bool visualize = false;
        public BokehDestination bokehDestination = BokehDestination.Background;

        private float widthOverHeight = 1.25f;
        private float oneOverBaseSize = 1.0f / 512.0f;

        public bool bokeh = false;
        public bool bokehSupport = true;
        public Shader bokehShader;
        public Texture2D bokehTexture;
        public float bokehScale = 2.4f;
        public float bokehIntensity = 0.15f;
        public float bokehThreshholdContrast = 0.1f;
        public float bokehThreshholdLuminance = 0.55f;
        public int bokehDownsample = 1;
        private Material bokehMaterial;

        void CreateMaterials()
        {
            dofBlurMaterial = CheckShaderAndCreateMaterial(dofBlurShader, dofBlurMaterial);
            dofMaterial = CheckShaderAndCreateMaterial(dofShader, dofMaterial);
            bokehSupport = bokehShader.isSupported;

            if (bokeh && bokehSupport && bokehShader)
                bokehMaterial = CheckShaderAndCreateMaterial(bokehShader, bokehMaterial);
        }

        public override bool CheckResources()
        {
            //	 CheckSupport(true);

            dofBlurMaterial = CheckShaderAndCreateMaterial(dofBlurShader, dofBlurMaterial);
            dofMaterial = CheckShaderAndCreateMaterial(dofShader, dofMaterial);
            bokehSupport = bokehShader.isSupported;

            if (bokeh && bokehSupport && bokehShader)
                bokehMaterial = CheckShaderAndCreateMaterial(bokehShader, bokehMaterial);

            if (!isSupported)
                ReportAutoDisable();
            return isSupported;
        }

        void OnDisable()
        {
            //Quads.Cleanup ();	
        }

        void OnEnable()
        {
            camera.depthTextureMode |= DepthTextureMode.Depth;
        }

        float FocalDistance01(float worldDist)
        {
            return camera.WorldToViewportPoint((worldDist - camera.nearClipPlane) * camera.transform.forward + camera.transform.position).z / (camera.farClipPlane - camera.nearClipPlane);
        }

        int GetDividerBasedOnQuality()
        {
            int divider = 1;
            if (resolution == DofResolution.Medium)
                divider = 2;
            else if (resolution == DofResolution.Low)
                divider = 2;
            return divider;
        }

        int GetLowResolutionDividerBasedOnQuality(int baseDivider)
        {
            int lowTexDivider = baseDivider;
            if (resolution == DofResolution.High)
                lowTexDivider *= 2;
            if (resolution == DofResolution.Low)
                lowTexDivider *= 2;
            return lowTexDivider;
        }

        private RenderTexture foregroundTexture = null;
        private RenderTexture mediumRezWorkTexture = null;
        private RenderTexture finalDefocus = null;
        private RenderTexture lowRezWorkTexture = null;
        private RenderTexture bokehSource = null;
        private RenderTexture bokehSource2 = null;

        public void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (CheckResources() == false)
            {
                Graphics.Blit(source, destination);
                return;
            }

            if (smoothness < 0.1f)
                smoothness = 0.1f;

            // update needed focal & rt size parameter

            bokeh = bokeh && bokehSupport;
            float bokehBlurAmplifier = bokeh ? BOKEH_EXTRA_BLUR : 1.0f;

            bool blurForeground = quality > Dof34QualitySetting.OnlyBackground;
            float focal01Size = focalSize / (camera.farClipPlane - camera.nearClipPlane); ;

            if (simpleTweakMode)
            {
                focalDistance01 = objectFocus ? (camera.WorldToViewportPoint(objectFocus.position)).z / (camera.farClipPlane) : FocalDistance01(focalPoint);
                focalStartCurve = focalDistance01 * smoothness;
                focalEndCurve = focalStartCurve;
                blurForeground = blurForeground && (focalPoint > (camera.nearClipPlane + Mathf.Epsilon));
            }
            else
            {
                if (objectFocus)
                {
                    Vector3 vpPoint = camera.WorldToViewportPoint(objectFocus.position);
                    vpPoint.z = (vpPoint.z) / (camera.farClipPlane);
                    focalDistance01 = vpPoint.z;
                }
                else
                    focalDistance01 = FocalDistance01(focalZDistance);

                focalStartCurve = focalZStartCurve;
                focalEndCurve = focalZEndCurve;
                blurForeground = blurForeground && (focalPoint > (camera.nearClipPlane + Mathf.Epsilon));
            }

            widthOverHeight = (1.0f * source.width) / (1.0f * source.height);
            oneOverBaseSize = 1.0f / 512.0f;

            dofMaterial.SetFloat("_ForegroundBlurExtrude", foregroundBlurExtrude);
            dofMaterial.SetVector("_CurveParams", new Vector4(simpleTweakMode ? 1.0f / focalStartCurve : focalStartCurve, simpleTweakMode ? 1.0f / focalEndCurve : focalEndCurve, focal01Size * 0.5f, focalDistance01));
            dofMaterial.SetVector("_InvRenderTargetSize", new Vector4(1.0f / (1.0f * source.width), 1.0f / (1.0f * source.height), 0.0f, 0.0f));

            int divider = GetDividerBasedOnQuality();
            int lowTexDivider = GetLowResolutionDividerBasedOnQuality(divider);

            AllocateTextures(blurForeground, source, divider, lowTexDivider);

            // WRITE COC to alpha channel		
            // source is only being bound to detect y texcoord flip
            Graphics.Blit(source, source, dofMaterial, 3);

            // better DOWNSAMPLE (could actually be weighted for higher quality)
            Downsample(source, mediumRezWorkTexture);

            // BLUR A LITTLE first, which has two purposes
            // 1.) reduce jitter, noise, aliasing
            // 2.) produce the little-blur buffer used in composition later     	     
            Blur(mediumRezWorkTexture, mediumRezWorkTexture, DofBlurriness.Low, 4, maxBlurSpread);

            if (bokeh && (bokehDestination & BokehDestination.Background) != 0)
            {
                dofMaterial.SetVector("_Threshhold", new Vector4(bokehThreshholdContrast, bokehThreshholdLuminance, 0.95f, 0.0f));

                // add and mark the parts that should end up as bokeh shapes
                Graphics.Blit(mediumRezWorkTexture, bokehSource2, dofMaterial, 11);

                // remove those parts (maybe even a little tittle bittle more) from the regurlarly blurred buffer		
                //Graphics.Blit (mediumRezWorkTexture, lowRezWorkTexture, dofMaterial, 10);
                Graphics.Blit(mediumRezWorkTexture, lowRezWorkTexture);//, dofMaterial, 10);

                // maybe you want to reblur the small blur ... but not really needed.
                //Blur (mediumRezWorkTexture, mediumRezWorkTexture, DofBlurriness.Low, 4, maxBlurSpread);						

                // bigger BLUR
                Blur(lowRezWorkTexture, lowRezWorkTexture, bluriness, 0, maxBlurSpread * bokehBlurAmplifier);
            }
            else
            {
                // bigger BLUR
                Downsample(mediumRezWorkTexture, lowRezWorkTexture);
                Blur(lowRezWorkTexture, lowRezWorkTexture, bluriness, 0, maxBlurSpread);
            }

            dofBlurMaterial.SetTexture("_TapLow", lowRezWorkTexture);
            dofBlurMaterial.SetTexture("_TapMedium", mediumRezWorkTexture);
            Graphics.Blit(null, finalDefocus, dofBlurMaterial, 3);

            // we are only adding bokeh now if the background is the only part we have to deal with
            if (bokeh && (bokehDestination & BokehDestination.Background) != 0)
                AddBokeh(bokehSource2, bokehSource, finalDefocus);

            dofMaterial.SetTexture("_TapLowBackground", finalDefocus);
            dofMaterial.SetTexture("_TapMedium", mediumRezWorkTexture); // needed for debugging/visualization

            // FINAL DEFOCUS (background)
            Graphics.Blit(source, blurForeground ? foregroundTexture : destination, dofMaterial, visualize ? 2 : 0);

            // FINAL DEFOCUS (foreground)
            if (blurForeground)
            {
                // WRITE COC to alpha channel			
                Graphics.Blit(foregroundTexture, source, dofMaterial, 5);

                // DOWNSAMPLE (unweighted)
                Downsample(source, mediumRezWorkTexture);

                // BLUR A LITTLE first, which has two purposes
                // 1.) reduce jitter, noise, aliasing
                // 2.) produce the little-blur buffer used in composition later   
                BlurFg(mediumRezWorkTexture, mediumRezWorkTexture, DofBlurriness.Low, 2, maxBlurSpread);

                if (bokeh && (bokehDestination & BokehDestination.Foreground) != 0)
                {
                    dofMaterial.SetVector("_Threshhold", new Vector4(bokehThreshholdContrast * 0.5f, bokehThreshholdLuminance, 0.0f, 0.0f));

                    // add and mark the parts that should end up as bokeh shapes
                    Graphics.Blit(mediumRezWorkTexture, bokehSource2, dofMaterial, 11);

                    // remove the parts (maybe even a little tittle bittle more) that will end up in bokeh space			
                    //Graphics.Blit (mediumRezWorkTexture, lowRezWorkTexture, dofMaterial, 10);
                    Graphics.Blit(mediumRezWorkTexture, lowRezWorkTexture);//, dofMaterial, 10);

                    // big BLUR		
                    BlurFg(lowRezWorkTexture, lowRezWorkTexture, bluriness, 1, maxBlurSpread * bokehBlurAmplifier);
                }
                else
                {
                    // big BLUR		
                    BlurFg(mediumRezWorkTexture, lowRezWorkTexture, bluriness, 1, maxBlurSpread);
                }

                // simple upsample once						
                Graphics.Blit(lowRezWorkTexture, finalDefocus);

                dofMaterial.SetTexture("_TapLowForeground", finalDefocus);
                Graphics.Blit(source, destination, dofMaterial, visualize ? 1 : 4);

                if (bokeh && (bokehDestination & BokehDestination.Foreground) != 0)
                    AddBokeh(bokehSource2, bokehSource, destination);
            }

            ReleaseTextures();
        }

        void Blur(RenderTexture from, RenderTexture to, DofBlurriness iterations, int blurPass, float spread)
        {
            RenderTexture tmp = RenderTexture.GetTemporary(to.width, to.height);
            if ((int)iterations > 1)
            {
                BlurHex(from, to, blurPass, spread, tmp);
                if ((int)iterations > 2)
                {
                    dofBlurMaterial.SetVector("offsets", new Vector4(0.0f, spread * oneOverBaseSize, 0.0f, 0.0f));
                    Graphics.Blit(to, tmp, dofBlurMaterial, blurPass);
                    dofBlurMaterial.SetVector("offsets", new Vector4(spread / widthOverHeight * oneOverBaseSize, 0.0f, 0.0f, 0.0f));
                    Graphics.Blit(tmp, to, dofBlurMaterial, blurPass);
                }
            }
            else
            {
                dofBlurMaterial.SetVector("offsets", new Vector4(0.0f, spread * oneOverBaseSize, 0.0f, 0.0f));
                Graphics.Blit(from, tmp, dofBlurMaterial, blurPass);
                dofBlurMaterial.SetVector("offsets", new Vector4(spread / widthOverHeight * oneOverBaseSize, 0.0f, 0.0f, 0.0f));
                Graphics.Blit(tmp, to, dofBlurMaterial, blurPass);
            }
            RenderTexture.ReleaseTemporary(tmp);
        }

        void BlurFg(RenderTexture from, RenderTexture to, DofBlurriness iterations, int blurPass, float spread)
        {
            // we want a nice, big coc, hence we need to tap once from this (higher resolution) texture
            dofBlurMaterial.SetTexture("_TapHigh", from);

            RenderTexture tmp = RenderTexture.GetTemporary(to.width, to.height);
            if ((int)iterations > 1)
            {
                BlurHex(from, to, blurPass, spread, tmp);
                if ((int)iterations > 2)
                {
                    dofBlurMaterial.SetVector("offsets", new Vector4(0.0f, spread * oneOverBaseSize, 0.0f, 0.0f));
                    Graphics.Blit(to, tmp, dofBlurMaterial, blurPass);
                    dofBlurMaterial.SetVector("offsets", new Vector4(spread / widthOverHeight * oneOverBaseSize, 0.0f, 0.0f, 0.0f));
                    Graphics.Blit(tmp, to, dofBlurMaterial, blurPass);
                }
            }
            else
            {
                dofBlurMaterial.SetVector("offsets", new Vector4(0.0f, spread * oneOverBaseSize, 0.0f, 0.0f));
                Graphics.Blit(from, tmp, dofBlurMaterial, blurPass);
                dofBlurMaterial.SetVector("offsets", new Vector4(spread / widthOverHeight * oneOverBaseSize, 0.0f, 0.0f, 0.0f));
                Graphics.Blit(tmp, to, dofBlurMaterial, blurPass);
            }
            RenderTexture.ReleaseTemporary(tmp);
        }

        void BlurHex(RenderTexture from, RenderTexture to, int blurPass, float spread, RenderTexture tmp)
        {
            dofBlurMaterial.SetVector("offsets", new Vector4(0.0f, spread * oneOverBaseSize, 0.0f, 0.0f));
            Graphics.Blit(from, tmp, dofBlurMaterial, blurPass);
            dofBlurMaterial.SetVector("offsets", new Vector4(spread / widthOverHeight * oneOverBaseSize, 0.0f, 0.0f, 0.0f));
            Graphics.Blit(tmp, to, dofBlurMaterial, blurPass);
            dofBlurMaterial.SetVector("offsets", new Vector4(spread / widthOverHeight * oneOverBaseSize, spread * oneOverBaseSize, 0.0f, 0.0f));
            Graphics.Blit(to, tmp, dofBlurMaterial, blurPass);
            dofBlurMaterial.SetVector("offsets", new Vector4(spread / widthOverHeight * oneOverBaseSize, -spread * oneOverBaseSize, 0.0f, 0.0f));
            Graphics.Blit(tmp, to, dofBlurMaterial, blurPass);
        }

        void Downsample(RenderTexture from, RenderTexture to)
        {
            dofMaterial.SetVector("_InvRenderTargetSize", new Vector4(1.0f / (1.0f * to.width), 1.0f / (1.0f * to.height), 0.0f, 0.0f));
            Graphics.Blit(from, to, dofMaterial, SMOOTH_DOWNSAMPLE_PASS);
        }

        void AddBokeh(RenderTexture bokehInfo, RenderTexture tempTex, RenderTexture finalTarget)
        {
            if (bokehMaterial)
            {
                //	Mesh[] meshes = Quads.GetMeshes (tempTex.width, tempTex.height);	// quads: exchanging more triangles with less overdraw			

                RenderTexture.active = tempTex;
                GL.Clear(false, true, new Color(0.0f, 0.0f, 0.0f, 0.0f));

                GL.PushMatrix();
                GL.LoadIdentity();

                // point filter mode is important, otherwise we get bokeh shape & size artefacts
                bokehInfo.filterMode = FilterMode.Point;

                float arW = (bokehInfo.width * 1.0f) / (bokehInfo.height * 1.0f);
                float sc = 2.0f / (1.0f * bokehInfo.width);
                sc += bokehScale * maxBlurSpread * BOKEH_EXTRA_BLUR * oneOverBaseSize;

                bokehMaterial.SetTexture("_Source", bokehInfo);
                bokehMaterial.SetTexture("_MainTex", bokehTexture);
                bokehMaterial.SetVector("_ArScale", new Vector4(sc, sc * arW, 0.5f, 0.5f * arW));
                bokehMaterial.SetFloat("_Intensity", bokehIntensity);
                bokehMaterial.SetPass(0);

                //foreach(Mesh m in meshes)
                //		if (m) Graphics.DrawMeshNow (m, Matrix4x4.identity);	

                GL.PopMatrix();

                Graphics.Blit(tempTex, finalTarget, dofMaterial, 8);

                // important to set back as we sample from this later on
                bokehInfo.filterMode = FilterMode.Bilinear;
            }
        }


        void ReleaseTextures()
        {
            if (foregroundTexture) RenderTexture.ReleaseTemporary(foregroundTexture);
            if (finalDefocus) RenderTexture.ReleaseTemporary(finalDefocus);
            if (mediumRezWorkTexture) RenderTexture.ReleaseTemporary(mediumRezWorkTexture);
            if (lowRezWorkTexture) RenderTexture.ReleaseTemporary(lowRezWorkTexture);
            if (bokehSource) RenderTexture.ReleaseTemporary(bokehSource);
            if (bokehSource2) RenderTexture.ReleaseTemporary(bokehSource2);
        }

        void AllocateTextures(bool blurForeground, RenderTexture source, int divider, int lowTexDivider)
        {
            foregroundTexture = null;
            if (blurForeground)
                foregroundTexture = RenderTexture.GetTemporary(source.width, source.height, 0);
            mediumRezWorkTexture = RenderTexture.GetTemporary(source.width / divider, source.height / divider, 0);
            finalDefocus = RenderTexture.GetTemporary(source.width / divider, source.height / divider, 0);
            lowRezWorkTexture = RenderTexture.GetTemporary(source.width / lowTexDivider, source.height / lowTexDivider, 0);
            bokehSource = null;
            bokehSource2 = null;
            if (bokeh)
            {
                bokehSource = RenderTexture.GetTemporary(source.width / (lowTexDivider * bokehDownsample), source.height / (lowTexDivider * bokehDownsample), 0, RenderTextureFormat.ARGBHalf);
                bokehSource2 = RenderTexture.GetTemporary(source.width / (lowTexDivider * bokehDownsample), source.height / (lowTexDivider * bokehDownsample), 0, RenderTextureFormat.ARGBHalf);
                bokehSource.filterMode = FilterMode.Bilinear;
                bokehSource2.filterMode = FilterMode.Bilinear;
                RenderTexture.active = bokehSource2;
                GL.Clear(false, true, new Color(0.0f, 0.0f, 0.0f, 0.0f));
            }

            // to make sure: always use bilinear filter setting

            source.filterMode = FilterMode.Bilinear;
            finalDefocus.filterMode = FilterMode.Bilinear;
            mediumRezWorkTexture.filterMode = FilterMode.Bilinear;
            lowRezWorkTexture.filterMode = FilterMode.Bilinear;
            if (foregroundTexture)
                foregroundTexture.filterMode = FilterMode.Bilinear;
        }
    }

}
