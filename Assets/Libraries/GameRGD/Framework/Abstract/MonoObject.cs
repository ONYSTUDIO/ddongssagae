using System;
using UniRx;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Helen
{
    public abstract class MonoObject : MonoBehaviour, ICompositeDisposable
    {
        protected virtual void Initialize(
            SafeOverloading safeOverloading = null)
        {
            if (Active.Value)
                Deactivate();
        }

        public IReadOnlyReactiveProperty<bool> Active
        {
            get { return active; }
        }

        private readonly ReactiveProperty<bool> active =
            new ReactiveProperty<bool>();

        public virtual UniTask Activate()
        {
            if (active.Value)
                return UniTask.FromResult(default(object));

            active.Value = true;

            compositeDisposable.Clear();

            if (awakedValue == false)
                ObservableSceneEvent.SceneUnloadedAsObservable().Subscribe(OnUnloadScene).AddTo(this);

            return OnActivate();
        }

        public virtual void Deactivate(bool force = false)
        {
            if (!active.Value && !force)
                return;

            compositeDisposable.Clear();

            OnDeactivate();

            active.Value = false;
        }

        private bool awakedValue = false;

        private void Awake()
        {
            awakedValue = true;

            OnAwake();
        }

        private void OnDestroy()
        {
            Deactivate();
            OnFinalize();
        }

        private void OnUnloadScene(Scene scene)
        {
            if (scene.handle != gameObject.scene.handle)
                return;

            Deactivate();
            OnFinalize();
        }

#if KEEP // 나중에 혹시 필요할까?
        private void OnApplicationQuit()
        {
            Deactivate();
            OnFinalize();
        }
#endif

        protected virtual UniTask OnActivate()
        {
            return UniTask.FromResult(default(object));
        }

        protected virtual void OnDeactivate()
        {
        }

        protected virtual void OnAwake()
        {
        }

        protected virtual void OnFinalize()
        {
        }

        #region support ICompositeDisposable

        private readonly CompositeDisposable compositeDisposable =
            new CompositeDisposable();

        void ICompositeDisposable.Add(IDisposable item)
        {
            Assert.IsTrue(Active.Value);

            compositeDisposable.Add(item);
        }

        bool ICompositeDisposable.Remove(IDisposable item)
        {
            return compositeDisposable.Remove(item);
        }

        void ICompositeDisposable.Clear()
        {
            compositeDisposable.Clear();
        }

        #endregion support ICompositeDisposable
    }

    public static class MonoObjectExtensions
    {
        public static T InjectBy<T>(this T monoObject, IContainer container, params object[] customParams)
            where T : MonoObject
        {
            return container.Inject(monoObject, customParams);
        }

        public static void AddTo(this IDisposable disposable, MonoObject monoObject)
        {
            (monoObject as ICompositeDisposable).Add(disposable);
        }
    }
}