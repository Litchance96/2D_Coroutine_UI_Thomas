//disable warning about read-only field
#pragma warning disable IDE0044


using System.Collections.Generic;
using System.Collections;
using UnityEngine;


public class LevelManager : MonoBehaviour
{
    [SerializeField][Range(2, 24)] private int cardNumber;
    [SerializeField] private int cardsByLine = 4;
    [SerializeField] private Vector2 offSet = Vector2.one * 0.2f;
    [SerializeField] private CardBehavior prefab;

    [SerializeField] private Sprite[] spriteFaceUp;

    public MouseManager MouseManager {get; private set;}

    private List<CardBehavior> cardBehaviors = new();

    private int cardFocusedId = -1;

    private int firstCardFaceUpId = -1;

    private int secondCardFaceUpId = -1;

    private int Pairs => faceIdAlreadyFaceUp.Count;

    private List<int> faceIdAlreadyFaceUp = new();
    [SerializeField] private float timeBeforeFaceDown;

    [SerializeField] private GameObject[] gameObjectActivateOnVictory;
    private float timeBeforeVictoryDisplay =1f;
    

    private CardBehavior CardFocused => cardFocusedId >= 0 ? cardBehaviors[cardFocusedId] : null;
    private CardBehavior FirstCardFaceUp => firstCardFaceUpId >= 0 ? cardBehaviors[firstCardFaceUpId] : null;

    private CardBehavior SecondCardFaceUp => secondCardFaceUpId >= 0 ? cardBehaviors[secondCardFaceUpId] : null;

    private void Start()
    {
        if (cardNumber % 2 != 0)
        {
            cardNumber++;
            Debug.LogWarning($"Card Number need to be an even number then it's became {cardNumber}.");

        }

        foreach(GameObject go in gameObjectActivateOnVictory)
        {
            go.SetActive(false);
        }

        //InstantiateCards();
    }
    public void SetCardNumber(int n)
    {
        cardNumber = n;

        if (cardNumber % 2 != 0)
        {
            cardNumber++;
            Debug.LogWarning($"Card Number need to be an even number then it's became {cardNumber}.");
        }
    }
    public void InstantiateCards()
    {

        List<int> facePoolIndex = new();

        for (int index =0; index < cardNumber/2; index++)
        {
            facePoolIndex.Add(index);
            facePoolIndex.Add(index);
        }
        ;


        BoxCollider2D collider = prefab.GetComponentInChildren<BoxCollider2D>();
        float width = collider.size.x;
        float height = collider.size.y;

        int lines = cardNumber / cardsByLine;
        if (cardNumber % cardsByLine != 0) lines++;

        float px = (cardsByLine * (width + offSet.x) - offSet.x) / -2f;
        float py = (lines * (height + offSet.y) - offSet.y) / -2f;
        Vector2 origin = new(px, py);

        for (int y = 0; y < lines; y++)
        {
            for (int x = 0; x < cardsByLine; x++)
            {
                Vector2 position = origin + y * (height + offSet.y) * Vector2.up;
                position += x * (width + offSet.x) * Vector2.right;
                CardBehavior cardBehavior = Instantiate(prefab, position, Quaternion.identity);

                int rndIndex = Random.Range(0, facePoolIndex.Count);
                int faceIndex = facePoolIndex[rndIndex];
                cardBehavior.SetFace(spriteFaceUp[faceIndex], faceIndex);
                facePoolIndex.RemoveAt(rndIndex);


                cardBehavior.ConnectToManager(this, cardBehaviors.Count);

                cardBehaviors.Add(cardBehavior);
                if (cardBehaviors.Count >= cardNumber) return;
            }
        }
    }

