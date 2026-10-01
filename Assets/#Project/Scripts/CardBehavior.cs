
using UnityEngine;

public class CardBehavior : MonoBehaviour
{
    [SerializeField] private Sprite spriteFaceDown;
    [SerializeField] private Sprite spriteFaceUp;

    // do no have the warning about mask effect.
#pragma warning disable CS0108
    [SerializeField] private SpriteRenderer renderer;
#pragma warning restore CS0108

    void Start()
    {
        if (renderer == null)
        {
            renderer = GetComponentInChildren<SpriteRenderer>();
            if (renderer == null)
            {
                Debug.LogError("CardBehavior needs a SpriteRender to works.");
            }
        }

        if (spriteFaceDown == null)
        {
            spriteFaceDown = renderer.sprite;
        }
        else
        {
            renderer.sprite = spriteFaceDown;
        }
    }

    public void FaceUp()
    {
        renderer.sprite = spriteFaceUp;
    }
}
