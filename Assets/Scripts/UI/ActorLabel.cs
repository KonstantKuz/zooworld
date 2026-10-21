using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public sealed class ActorLabel : MonoBehaviour
    {
        [SerializeField] private float duration = 1f;
        [SerializeField] private Vector3 worldOffset = new(0f, 0f, -1.2f);

        private Text text;

        public float Duration => duration;
        public Vector3 WorldOffset => worldOffset;

        public void SetText(string message, Color color)
        {
            text ??= GetComponent<Text>();
            text.text = message;
            text.color = color;
        }

        public void SetScreenPosition(Vector2 screenPoint)
        {
            RectTransform parent = (RectTransform)transform.parent;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPoint, null, out Vector2 local))
            {
                ((RectTransform)transform).anchoredPosition = local;
            }
        }
    }
}
