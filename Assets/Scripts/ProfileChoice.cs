using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProfileChoice : MonoBehaviour
{
    private static ProfileChoice instance;

    public static ProfileChoice Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<ProfileChoice>();
            }
            return instance;
        }
    }

    [SerializeField] private CardData choice1;
    [SerializeField] private CardData choice2;

    [SerializeField] private CardProfile cardProfile1;
    [SerializeField] private BackCardProfile backCardProfile1;

    [SerializeField] private CardProfile cardProfile2;
    [SerializeField] private BackCardProfile backCardProfile2;

    private readonly List<Tuple<CardData, CardData>> _pairs = new List<Tuple<CardData, CardData>>();
    private int _pairIndex = 0;

    [SerializeField] private List<CardData> chosenCards;

    private const int MaxChosen = 5;

    private void Awake()
    {
        // Ensure list is not null if not set in inspector
        if (chosenCards == null)
            chosenCards = new List<CardData>();

        SetupProfileChoice();
    }

    [SerializeField]
    private bool test;

    private void Update()
    {
        if(test)
        {
            test = false;
            SetupProfileChoice();
        }
    }


    public void SetupProfileChoice()
    {
        var cards = LobbySceneManager.Instance.CurrentGameState.Board;
        _pairs.Clear();
        _pairIndex = 0;
        chosenCards.Clear();

        if (cards == null || cards.Count == 0)
        {
            ClearChoices();
            return;
        }

        var profileCards = cards.Where(x => x.TypeCard == TypeCard.PROFILE && x.IdArea == 2).ToList();

        if (profileCards.Count < 2)
        {
            ClearChoices();
            return;
        }

        // Pair in order. If odd, last is dropped.
        for (int i = 0; i + 1 < profileCards.Count; i += 2)
        {
            _pairs.Add(Tuple.Create(profileCards[i], profileCards[i + 1]));
        }

        SetupChoice();
    }

    public void SetupChoice()
    {
        if (_pairs.Count == 0)
        {
            ClearChoices();
            return;
        }

        if (_pairIndex < 0 || _pairIndex >= _pairs.Count)
            _pairIndex = 0;

        var pair = _pairs[_pairIndex];
        choice1 = pair.Item1;
        choice2 = pair.Item2;

        RefreshChoiceUI(choice1, choice2);
    }

    private void RefreshChoiceUI(CardData c1, CardData c2)
    {
        if (c1 != null && c2 != null)
        {
            cardProfile1.SetupCard(c1);
            backCardProfile1.SetupBackCard(c1);

            cardProfile2.SetupCard(c2);
            backCardProfile2.SetupBackCard(c2);
        }
        else
        {
            // Optionally clear UI here if needed
            // cardProfile1.Clear(); backCardProfile1.Clear(); etc.
        }
    }

    public void NextChoice()
    {
        if (_pairs.Count == 0)
        {
            ClearChoices();
            return;
        }

        _pairIndex = (_pairIndex + 1) % _pairs.Count;
        SetupChoice();
    }

    public void PreviousChoice()
    {
        if (_pairs.Count == 0)
        {
            ClearChoices();
            return;
        }

        _pairIndex = (_pairIndex - 1 + _pairs.Count) % _pairs.Count;
        SetupChoice();
    }

    public Tuple<CardData, CardData> GetCurrentPair()
    {
        if (_pairs.Count == 0) return null;
        return _pairs[_pairIndex];
    }

    private void ClearChoices()
    {
        choice1 = null;
        choice2 = null;
        RefreshChoiceUI(null, null);
    }

    public void MakeChoice1()
    {
        Choose(choice1);
    }

    public void MakeChoice2()
    {
        Choose(choice2);
    }

    // Core selection logic shared by both buttons
    private void Choose(CardData selected)
    {
        if (selected == null) return;

        // Prevent overflow past MaxChosen
        if (chosenCards.Count >= MaxChosen)
        {
            EndSelection();
            return;
        }

        chosenCards.Add(selected);

        // Consume the current pair
        if (_pairs.Count > 0)
        {
            _pairs.RemoveAt(_pairIndex);
            if (_pairIndex >= _pairs.Count)
                _pairIndex = 0;
        }

        // End if we reached MaxChosen or if no pairs remain
        if (chosenCards.Count >= MaxChosen || _pairs.Count == 0)
        {
            EndSelection();
            return;
        }

        // Otherwise, show the next pair
        SetupChoice();
    }
    [SerializeField]
    private TwilyButton chose1Btn;
    [SerializeField]
    private TwilyButton chose2Btn;
    [SerializeField]
    private TwilyButton nextBtn;
    private void EndSelection()
    {
        Debug.Log($"Selection complete. {chosenCards.Count} cards chosen.");
        // Lock UI if desired
        ClearChoices();

        // TODO: trigger your next step, e.g. send chosenCards somewhere:
        // OnProfileSelectionComplete?.Invoke(chosenCards);
        // Or LobbySceneManager.Instance.SubmitProfileChoices(chosenCards);
        WsClient.Instance.SelectTeamPlayer(chosenCards);

        chose1Btn.gameObject.SetActive(false);
        chose2Btn.gameObject.SetActive(false);
        nextBtn.gameObject.SetActive(false);
    }
}
