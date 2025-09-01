using TMPro;
using UnityEngine;

public class GameInSalonItem : MonoBehaviour
{
    public TextMeshProUGUI nameGame;

    public void Setup(SalonInfo salon)
    {
        nameGame.text = salon.Name;
    }
}
