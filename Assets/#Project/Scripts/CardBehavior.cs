using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CardBehavior : MonoBehaviour
{
    private const string FACE_UP_ANIMATION = "IsFaceUp";
    [SerializeField] private Sprite spriteFaceDown;
    [SerializeField] private Sprite spriteFaceUp;

    // do no have the warning about mask effect.
#pragma warning disable CS0108
    [SerializeField] private SpriteRenderer renderer;

    public bool IsFaceUp { get; private set; } = false;

    private LevelManager manager = null;
    private int id;


#pragma warning restore CS0108

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

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

    private void Turn(bool isFaceUp)
    {
        animator.SetBool(FACE_UP_ANIMATION, isFaceUp);
        IsFaceUp = isFaceUp;
    }

    public void TurnFaceUp() => Turn(true);
    public void TurnFaceDown() => Turn(false);

    private void FaceUp()
    {
        renderer.sprite = spriteFaceUp;
    }

    private void FaceDown()
    {
        renderer.sprite = spriteFaceDown;
    }

    public void ConnectToManager(LevelManager manager, int id)
    {
        this.manager = manager;
        this.id = id;
    }
}
