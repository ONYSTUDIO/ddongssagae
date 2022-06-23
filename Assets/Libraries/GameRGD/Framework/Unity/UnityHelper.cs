using System;
using System.Collections;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class UnityYield
{
    private static WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

    public static WaitForEndOfFrame WaitForEndOfFrame() => waitForEndOfFrame;

    private static WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

    public static WaitForFixedUpdate WaitForFixedUpdate() => waitForFixedUpdate;

    public static WaitForSeconds WaitForSeconds(float time) => new WaitForSeconds(time);

    public static WaitForSecondsRealtime WaitForSecondsRealtime(float time) => new WaitForSecondsRealtime(time);

    public static WaitUntil WaitUntil(Func<bool> predicate) => new WaitUntil(predicate);

    public static WaitWhile WaitWhile(Func<bool> predicate) => new WaitWhile(predicate);
}

public static class UnityHelper
{
    public static string GetGameObjectPath(this GameObject gameObject)
    {
        string path = "/" + gameObject.name;
        while (gameObject.transform.parent != null)
        {
            gameObject = gameObject.transform.parent.gameObject;
            path = "/" + gameObject.name + path;
        }
        return path;
    }

    public static RectTransform GetRectTransform(this GameObject gameObject)
    {
        return gameObject.transform as RectTransform;
    }

    public static RectTransform GetRectTransform(this Component component)
    {
        return component.transform as RectTransform;
    }

    public static T GetOrAddComponent<T>(this GameObject gameObject)
            where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if (component == null)
            component = gameObject.AddComponent<T>();
        return component;
    }

    public static Component GetOrAddComponent(this GameObject gameObject, Type type)
    {
        Component component = gameObject.GetComponent(type);
        if (component == null)
            component = gameObject.AddComponent(type);
        return component;
    }

    public static void SetLayerRecursively(this GameObject gameObject, int layer)
    {
        foreach (Transform transform in gameObject.GetComponentsInChildren<Transform>(true))
            transform.gameObject.layer = layer;
    }

    public static T FindComponentInRootObjects<T>(this Scene scene, bool findInDeactiveObject = true)
        where T : Component
    {
        if (scene.isLoaded)
        {
            GameObject[] rootObjects = scene.GetRootGameObjects();
            for (int i = 0; i < rootObjects.Length; ++i)
            {
                if (!findInDeactiveObject && !rootObjects[i].activeInHierarchy)
                    continue;

                T component = rootObjects[i].GetComponent<T>();
                if (component != null)
                    return component;
            }
        }
        return null;
    }

    public static void DestroyChildren(this GameObject gameObject)
    {
        gameObject.transform.DestroyChildren();
    }

    public static void DestroyChildren(this Component component)
    {
        component.transform.DestroyChildren();
    }

    public static void DestroyChildren(this Transform transform)
    {
        int childCount = transform.childCount;
        for (int i = childCount - 1; i >= 0; --i)
            GameObject.Destroy(transform.GetChild(i).gameObject);
    }

    public static string GetArg(string name)
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == name && args.Length > i + 1)
            {
                return args[i + 1];
            }
        }
        return null;
    }
}

public static class ComponentHelper
{
    public static T GetOrAddComponent<T>(this Component component)
    where T : Component
    {
        return component.gameObject.GetOrAddComponent<T>();
    }
}

public static class AnimatorHelper
{
    public static async UniTask PlayAsync(
        this Animator animator, string name,
        int layer = 0, float normalizeTime = 0.0f,
        CancellationToken cancellationToken = default)
    {
        if (!animator || !animator.isActiveAndEnabled)
            return;

        int animHash = Animator.StringToHash(name);

        animator.Play(animHash, layer, normalizeTime);
        animator.Update(0.0f);

        while (!cancellationToken.IsCancellationRequested)
        {
            if (!animator || !animator.isActiveAndEnabled)
                break;

            if (!animator.IsInTransition(layer))
            {
                var currStateInfo = animator.GetCurrentAnimatorStateInfo(layer);
                if (currStateInfo.shortNameHash != animHash)
                    break;

                if (currStateInfo.normalizedTime >= 1.0f)
                    break;
            }

            await UniTask.Yield();
        }
    }
}

public static class TransformHelper
{
    public static Vector2 GetPositionXZ(this Transform transform)
    {
        Vector3 position = transform.position;
        return new Vector2(position.x, position.z);
    }

    public static Vector2 GetForwardXZ(this Transform transform)
    {
        Vector3 forward = transform.forward;
        return new Vector2(forward.x, forward.z);
    }

    public static Vector2 GetRightXZ(this Transform transform)
    {
        Vector3 right = transform.forward;
        return new Vector2(right.x, right.z);
    }

    public static Vector3 TransformPointUnscaled(this Transform transform, Vector3 position)
    {
        return transform.position + transform.rotation * position;
    }

    public static Vector3 InverseTransformPointUnscaled(this Transform transform, Vector3 position)
    {
        position -= transform.position;
        return Quaternion.Inverse(transform.rotation) * position;
    }
}

public static class RectTransformHelper
{
    public static void StretchToTarget(this RectTransform rectTransform, RectTransform targetRect)
    {
        if (rectTransform == null || targetRect == null)
            return;

        rectTransform.pivot = targetRect.pivot;
        rectTransform.position = targetRect.position;
        rectTransform.rotation = targetRect.rotation;

        rectTransform.localScale = Vector3Helper.Divide(targetRect.lossyScale, rectTransform.parent.lossyScale);
        rectTransform.sizeDelta = targetRect.rect.size;
        rectTransform.anchorMax = Vector2Helper.center;
        rectTransform.anchorMin = Vector2Helper.center;
    }
}

