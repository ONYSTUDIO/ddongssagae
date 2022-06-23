using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Helen
{
    public abstract class MonoScene : MonoObject
    {
        public Camera SceneCamera
        {
            get => sceneCamera;
        }

        public virtual IEnumerable<Camera> OverlayCameras
        {
            get => null;
        }

        public bool MainScene
        {
            get; set;
        } = true;

        [SerializeField]
        private Camera sceneCamera = null;

        public IContainer Container
        {
            get; private set;
        }

        public CancellationToken CancellationToken
        {
            get
            {
                if (cancellationTokenSource == null)
                    cancellationTokenSource = new CancellationTokenSource();
                return cancellationTokenSource.Token;
            }
        }

        private CancellationTokenSource cancellationTokenSource;

        protected override void Initialize(
            SafeOverloading safeOverloading = null)
        {
            Initialize(SceneContext.Container);
        }

        protected virtual void Initialize(
            IContainer container, SafeOverloading safeOverloading = null)
        {
            base.Initialize();

            Container = container;
        }

        public override async UniTask Activate()
        {
            if (Active.Value)
                return;

            // UnloadUnusedAssets 내부에서 GC.Collect를 호출한다고 함.
            // https://forum.unity.com/threads/resources-unloadunusedassets-vs-gc-collect.358597/#post-2321896
            if (MainScene)
                await Resources.UnloadUnusedAssets();

            // #FIXME 주석해제 필요
            // await MaterialControllService.Initialize();

            await base.Activate();

            if (MainScene)
                SetupCamera();
        }

        private void SetupCamera()
        {
            if (SceneCamera != null)
            {
                SceneCamera.gameObject.tag = "MainCamera";
                SceneCamera.gameObject.name = "MainCamera";

                // #FIXME 주석해제 필요
                // CameraService.SetStackingBase(SceneCamera);
                // CameraService.ClearStackedOverlay();
                // SceneCamera.GetOrAddComponent<WorldVisibilityCamera>().Refresh();

                if (OverlayCameras != null)
                {
                    int index = 0;
                    foreach (Camera overlayCamera in OverlayCameras)
                    {
                        overlayCamera.gameObject.tag = "Untagged";
                        overlayCamera.gameObject.name = $"OverlayCamera_{index++}";
                        // #FIXME 주석해제 필요
                        // CameraService.AddStackingOverlay(overlayCamera);
                    }
                }
                // #FIXME 주석해제 필요
                // CameraService.AddStackingOverlay(Environment.UICamera);
                // Environment.UICamera.GetOrAddComponent<WorldVisibilityUICamera>().Refresh();
            }
            else
            {
                // #FIXME 주석해제 필요
                // CameraService.SetStackingBase(Camera.main);
                // CameraService.ClearStackedOverlay();
            }
        }

        protected void SetSceneCamera(Camera camera)
        {
            sceneCamera = camera;
        }

        protected override UniTask OnActivate()
        {
            return UniTask.FromResult(default(object));
        }

        protected override void OnDeactivate()
        {
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }
        }

        protected override void OnFinalize()
        {
        }
    }
}