    public void ConnectMouseManager(MouseManager mouseManager)
    {
        MouseManager = mouseManager;
    }
    public void MouseOnCard(CardBehavior cardBehavior)
    {
        if (cardBehavior == null)
        {
            if (CardFocused != null)
            {
                CardFocused.UnFocus();
                cardFocusedId = -1;
            }

        }
        else if (cardBehavior.Id != cardFocusedId && cardBehavior.Id != firstCardFaceUpId)
        {
            if (CardFocused != null)
            {
                CardFocused.UnFocus();
            }
            if(!faceIdAlreadyFaceUp.Contains(cardBehavior.FaceId))
            {
                cardFocusedId = cardBehavior.Id;
                cardBehavior.Focus();
            }

        }
    }
    public void MouseClick()
    {
        if(CardFocused != null) 
        {

            CardFocused.TurnFaceUp();
            if(FirstCardFaceUp == null)
            {
                
                firstCardFaceUpId = cardFocusedId;
                
            }
            else
            {
                secondCardFaceUpId = cardFocusedId;
                StartCoroutine(CheckResult());

            }

            CardFocused.UnFocus();
            cardFocusedId = -1;

            
        }

    }

    public void RestartDisplay()
    {
        List<int> facePoolIndex = new();

        for (int n =0; n < cardNumber/2; n++)
        {
            int index = Random.Range(0, spriteFaceUp.Length);
            while (facePoolIndex.Contains(index))
                index = Random.Range(0, spriteFaceUp.Length);

            facePoolIndex.Add(index);
            facePoolIndex.Add(index);
        }
        
        foreach(CardBehavior cardBehavior in cardBehaviors )
        {
                int rndIndex = Random.Range(0, facePoolIndex.Count);
                int faceIndex = facePoolIndex[rndIndex];
                cardBehavior.SetFace(spriteFaceUp[faceIndex], faceIndex);
                facePoolIndex.RemoveAt(rndIndex);
        }
        
        foreach(GameObject go in gameObjectActivateOnVictory)
        {
            go.SetActive(false);
        }

        faceIdAlreadyFaceUp.Clear();
    }

