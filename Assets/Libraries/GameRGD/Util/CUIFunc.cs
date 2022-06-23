using System;
using System.Dynamic;
using System.Runtime.CompilerServices;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace DoubleuGames.GameRGD
{
    public static class CUIFunc
    {
        public static async UniTaskVoid SetTextureToRawImage(RawImage image, string url)
        {
            if (image != null) image.texture = await CMiscFunc.LoadImageTextureAsync(url);
        }

        public static void SetText(Text text, string value)
        {
            if (text != null) text.text = value;
        }

        public static void SetText(Text text, int value, TextOptions option = TextOptions.Number)
        {
            var _str = $"{value}";
            switch (option)
            {
                case TextOptions.Number:
                    _str = CMiscFunc.ChangeNumberToString(value);
                    break;
                case TextOptions.NumberWithSign:
                    _str = (value > 0 ? "+" : "") + CMiscFunc.ChangeNumberToString(value);
                    break;
                case TextOptions.TimeWithColon:
                    _str = CMiscFunc.GetRemainTimeFormat(value);
                    break;
                case TextOptions.TimeWithString:
                    _str = CMiscFunc.SetTimeString(value);
                    break;
                case TextOptions.TimeWithMinutesString:
                    _str = CMiscFunc.SetTimeMinuteString(value);
                    break;
                case TextOptions.KMB:
                    _str = CMiscFunc.ChangeNumberToString_KMB(value);
                    break;
                case TextOptions.KMB_v2:
                    _str = CMiscFunc.ChangeNumberToString_KMB_v2(value);
                    break;
                case TextOptions.KMB_v3:
                    _str = CMiscFunc.ChangeNumberToString_KMB_v3(value);
                    break;
            }
            if (text != null) text.text = _str;
        }

        public static void SetText(Text text, long value, TextOptions option = TextOptions.Number)
        {
            var _str = $"{value}";
            switch (option)
            {
                case TextOptions.Number:
                    _str = CMiscFunc.ChangeNumberToString(value);
                    break;
                case TextOptions.TimeWithColon:
                    _str = CMiscFunc.GetRemainTimeFormat(value);
                    break;
                case TextOptions.KMB:
                    _str = CMiscFunc.ChangeNumberToString_KMB(value);
                    break;
                case TextOptions.KMB_v2:
                    _str = CMiscFunc.ChangeNumberToString_KMB_v2(value);
                    break;
                case TextOptions.KMB_v3:
                    _str = CMiscFunc.ChangeNumberToString_KMB_v3(value);
                    break;
            }
            if (text != null) text.text = _str;
        }

        public static void SetValue(Slider slider, float value)
        {
            if (slider != null) slider.value = value;
        }

        public static void SetActiveByActiveSelf(GameObject go, bool value)
        {
            if (go != null) go.SetActiveByActiveSelf(value);
        }

        public static void SetActiveByActiveSelf(Component component, bool value)
        {
            if (component != null) SetActiveByActiveSelf(component.gameObject, value);
        }

        public static void SetColor(Text text, Color color)
        {
            if (text != null) text.color = color;
        }

        public static void SetColor(Image image, Color color)
        {
            if (image != null) image.color = color;
        }

        public static void SetTexture(Image image, Sprite sprite)
        {
            if (image != null) image.sprite = sprite;
        }

        public static void SetTexture(Image image, SpriteAtlas atlas, string name)
        {
            if (atlas != null) SetTexture(image, atlas.GetSprite(name));
        }

        public static void SetFillAmount(Image image, float value)
        {
            if (image != null) image.fillAmount = value;
        }

        public static void SetInteractable(Selectable selectable, bool value)
        {
            if (selectable != null)
            {
                if (selectable.GetType() == typeof(Button))
                {
                    var _button = selectable as Button;
                    _button.InteractableWithText(value);
                }
                else selectable.interactable = value;
            }
        }

        public static GameObject CreateImageGameObject(string gameObjectName, SpriteAtlas atlas, string spriteName, Transform parent = default)
        {
            var _go = new GameObject(gameObjectName);
            var _image = _go.GetOrAddComponent<Image>();

            if (atlas != null)
            {
                SetTexture(_image, atlas.GetSprite(spriteName));
                _image.SetNativeSize();
            }

            if (parent != default) _go.transform.SetParent(parent, false);
            return _go;
        }

        public static void InteractableWithText(this Selectable button, bool value)
        {
            if (button.transition == Selectable.Transition.ColorTint)
            {
                var _text = button.transform.GetComponentInChildren<Text>();
                if (_text != default && button.interactable != value)
                {
                    _text.color = value ? _text.color * 2 : _text.color / 2;
                }
            }
            button.interactable = value;
        }

        public static void AddDebugText(this GameObject gameObject, string debug, float posY = 0f, int fontSize = 30)
        {
            var _trasform = gameObject.transform.Find("TXT_Debug");
            var _textObj = null as GameObject;

            if (_trasform == default)
            {
                _textObj = new GameObject();
                _textObj.name = "TXT_Debug";
                _textObj.transform.SetParent(gameObject.transform);

                var _rect = _textObj.AddComponent<RectTransform>();
                _rect.anchorMin = new Vector2(0f, 0.5f);
                _rect.anchorMax = new Vector2(1f, 0.5f);
                _rect.localPosition = new Vector3(0f, posY, 0f);

                // var _rectPos = _rect.localPosition;
                // _rectPos.y = posY;
                // _rect.position = _rectPos;
                var _rectSize = _rect.sizeDelta;
                _rectSize.y = fontSize * 1.3f;
                _rect.sizeDelta = _rectSize;
                _rect.localScale = Vector3.one;

                var _text = _textObj.AddComponent<Text>();
                _text.font = Resources.Load<Font>("Common/Fonts/XBall");
                _text.fontSize = fontSize;
                _text.text = debug;
                _text.color = new Color(1f, 1f, 1f, 0.5f);
                _text.alignment = TextAnchor.MiddleCenter;
                _text.resizeTextForBestFit = true;
                _text.resizeTextMaxSize = fontSize;
                _text.raycastTarget = false;

                var _outline = _textObj.AddComponent<Outline>();
                _outline.effectColor = Color.black;
                _outline.effectDistance = new Vector2(2f, -2f);
            }
            else
            {
                _textObj = _trasform.gameObject;
                var _text = _textObj.GetOrAddComponent<Text>();
                _text.text = debug;
            }
        }
    }

    public enum TextOptions
    {
        Default = 0,
        Number = 1,
        NumberWithSign = 2,
        TimeWithColon = 3,
        TimeWithString = 4,
        TimeWithMinutesString = 5,
        KMB = 6,
        KMB_v2 = 7,
        KMB_v3 = 8,
    }

    public class ButtonEnabledScope : IDisposable
    {
        private Button m_Button;

        public ButtonEnabledScope(Button button)
        {
            if (button == null) throw new ArgumentNullException(nameof(button));

            m_Button = button;
            m_Button.enabled = false;
        }

        public void Dispose()
        {
            m_Button.enabled = true;
        }
    }
}
