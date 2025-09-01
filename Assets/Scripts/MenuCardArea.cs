using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // for LayoutRebuilder

public class MenuCardArea : MonoBehaviour
{
    [Header("Layout")]
    [SerializeField] private int numberCardByLine = 6;

    [Tooltip("Prefab for one horizontal row (must have a HorizontalLayoutGroup).")]
    [SerializeField] private GameObject prefabLineCard;

    [Tooltip("Content object of the ScrollView (should have a VerticalLayoutGroup).")]
    [SerializeField] private RectTransform contentScrollView;

    [SerializeField]
    private List<RowCardState> rowStates;

    /// <summary>
    /// Instantiates a card into the last row, creating a new row if needed.
    /// Returns the instantiated card GameObject.
    /// </summary>
    public GameObject AddCardToArea(GameObject cardPrefab)
    {
        if (cardPrefab == null)
        {
            Debug.LogError("[MenuCardArea] AddCardToArea: cardPrefab is null.");
            return null;
        }
        if (prefabLineCard == null || contentScrollView == null)
        {
            Debug.LogError("[MenuCardArea] Missing prefabLineCard or contentScrollView.");
            return null;
        }

        // Ensure list is initialized
        if (rowStates == null) rowStates = new List<RowCardState>();

        // Find a row with space
        RowCardState targetRow = null;
        foreach (var row in rowStates)
        {
            if (!row.gameObject.activeSelf) continue; // skip inactive rows
            if (row.CardInstancied.Count < numberCardByLine)
            {
                targetRow = row;
                break;
            }
        }

        // If no active row with space, try to activate an inactive one
        if (targetRow == null)
        {
            foreach (var row in rowStates)
            {
                if (!row.gameObject.activeSelf)
                {
                    row.gameObject.SetActive(true);
                    targetRow = row;
                    break;
                }
            }
        }

        // Safety: if still no row, we have no spare rows
        if (targetRow == null)
        {
            Debug.LogWarning("[MenuCardArea] No available row to add card.");
            return null;
        }

        // Instantiate the card and parent it to the row
        GameObject card = Instantiate(cardPrefab, targetRow.transform, false);
        targetRow.CardInstancied.Add(card);

        return card;
    }

    public void DeleteAllCardsInArea()
    {
        if (rowStates == null) return;

        foreach (var row in rowStates)
        {
            // Destroy all cards in this row
            for (int i = row.CardInstancied.Count - 1; i >= 0; i--)
            {
                Destroy(row.CardInstancied[i]);
            }

            // Clear the list of references
            row.CardInstancied.Clear();

            // Deactivate the row
            row.gameObject.SetActive(false);
        }
    }



    private void Start()
    {
        InitializeLineCard();
    }

    private void InitializeLineCard()
    {
        rowStates.Clear();
        int id = 0;
        foreach (Transform child in contentScrollView)
        {
            RowCardState rcs = child.GetComponent<RowCardState>();
            if (rcs != null)
            {
                rowStates.Add(rcs);
                rcs.IdRow = id;
                id++;
            }
            rcs.gameObject.SetActive(false);
        }
    }

}