public static class EventSystemHelper
{
    public static GameObject CreateEventSystem()
    {
        GameObject go = new GameObject("EventSystem");

        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();

        return go;
    }
}

public static class Vector2Helper
{
    public static readonly Vector2 center = new Vector2(0.5f, 0.5f);

    public static Vector2 Rotate(this Vector2 v, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        float tx = v.x;
        float ty = v.y;

        return new Vector2(cos * tx - sin * ty, sin * tx + cos * ty);
    }

    public static Vector3 ConvertXZToVector3(this Vector2 v)
    {
        return new Vector3(v.x, 0f, v.y);
    }

    //public static Vector2 Rotate(this Vector2 v, float degrees)
    //{
    //    return Quaternion.Euler(0, 0, degrees) * v;
    //}
}

public static class Vector3Helper
{
    public static Vector2 GetXZ(this Vector3 v)
    {
        return new Vector2(v.x, v.z);
    }

    public static Vector3 Mul(Vector3 a, Vector3 b)
    {
        Vector3 result = new Vector3();
        result.x = a.x * b.x;
        result.y = a.y * b.y;
        result.z = a.z * b.z;
        return result;
    }

    public static Vector3 Divide(Vector3 a, Vector3 b)
    {
        Vector3 result = new Vector3();
        result.x = a.x / b.x;
        result.y = a.y / b.y;
        result.z = a.z / b.z;
        return result;
    }

    public static Vector3 MakeVec3(float value)
    {
        return new Vector3(value, value, value);
    }
}

public static class UnityUIHelper
{
    public static void SetPlaceHolderText(this InputField inputField, string value)
    {
        if (inputField.placeholder)
        {
            Text text = inputField.placeholder.GetComponent<Text>();
            if (text)
                text.text = value;
        }
    }

    public static void TimeLock(this Button button, float seconds)
    {
        button.StartCoroutine(TimeLockCoroutine(button, seconds));
    }

    private static IEnumerator TimeLockCoroutine(Button button, float time)
    {
        if (button)
            button.interactable = false;

        yield return UnityYield.WaitForSecondsRealtime(time);

        if (button)
            button.interactable = true;
    }

    public static async UniTask LockButton(this UniTask task, Button button, CancellationToken cancellationToken = default)
    {
        bool value = button.interactable;
        if (button)
            button.interactable = false;
        // await task.WithCancellation(cancellationToken); // UniTask 2.2.0 에서 변경
        await task.AttachExternalCancellation(cancellationToken);
        if (button)
            button.interactable = value;
    }

    public static async UniTask<T> LockButton<T>(this UniTask<T> task, Button button, CancellationToken cancellationToken = default)
    {
        bool value = button.interactable;
        if (button)
            button.interactable = false;
        // T ret = await task.WithCancellation(cancellationToken);  // UniTask 2.2.0 에서 변경
        T ret = await task.AttachExternalCancellation(cancellationToken);
        if (button)
            button.interactable = value;
        return ret;
    }
}

public static class StringHelper
{
    private static Regex spaceRegex = new Regex("[\\s]");

    public static bool ContainsSpace(this string str)
    {
        return spaceRegex.IsMatch(str);
    }

    private static Regex specialCharRegex = new Regex("[\\{\\}\\/?.,;:|*~`!^\\-_+<>@\\#$%&\\\\=\'\"]", RegexOptions.IgnoreCase);

    public static bool ContainsSpecialChar(this string str)
    {
        return specialCharRegex.IsMatch(str);
    }

    public static bool IgnoreEmojiTest { get; set; } = false;

#if UNUSED
    private static Regex emojiRegex = new Regex("(?:[\u2700-\u27bf]|(?:\ud83c[\udde6-\uddff]){2}|[\ud800-\udbff][\udc00-\udfff]|[\u0023-\u0039]\ufe0f?\u20e3|\u3299|\u3297|\u303d|\u3030|\u24c2|\ud83c[\udd70-\udd71]|\ud83c[\udd7e-\udd7f]|\ud83c\udd8e|\ud83c[\udd91-\udd9a]|\ud83c[\udde6-\uddff]|\ud83c[\ude01-\ude02]|\ud83c\ude1a|\ud83c\ude2f|\ud83c[\ude32-\ude3a]|\ud83c[\ude50-\ude51]|\u203c|\u2049|[\u25aa-\u25ab]|\u25b6|\u25c0|[\u25fb-\u25fe]|\u00a9|\u00ae|\u2122|\u2139|\ud83c\udc04|[\u2600-\u26FF]|\u2b05|\u2b06|\u2b07|\u2b1b|\u2b1c|\u2b50|\u2b55|\u231a|\u231b|\u2328|\u23cf|[\u23e9-\u23f3]|[\u23f8-\u23fa]|\ud83c\udccf|\u2934|\u2935|[\u2190-\u21ff])");
    public static bool ContainsEmoji(this string str)
    {
        if (IgnoreEmojiTest)
            return false;
        return emojiRegex.IsMatch(str);
    }
#else

    public static bool ContainsEmoji(this string value)
    {
        for (int i = 0; i < value.Length; ++i)
        {
            if (char.GetUnicodeCategory(value[i]) == System.Globalization.UnicodeCategory.Surrogate)
                return true;
        }
        return false;
    }

#endif
}