using System.Collections.Generic;
using UnityEngine;

public class ProfileCardArea : MonoBehaviour
{
    [SerializeField]
    private List<SelectableProfile> cards;

    public List<SelectableProfile> Cards { get => cards; set => cards = value; }
}
