using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class UIBaseMeshEffect : UnityEngine.UI.BaseMeshEffect
{
    [SerializeField]
    private Vector2 m_ShadowDistance = new Vector2(3f, -3f);

    [SerializeField]
    private bool m_ShadowUseAlpha = false;

    [SerializeField]
    private Vector2 m_OutlineDistance = new Vector2(1f, -1f);

    [SerializeField]
    private bool m_OutlineUseAlpha = false;

    protected abstract Color color { get; }

    protected UIBaseMeshEffect()
    {
    }

    private const float kMaxEffectDistance = 600f;

    private static Vector2 ClampEffectDistance(Vector2 value)
    {
        if (value.x > kMaxEffectDistance)
            value.x = kMaxEffectDistance;
        if (value.x < -kMaxEffectDistance)
            value.x = -kMaxEffectDistance;

        if (value.y > kMaxEffectDistance)
            value.y = kMaxEffectDistance;
        if (value.y < -kMaxEffectDistance)
            value.y = -kMaxEffectDistance;

        return value;
    }

    private static void ApplyShadowZeroAlloc(
        List<UIVertex> verts, Color32 color, int start, int end, float x, float y, bool useAlpha)
    {
        UIVertex vt;

        var neededCapacity = verts.Count + end - start;
        if (verts.Capacity < neededCapacity)
            verts.Capacity = neededCapacity;

        for (int i = start; i < end; ++i)
        {
            vt = verts[i];
            verts.Add(vt);

            Vector3 v = vt.position;
            v.x += x;
            v.y += y;
            vt.position = v;
            var newColor = color;
            if (useAlpha)
                newColor.a = (byte)((newColor.a * verts[i].color.a) / 255);
            vt.color = newColor;
            verts[i] = vt;
        }
    }

    private static void ApplyShadow(List<UIVertex> verts, Color32 color, int start, int end, float x, float y, bool useAlpha)
    {
        ApplyShadowZeroAlloc(verts, color, start, end, x, y, useAlpha);
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
            return;

        var output = UIListPool<UIVertex>.Get();
        vh.GetUIVertexStream(output);

        if (m_ShadowDistance.x != 0f || m_ShadowDistance.y != 0f)
        { // shadow
            var shadowDistance = ClampEffectDistance(m_ShadowDistance);
            ApplyShadow(output, color, 0, output.Count, shadowDistance.x, shadowDistance.y, m_ShadowUseAlpha);
        }

        if (m_OutlineDistance.x != 0f || m_OutlineDistance.y != 0f)
        { // outline
            var outlineDistance = ClampEffectDistance(m_OutlineDistance);
            var neededCpacity = output.Count * 5;
            if (output.Capacity < neededCpacity)
                output.Capacity = neededCpacity;            

            var start = 0;
            var end = output.Count;
            ApplyShadowZeroAlloc(output, color, start, output.Count, outlineDistance.x, outlineDistance.y, m_OutlineUseAlpha);

            start = end;
            end = output.Count;
            ApplyShadowZeroAlloc(output, color, start, output.Count, outlineDistance.x, -outlineDistance.y, m_OutlineUseAlpha);

            start = end;
            end = output.Count;
            ApplyShadowZeroAlloc(output, color, start, output.Count, -outlineDistance.x, outlineDistance.y, m_OutlineUseAlpha);

            start = end;
            end = output.Count;
            ApplyShadowZeroAlloc(output, color, start, output.Count, -outlineDistance.x, -outlineDistance.y, m_OutlineUseAlpha);
        }

        vh.Clear();
        vh.AddUIVertexTriangleStream(output);
        UIListPool<UIVertex>.Release(output);
    }


#if UNITY_EDITOR
    protected override void Reset()
    {
        base.Reset();

        if (GetComponent<Outline>())
        {
            if (UnityEditor.EditorUtility.DisplayDialog(
                    "UI Effect 중복 사용",
                    "Outline과 TextEffect를 같이 사용할 수 없습니다. Outline을 삭제 후 컴포넌트를 추가해주세요.",
                    "확인"))
            {
                DestroyImmediate(this);
            }
        }
    }
#endif
}