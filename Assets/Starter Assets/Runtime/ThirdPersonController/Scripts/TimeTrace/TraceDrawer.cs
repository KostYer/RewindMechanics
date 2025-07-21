using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace RewindSystem
{
    public class TraceDrawer: MonoBehaviour
    {
        [SerializeField] private SkinnedMeshRenderer _ghostMeshRenderer;
        [SerializeField] private Material _ghostBaseMaterial;
        [SerializeField] private TraceSettingsSO _traceSettings;
     //   [SerializeField] private float _delay = .3f;
     //   [SerializeField] private float traceUnitLifetime = 14.5f;
        private CancellationTokenSource _tokenSource;
       
        
        public void StartDraw()
        {
            _tokenSource?.Cancel();  
            _tokenSource?.Dispose();
            _tokenSource = new CancellationTokenSource();
            
            DrawPeriodically(_tokenSource.Token).Forget();
        }
        
        public void StopDraw()
        {
            _tokenSource?.Cancel();  
            _tokenSource?.Dispose();
            _tokenSource = null;
        }
        
        private async UniTask DrawPeriodically(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                BakeMashSnapshot();
                await UniTask.Delay((int)(_traceSettings.TraceUnitSpawnRate * 1000), DelayType.DeltaTime);
            }
        }

        private void BakeMashSnapshot()
        {
            
            Mesh bakedMesh = new Mesh();
            _ghostMeshRenderer.BakeMesh(bakedMesh);

            GameObject snapshot = new GameObject("BakedMeshSnapshot");
            snapshot.transform.position = _ghostMeshRenderer.transform.position;
            snapshot.transform.rotation = _ghostMeshRenderer.transform.rotation;
            snapshot.transform.localScale = _ghostMeshRenderer.transform.lossyScale;

            var meshFilter = snapshot.AddComponent<MeshFilter>();
            var meshRenderer = snapshot.AddComponent<MeshRenderer>();

            meshFilter.mesh = bakedMesh;
            
            Material[] newMaterials = new Material[bakedMesh.subMeshCount];
            for (int i = 0; i < bakedMesh.subMeshCount; i++)
            {
                newMaterials[i] = new Material(_ghostBaseMaterial); 
                newMaterials[i].SetFloat("_Alpha", _traceSettings.StartingAlpha);  
             ///   newMaterials[i].SetColor("_GhostColor", _traceSettings.TraceColor);  
            }
            meshRenderer.materials = newMaterials;

            var traceUnit = new TraceUnit(snapshot);
            traceUnit.FadeOutAsync(_traceSettings.TraceUnitLifetime, _traceSettings.StartingAlpha).Forget();

        }

    }
}