using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CaptureScreenRendererFeature : ScriptableRendererFeature
{
    class CaptureScreenPass : ScriptableRenderPass
    {
        private RTHandle tempTexture;
        private string profilerTag = "Capture Screen Pass";

        public CaptureScreenPass()
        {
            // 初始化临时渲染目标
            tempTexture = RTHandles.Alloc(
                name: "_TemporaryScreenTexture",
                scaleFactor: Vector2.one,
                useDynamicScale: true
            );
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);

            // 使用 cameraColorTargetHandle 代替 cameraColorTarget
            var source = renderingData.cameraData.renderer.cameraColorTargetHandle;

            // Blit 到临时渲染目标
            Blit(cmd, source, tempTexture);
            cmd.SetGlobalTexture("_BackgroundTexture", tempTexture);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            // 确保释放 RTHandle
            RTHandles.Release(tempTexture);
        }
    }

    CaptureScreenPass captureScreenPass;

    public override void Create()
    {
        // 创建渲染通道实例并设置渲染事件
        captureScreenPass = new CaptureScreenPass
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingTransparents
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // 添加渲染通道
        renderer.EnqueuePass(captureScreenPass);
    }
}
