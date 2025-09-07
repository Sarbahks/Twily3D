using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpecialInGameManager : MonoBehaviour
{
    private static SpecialInGameManager instance;

    public static SpecialInGameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<SpecialInGameManager>();
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
        var img = ImageDatas.FirstOrDefault(x => x.ImageId == id);
           if (img != null)
        {
            return img.SpriteImage;
        }
        return null;

    }

    public void OpenCardLinkedDoc(int attachedDocupentId)
    {
        var docCorresponding = CardDocuments.FirstOrDefault(x => x.CardDocId == attachedDocupentId);

        if (docCorresponding != null)
        {
            
                docCorresponding.Document.SetActive(true);
            
        }
    }
}

[Serializable]
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

[Serializable]
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
2  =write on the dashboard
 */