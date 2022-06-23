/*
Unity editor script to precisely move, rotate and scale GameObjects on 2D scenes, using the arrow keys.
Notes:
- To use it just include it on an "Assets/Editor" folder on your project.
- The action depends on the selected tool and the size of the movement depends on the Scene view zoom.
- The more "zoomed in" is the scene view the smaller is the movement step. 
- It will work when the current Scene tab is in 2D mode and there is at least one gameObject selected,
otherwise the scene camera will move as usual :)
David Darias. Lic: MIT. Dec 2017.
*/
#pragma warning disable 0618

using System;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR

namespace DoubleuGames.GameRGD
{
    [InitializeOnLoad]
    public class Arrows2DMovement
    {
        private static int rotationSnapAngle = 15;
        private static bool bEnabled = false;
        private const string MENU_NAME = "DoubleUGames/UI with Keyboard";

        [MenuItem(MENU_NAME)]
        private static void ToggleAction()
        {
            PerformAction(!bEnabled);
        }
        public static void PerformAction(bool enabled)
        {

            Menu.SetChecked(MENU_NAME, enabled);
            EditorPrefs.SetBool(MENU_NAME, enabled);

            bEnabled = enabled;
        }

        static Arrows2DMovement()
        {
            //avoid registering twice to the SceneGUI delegate
            SceneView.onSceneGUIDelegate -= OnSceneView;
            SceneView.onSceneGUIDelegate += OnSceneView;

            bEnabled = EditorPrefs.GetBool(MENU_NAME, false);

            /// Delaying until first editor tick so that the menu
            /// will be populated before setting check state, and
            /// re-apply correct action
            EditorApplication.delayCall += () =>
            {
                PerformAction(bEnabled);
            };
        }

        static void OnSceneView(SceneView sceneView)
        {

            if (!bEnabled) return;

            Event currentEvent = Event.current;

            //if the event is a keyDown on an orthographic camera
            if (currentEvent.isKey
            && currentEvent.type == EventType.KeyDown
            && (currentEvent.modifiers == EventModifiers.None || currentEvent.modifiers == EventModifiers.FunctionKey || currentEvent.modifiers == (EventModifiers.Control | EventModifiers.FunctionKey)) //arrow keys are function keys
            && sceneView.camera.orthographic)
            {
                //choose the right direction to move

                Vector2 scale = new Vector2(1, 1);
                if (currentEvent.modifiers == (EventModifiers.Control | EventModifiers.FunctionKey))
                {
                    scale = new Vector2(5, 5);
                }

                switch (currentEvent.keyCode)
                {
                    case KeyCode.RightArrow:
                        moveSelectedObjects(Vector3.right, scale, sceneView);
                        break;
                    case KeyCode.LeftArrow:
                        moveSelectedObjects(Vector3.left, scale, sceneView);
                        break;
                    case KeyCode.UpArrow:
                        moveSelectedObjects(Vector3.up, scale, sceneView);
                        break;
                    case KeyCode.DownArrow:
                        moveSelectedObjects(Vector3.down, scale, sceneView);
                        break;
                }
            }
        }

        private static void moveSelectedObjects(Vector3 direction, Vector2 scale, SceneView sceneView)
        {
            //the step size is a percent of the scene viewport
            // Vector2 cameraSize = getCameraSize(sceneView.camera);
            // Vector3 step = Vector3.Scale(direction, cameraSize);
            Vector3 step = Vector3.Scale(direction, scale);
            //choose the transformation based on the selected tool
            Action<Transform, Vector3> transform;
            switch (Tools.current)
            {
                case Tool.Rotate:
                    transform = rotateObject;
                    break;
                case Tool.Scale:
                    transform = scaleObject;
                    break;
                default:
                    transform = moveObject;
                    break;
            }
            //get the current scene selection and move them
            var selection = Selection.GetFiltered(typeof(GameObject), SelectionMode.Editable | SelectionMode.ExcludePrefab);
            //apply the transformation to every selected gameObject
            for (int i = 0; i < selection.Length; i++) transform((selection[i] as GameObject).transform, step);
            //only consume the event if there was at least one gameObject selected, otherwise the camera will move as usual :)
            if (selection.Length > 0) Event.current.Use();
        }

        private static Vector2 getCameraSize(Camera sceneCamera)
        {
            Vector3 topRightCorner = sceneCamera.ViewportToWorldPoint(new Vector2(1, 1));
            Vector3 bottomLeftCorner = sceneCamera.ViewportToWorldPoint(new Vector2(0, 0));
            return (topRightCorner - bottomLeftCorner);
        }

        private static void rotateObject(Transform t, Vector3 rotation)
        {
            //allow undo of the rotation
            Undo.RecordObject(t, "Rotation Step");
            if (rotation.y != 0)
            {
                //move the rotation to the nearest multiple of the snap angle.
                Vector3 currentRotation = t.rotation.eulerAngles;
                if (rotation.y > 0)
                {
                    currentRotation.z = (Mathf.RoundToInt(currentRotation.z) / rotationSnapAngle) * rotationSnapAngle;
                    currentRotation.z += rotationSnapAngle;
                }
                else
                {
                    int current = Mathf.RoundToInt(currentRotation.z);
                    if (current % rotationSnapAngle == 0) currentRotation.z -= rotationSnapAngle;
                    else currentRotation.z = (current / rotationSnapAngle) * rotationSnapAngle;
                }
                //set the new angle
                t.rotation = Quaternion.Euler(currentRotation);
            }
            else
            {
                t.Rotate(new Vector3(0, 0, -rotation.x));
            }
        }

        private static void scaleObject(Transform t, Vector3 scale)
        {
            //allow undo of the scale
            Undo.RecordObject(t, "Scale Step");
            t.localScale = t.localScale + scale;
        }

        private static void moveObject(Transform t, Vector3 movement)
        {
            //allow undo of the movements
            Undo.RecordObject(t, "Move Step");
            t.position = t.position + movement;
        }
    }
}
#endif
