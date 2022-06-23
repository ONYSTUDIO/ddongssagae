using System;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace UniRx
{
    public static class ObservableSceneEvent
    {
        public static IObservable<Tuple<Scene, Scene>> ActiveSceneChangedAsObservable()
        {
            return Observable.FromEvent<UnityAction<Scene, Scene>, Tuple<Scene, Scene>>(
                h => (x, y) => h(Tuple.Create(x, y)),
                h => SceneManager.activeSceneChanged += h,
                h => SceneManager.activeSceneChanged -= h
            );
        }

        public static IObservable<Tuple<Scene, LoadSceneMode>> SceneLoadedAsObservable()
        {
            return Observable.FromEvent<UnityAction<Scene, LoadSceneMode>, Tuple<Scene, LoadSceneMode>>(
                h => (x, y) => h(Tuple.Create(x, y)),
                h => SceneManager.sceneLoaded += h,
                h => SceneManager.sceneLoaded -= h
            );
        }

        public static IObservable<Scene> SceneUnloadedAsObservable()
        {
            return Observable.FromEvent<UnityAction<Scene>, Scene>(
                h => h.Invoke,
                h => SceneManager.sceneUnloaded += h,
                h => SceneManager.sceneUnloaded -= h
            );
        }
    }
}