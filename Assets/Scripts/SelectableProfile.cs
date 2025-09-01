using UnityEngine;

public class SelectableProfile : MonoBehaviour
{
    [SerializeField]
    private CardData card;

    public CardData Card { get => card; set => card = value; }


    public void SelectProfileCard()
    {
        Debug.Log("select profile");
    }

}
