using System.Collections.Generic;
using UnityEngine;

public class RowCardState : MonoBehaviour
{
    [SerializeField] private int idRow;
    [SerializeField] private List<GameObject> cardInstancied;


    public int IdRow { get => idRow; set => idRow = value; }
    public List<GameObject> CardInstancied { get => cardInstancied; set => cardInstancied = value; }

}
