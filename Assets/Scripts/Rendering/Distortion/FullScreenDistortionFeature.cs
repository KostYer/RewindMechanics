using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DistortionRendering
{
public class FullScreenDistortionFeature: ScriptableRendererFeature
{    
    public float amount = 0.5f;
    class FullScreenMyPass : ScriptableRenderPass
    {
        private Material material;
        private RenderTargetHandle tempTexture;
        private  RenderTexture tempRT;

        public FullScreenMyPass(Material mat)
        {
            this.material = mat;
            tempTexture.Init("_TempColorTex");
          
        }
     
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (material == null || renderingData.cameraData.isSceneViewCamera) 
                return;

            CommandBuffer cmd = CommandBufferPool.Get("MyFullScreenEffect");

            var source = renderingData.cameraData.renderer.cameraColorTarget;
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
            desc.msaaSamples = 1;

            if (tempRT == null || tempRT.width != desc.width || tempRT.height != desc.height)
            {
                if (tempRT != null)
                    tempRT.Release();

                tempRT = new RenderTexture(desc);
                tempRT.Create();
            }

            cmd.SetViewport(new Rect(0, 0, desc.width, desc.height));

            // Copy screen to tempRT
            Blit(cmd, source, tempRT);

            material.SetTexture("_BlitTexture", tempRT);

            // Blit with effect back to source
            Blit(cmd, tempRT, source, material);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    [System.Serializable]
    public class Settings
    {
        public Material material;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public Settings settings = new Settings();
    private FullScreenMyPass pass;

    public override void Create()
    {
        pass = new FullScreenMyPass(settings.material)
        {
            renderPassEvent = settings.renderPassEvent
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(pass);  
    }
   }
}