    private IEnumerator VictoryDisplay()
    {
        yield return new WaitForSeconds(timeBeforeVictoryDisplay);

        foreach (CardBehavior cardBehavior in cardBehaviors)

        cardBehavior.TurnFaceDown();

        foreach(GameObject go in gameObjectActivateOnVictory)
        {
            go.SetActive(true);
        }
    }
    private IEnumerator CheckResult()
    {
        MouseManager.enabled = false;

        yield return new WaitForSeconds(timeBeforeFaceDown);

        if(FirstCardFaceUp.FaceId != SecondCardFaceUp.FaceId)
        {
            FirstCardFaceUp.TurnFaceDown();
            SecondCardFaceUp.TurnFaceDown();
        }
        else
        {
            faceIdAlreadyFaceUp.Add(FirstCardFaceUp.FaceId);
            Debug.Log($"Nombre de paires trouvées : {Pairs}");
            if(Pairs >= cardNumber/2)
            {
                StartCoroutine(VictoryDisplay());//On est dans une co-routine et on relance une autre co-routine (attention, avoir bonne maitrise de ce qu'on fait)
                
            }

        
        }

        firstCardFaceUpId = -1;
        secondCardFaceUpId = -1;


        MouseManager.enabled = true;
    }
}







    // public void Instantiate6Cards()
    // {
    //     List<int> facePoolIndex = new();

    //     cardNumber = 6;

    //     for (int index =0; index < cardNumber/2; index++)
    //     {
    //         facePoolIndex.Add(index);
    //         facePoolIndex.Add(index);
    //     }
    //     ;


    //     BoxCollider2D collider = prefab.GetComponentInChildren<BoxCollider2D>();
    //     float width = collider.size.x;
    //     float height = collider.size.y;

    //     int lines = cardNumber / cardsByLine;
    //     if (cardNumber % cardsByLine != 0) lines++;

    //     float px = (cardsByLine * (width + offSet.x) - offSet.x) / -2f;
    //     float py = (lines * (height + offSet.y) - offSet.y) / -2f;
    //     Vector2 origin = new(px, py);

    //     for (int y = 0; y < lines; y++)
    //     {
    //         for (int x = 0; x < cardsByLine; x++)
    //         {
    //             Vector2 position = origin + y * (height + offSet.y) * Vector2.up;
    //             position += x * (width + offSet.x) * Vector2.right;
    //             CardBehavior cardBehavior = Instantiate(prefab, position, Quaternion.identity);

    //             int rndIndex = Random.Range(0, facePoolIndex.Count);
    //             int faceIndex = facePoolIndex[rndIndex];
    //             cardBehavior.SetFace(spriteFaceUp[faceIndex], faceIndex);
    //             facePoolIndex.RemoveAt(rndIndex);


    //             cardBehavior.ConnectToManager(this, cardBehaviors.Count);

    //             cardBehaviors.Add(cardBehavior);
    //             if (cardBehaviors.Count >= cardNumber) return;
    //         }
    //     }
    // }

    // public void Instantiate12Cards()
    // {
    //     List<int> facePoolIndex = new();

    //     cardNumber = 12;

    //     for (int index =0; index < cardNumber/2; index++)
    //     {
    //         facePoolIndex.Add(index);
    //         facePoolIndex.Add(index);
    //     }
    //     ;


    //     BoxCollider2D collider = prefab.GetComponentInChildren<BoxCollider2D>();
    //     float width = collider.size.x;
    //     float height = collider.size.y;

    //     int lines = cardNumber / cardsByLine;
    //     if (cardNumber % cardsByLine != 0) lines++;

    //     float px = (cardsByLine * (width + offSet.x) - offSet.x) / -2f;
    //     float py = (lines * (height + offSet.y) - offSet.y) / -2f;
    //     Vector2 origin = new(px, py);

    //     for (int y = 0; y < lines; y++)
    //     {
    //         for (int x = 0; x < cardsByLine; x++)
    //         {
    //             Vector2 position = origin + y * (height + offSet.y) * Vector2.up;
    //             position += x * (width + offSet.x) * Vector2.right;
    //             CardBehavior cardBehavior = Instantiate(prefab, position, Quaternion.identity);

    //             int rndIndex = Random.Range(0, facePoolIndex.Count);
    //             int faceIndex = facePoolIndex[rndIndex];
    //             cardBehavior.SetFace(spriteFaceUp[faceIndex], faceIndex);
    //             facePoolIndex.RemoveAt(rndIndex);


    //             cardBehavior.ConnectToManager(this, cardBehaviors.Count);

    //             cardBehaviors.Add(cardBehavior);
    //             if (cardBehaviors.Count >= cardNumber) return;
    //         }
    //     }
    // }

    // public void Instantiate24Cards()
    // {
    //     List<int> facePoolIndex = new();

    //     cardNumber = 24;

    //     for (int index =0; index < cardNumber/2; index++)
    //     {
    //         facePoolIndex.Add(index);
    //         facePoolIndex.Add(index);
    //     }
    //     ;


    //     BoxCollider2D collider = prefab.GetComponentInChildren<BoxCollider2D>();
    //     float width = collider.size.x;
    //     float height = collider.size.y;

    //     int lines = cardNumber / cardsByLine;
    //     if (cardNumber % cardsByLine != 0) lines++;

    //     float px = (cardsByLine * (width + offSet.x) - offSet.x) / -2f;
    //     float py = (lines * (height + offSet.y) - offSet.y) / -2f;
    //     Vector2 origin = new(px, py);

    //     for (int y = 0; y < lines; y++)
    //     {
    //         for (int x = 0; x < cardsByLine; x++)
    //         {
    //             Vector2 position = origin + y * (height + offSet.y) * Vector2.up;
    //             position += x * (width + offSet.x) * Vector2.right;
    //             CardBehavior cardBehavior = Instantiate(prefab, position, Quaternion.identity);

    //             int rndIndex = Random.Range(0, facePoolIndex.Count);
    //             int faceIndex = facePoolIndex[rndIndex];
    //             cardBehavior.SetFace(spriteFaceUp[faceIndex], faceIndex);
    //             facePoolIndex.RemoveAt(rndIndex);


    //             cardBehavior.ConnectToManager(this, cardBehaviors.Count);

    //             cardBehaviors.Add(cardBehavior);
    //             if (cardBehaviors.Count >= cardNumber) return;
    //         }
    //     }
    // }
