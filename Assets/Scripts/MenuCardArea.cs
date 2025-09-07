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
    public GameObject AddCardToArea(GameObject cardPrefab)//those card have a BoardCaseUI, i want to get it;
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

    public List<BoardCaseUI> GetAllCardsInArea()
    {
        var result = new List<BoardCaseUI>();
        if (rowStates == null || rowStates.Count == 0) return result;

        var seen = new HashSet<BoardCaseUI>();

        foreach (var row in rowStates)
        {
            if (row == null) continue;

            // Prefer the explicit list of instantiated cards
            if (row.CardInstancied != null && row.CardInstancied.Count > 0)
            {
                foreach (var go in row.CardInstancied)
                {
                    if (go == null) continue;
                    var ui = go.GetComponent<BoardCaseUI>();
                    if (ui != null && seen.Add(ui))
                        result.Add(ui);
                    else if (ui == null)
                        Debug.LogWarning("[MenuCardArea] A card without BoardCaseUI was found in CardInstancied.");
                }
            }
            else
            {
                // Fallback: scan children (includeInactive: true to catch hidden rows/cards)
                var uis = row.GetComponentsInChildren<BoardCaseUI>(true);
                foreach (var ui in uis)
                {
                    if (ui != null && seen.Add(ui))
                        result.Add(ui);
                }
            }
        }

        return result;
    }

}


