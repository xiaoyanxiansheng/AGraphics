using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SSAOFeature : ScriptableRendererFeature
{
    [Serializable]
    public class Setting
    {
        public int textureSize = 512;
        [Range(0,100)]
        public int blurCount = 1;
        [Range(0,1)]
        public float ssaoIntensity = 0.5f;
        public float intensityScale = 1;
        [Range(0,1f)]
        public float intensityPower = 1;
        [Range(0,0.01f)]
        public float intensityThreshold = 0;
        [Range(0,0.02f)]
        public float depthOffset = 0;
        public ComputeShader csShader;
        public Shader resoveShader;
    }
    
    public Setting _setting = new Setting();

    private SSAOPass _pass;

    public override void Create()
    {
        _pass = new SSAOPass(_setting);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (_setting.csShader == null || _setting.resoveShader == null) return;
        _pass.Setup();
        renderer.EnqueuePass(_pass);
    }
}

public class SSAOPass : ScriptableRenderPass
{
    private SSAOFeature.Setting _setting;
    
    private int _ssaoRTId = Shader.PropertyToID("_SSAORT");
    private RenderTargetIdentifier _ssaoRTIdIdentifier;
    private int _ssaoTempRTId = Shader.PropertyToID("_SSAOTempRT");
    private RenderTargetIdentifier _ssaoTempRTIdIdentifier;
    
    private int _ssaoTemp2RTId = Shader.PropertyToID("_SSAOTemp2RT");
    private RenderTargetIdentifier _ssaoTemp2RTIdIdentifier;

    private int _cameraDepthTexture = Shader.PropertyToID("_CameraDepthTexture");
    private int _cameraColorTexture = Shader.PropertyToID("_CameraColorTexture");

    private int _csMain = 0 , _csBlur = 0 , _csResove = 0;
    
    public SSAOPass(SSAOFeature.Setting setting)
    {
        renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        _setting = setting;
    }

    public void Setup()
    {
        _csMain = _setting.csShader.FindKernel("CSMain");
        _csBlur = _setting.csShader.FindKernel("CSBlur");
        _csResove = _setting.csShader.FindKernel("CSResove");
        ConfigureInput(ScriptableRenderPassInput.Depth);
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        cameraTextureDescriptor.enableRandomWrite = true;
        cmd.GetTemporaryRT(_ssaoTemp2RTId, cameraTextureDescriptor);
        _ssaoTemp2RTIdIdentifier = new RenderTargetIdentifier(_ssaoTemp2RTId);
        
        cameraTextureDescriptor.width = _setting.textureSize;
        cameraTextureDescriptor.height = _setting.textureSize;
        cmd.GetTemporaryRT(_ssaoRTId, cameraTextureDescriptor);
        _ssaoRTIdIdentifier = new RenderTargetIdentifier(_ssaoRTId);
        
        cmd.GetTemporaryRT(_ssaoTempRTId, cameraTextureDescriptor);
        _ssaoTempRTIdIdentifier = new RenderTargetIdentifier(_ssaoTempRTId);
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(_ssaoRTId);
        cmd.ReleaseTemporaryRT(_ssaoTempRTId);
        cmd.ReleaseTemporaryRT(_ssaoTemp2RTId);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd, new ProfilingSampler("SSAORT")))
        {
            // SSAO
            cmd.SetComputeVectorParam(_setting.csShader,"_cameraResolution",new Vector4(renderingData.cameraData.camera.pixelWidth,renderingData.cameraData.camera.pixelHeight,0,0));
            cmd.SetComputeFloatParam(_setting.csShader,"_textureSize",_setting.textureSize);
            cmd.SetComputeFloatParam(_setting.csShader,"_intensityScale",_setting.intensityScale);
            cmd.SetComputeFloatParam(_setting.csShader,"_intensityPower",_setting.intensityPower);
            cmd.SetComputeFloatParam(_setting.csShader,"_depthOffset",_setting.depthOffset);
            cmd.SetComputeFloatParam(_setting.csShader,"_intensityThreshold",_setting.intensityThreshold);
            cmd.SetComputeFloatParam(_setting.csShader,"_ssaoIntensity",_setting.ssaoIntensity);
            cmd.SetComputeMatrixParam(_setting.csShader,"_WorldToCameraMatrix",renderingData.cameraData.camera.worldToCameraMatrix);
            cmd.SetComputeTextureParam(_setting.csShader,_csMain,"_CameraDepthTexture",_cameraDepthTexture);
            cmd.SetComputeTextureParam(_setting.csShader,_csMain,"_SSAORT",_ssaoRTIdIdentifier);
            cmd.DispatchCompute(_setting.csShader,_csMain,_setting.textureSize/8,_setting.textureSize/8,1);
            
            // Blur
            RenderTargetIdentifier t1 = _ssaoTempRTIdIdentifier;
            RenderTargetIdentifier t2 = _ssaoRTIdIdentifier;
            RenderTargetIdentifier t3;
            for (int i = 0; i < _setting.blurCount; i++)
            {
                cmd.SetComputeTextureParam(_setting.csShader,_csBlur,"_SSAORT",t1);
                cmd.SetComputeTextureParam(_setting.csShader,_csBlur,"_SSAOTEMPRT",t2);
                cmd.DispatchCompute(_setting.csShader,_csBlur, _setting.textureSize/8,_setting.textureSize/8, 1);
                t3 = t1;
                t1 = t2;
                t2 = t3;
            }
            _ssaoTempRTIdIdentifier = t1;
            
            cmd.SetComputeTextureParam(_setting.csShader,_csResove,"_CameraColorTexture",_cameraColorTexture);
            cmd.SetComputeTextureParam(_setting.csShader,_csResove,"_SSAORT",_ssaoTempRTIdIdentifier);
            cmd.SetComputeTextureParam(_setting.csShader,_csResove,"_SSAOTEMP2RT",_ssaoTemp2RTIdIdentifier);
            cmd.DispatchCompute(_setting.csShader,_csResove,renderingData.cameraData.camera.pixelWidth/8,renderingData.cameraData.camera.pixelHeight/8,2);
            
            // 测试
            cmd.Blit(_ssaoTemp2RTIdIdentifier,_cameraColorTexture);
        }
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}

