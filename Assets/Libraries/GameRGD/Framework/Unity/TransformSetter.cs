using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformSetter : MonoBehaviour
{
    public bool is_attach = true;

    //public bool local_offset = true;

    //public Vector3 offset_pos;
    //public Vector3 offset_rot;

    //public Vector3 world_pos;
    //public Vector3 world_rot;

    public string parent_name = string.Empty;
    public bool stop_when_anim_stop;

    public bool ignore_parent_rotate = false;
    public bool ignore_parent_scale = false;
    public bool ignore_parent_position = false;

    Vector3 oriPos;
    Quaternion oriRot;
    Vector3 oriScale;

    protected void SetOriginal( Transform tf )
    {
        oriPos = tf.localPosition;
        oriRot = tf.localRotation;
        oriScale = tf.localScale;
    }

    private void Awake()
    {
        SetOriginal(transform);
    }

    protected virtual void OnEnable()
    {
        SetTransform(); 
    }

    public void SetTransform()
    {
        if (transform.parent != null && !string.IsNullOrEmpty(parent_name))
        {
            var newParent = transform.parent.Find(parent_name);
            if( newParent != null )
            {
                transform.parent = newParent;
                transform.localPosition = oriPos;
                transform.localRotation = oriRot;
                transform.localScale = oriScale;
            }
        }

        if (transform.parent != null)
        {
            if (ignore_parent_position)
                transform.position = oriPos;
            if (ignore_parent_rotate)
                transform.rotation = oriRot;
            if (ignore_parent_scale)
            {
                var preParent = transform.parent;
                transform.parent = null;
                transform.localScale = oriScale;
                transform.parent = preParent;
            }
        }

        if (!is_attach)
        {
            transform.parent = null;
        }

        //if (is_attach)
        //{
        //    if (parent)
        //    {
        //        Vector3 tempLocalPos = transform.localPosition;
        //        gameObject.transform.SetParent(parent.transform, false);
        //        transform.localPosition = tempLocalPos;

        //        if (ignore_parent_scale)
        //        {
        //            Vector3 temp = parent.transform.localScale;
        //            transform.localScale = new Vector3(1f / temp.x, 1f / temp.y, 1f / temp.z);
        //            temp = transform.localPosition;
        //            temp.Scale(transform.localScale);
        //            transform.localPosition = temp;
        //        }
        //    }
        //}
        //else
        //{
        //    if (parent)
        //    {
        //        if (!ignore_rotate)
        //            gameObject.transform.rotation = parent.transform.rotation;

        //        if (ignore_parent_scale)
        //        {
        //            Vector3 temp = parent.transform.localScale;
        //            transform.localScale = new Vector3(1f / temp.x, 1f / temp.y, 1f / temp.z);
        //            temp = transform.localPosition;
        //            temp.Scale(transform.localScale);
        //            transform.localPosition = temp;
        //        }

        //        gameObject.transform.position = parent.transform.position;
        //    }
        //    gameObject.transform.SetParent(null);
        //}

        //if (local_offset)
        //{
        //    offset_pos.Set(offset_pos.z, offset_pos.y, offset_pos.x);
        //    gameObject.transform.position += (gameObject.transform.rotation * offset_pos);
        //    if (!ignore_rotate)
        //    {
        //        offset_rot.Set(offset_rot.z, offset_rot.y, offset_rot.x);
        //        gameObject.transform.Rotate(offset_rot);
        //    }
        //}
        //else
        //{
        //    gameObject.transform.position = world_pos;
        //    gameObject.transform.rotation = Quaternion.Euler(world_rot);
        //}
    }
}
