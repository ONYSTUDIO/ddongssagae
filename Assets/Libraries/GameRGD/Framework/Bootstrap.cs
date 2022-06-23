using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Helen
{
    public static partial class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("코드 품질", "IDE0051:사용되지 않는 private 멤버 제거", Justification = "<보류 중>")]
        private static void Startup()
        {
#if UNITY_EDITOR
            Application.runInBackground = true;
#endif

            Scene scene = SceneManager.GetActiveScene();
            MonoScene monoScene = scene.FindComponentInRootObjects<MonoScene>();
            if (monoScene == null)
                return;

            MethodInfo methodInfo = monoScene.GetType().UnderlyingSystemType
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(mi => mi.GetCustomAttribute<StartupAttribute>(false) != null)
                .FirstOrDefault();
            if (methodInfo == null)
                return;

            methodInfo.Invoke(monoScene, null);
        }
    }
}