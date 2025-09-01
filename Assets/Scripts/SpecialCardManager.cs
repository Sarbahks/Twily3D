using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpecialCardManager : MonoBehaviour
{
    private static SpecialCardManager instance;

    public static SpecialCardManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<SpecialCardManager>();
            }
            return instance;
        }
    }

    [SerializeField]
    private List<CardDocument> cardDocuments;

    public List<CardDocument> CardDocuments { get => cardDocuments; set => cardDocuments = value; }
    public List<ImageData> ImageDatas { get => imageDatas; set => imageDatas = value; }

    [SerializeField]
    private List<ImageData> imageDatas;


    public Sprite GetImageFromId(int id)
    {
        return ImageDatas.FirstOrDefault( x=> x.ImageId == id).SpriteImage;
    }

}

[SerializeField]
public class CardDocument
{
    [SerializeField]
    private int cardDocId = 0;
    [SerializeField]
    private GameObject document;

    public int CardDocId { get => cardDocId; set => cardDocId = value; }
    public GameObject Document { get => document; set => document = value; }
}

[SerializeField]
public class ImageData
{
    [SerializeField]
    private int imageId = 0;
    [SerializeField]
    private Sprite spriteImage;

    public int ImageId { get => imageId; set => imageId = value; }
    public Sprite SpriteImage { get => spriteImage; set => spriteImage = value; }
}

/*
 Documentation on effect card
1 = no writing directly on the card

 */