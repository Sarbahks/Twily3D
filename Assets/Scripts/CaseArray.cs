using System;
using TMPro;
using UnityEngine;

public class CaseArray : MonoBehaviour
{
    public RoleGameType Role;
    public BudgetType Budget;
    public int BudgetValue = -1;

    [SerializeField]
    private TMP_InputField textValue;

    public void SetupCase()
    {
        if(BudgetValue > -1)
        {
            textValue.text = Budget.ToString();
        }
    }

    public int GetValue()
    {
        // If the field is null or empty  return -1
        if (textValue == null || string.IsNullOrWhiteSpace(textValue.text))
            return -1;

        // Try parsing the input to int
        if (int.TryParse(textValue.text, out int result))
        {
            return result;
        }

        // If parsing fails  return -1
        return -1;
    }
}
