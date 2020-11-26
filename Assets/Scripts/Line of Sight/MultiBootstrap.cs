using UnityEngine;

namespace Line_of_Sight
{
    public class MultiBootstrap : MonoBehaviour
    {
        public Material MultiplicationMat;
        public Camera BaseCamera, LosCamera;
    
        private RenderTexture _baseTex, _losTex;
        private int _width, _height;
    
        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        private static readonly int LoSTex = Shader.PropertyToID("_LoSTex");

        private void Awake()
        {
            GetScreenDimensions();
            CreateCameraOutputs();
            SetOutputsToMaterial();
        }

        private void Update()
        {
            if (_width == Screen.width && _height == Screen.height)
                return;
        
            ClearTex();
        
            GetScreenDimensions();
            CreateCameraOutputs();
            SetOutputsToMaterial();
        }

        private void GetScreenDimensions()
        {
            _width = Screen.width;
            _height = Screen.height;
        }

        private void CreateCameraOutputs()
        {
            _baseTex = new RenderTexture(_width, _height, 0, RenderTextureFormat.Default, 0);
            _losTex = new RenderTexture(_width, _height, 0, RenderTextureFormat.Default, 0);

        }

        private void SetOutputsToMaterial()
        {
            BaseCamera.targetTexture = _baseTex;
            LosCamera.targetTexture = _losTex;
        
            MultiplicationMat.SetTexture(BaseMap, _baseTex);
            MultiplicationMat.SetTexture(LoSTex, _losTex);
        }

        private void ClearTex()
        {
            _baseTex.Release();
            _losTex.Release();
        }

        private void OnDestroy()
        {
            ClearTex();
        }
    }
}
