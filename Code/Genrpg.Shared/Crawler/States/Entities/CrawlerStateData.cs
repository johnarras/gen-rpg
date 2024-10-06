using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.UI.Interfaces;
using System;
using System.Collections.Generic;



public class CrawlerInputData
{
    public string InputLabel;
    public Action<string> ValidateTextAction;
    public ILabeledInputField InputField;
}

public class CrawlerStateData
{
    public CrawlerStateData(ECrawlerStates state, bool forceNextState = false)
    {
        Id = state;
        ForceNextState = forceNextState;
    }

    public List<CrawlerInputData> Inputs { get; set; } = new List<CrawlerInputData>();
    public object ExtraData { get; set; }
    public string InputPlaceholderText { get; set; }
    public ECrawlerStates Id { get; private set; }
    public PartyMember Member { get; set; }
    public List<CrawlerStateAction> Actions = new List<CrawlerStateAction>();
    public List<String> LoreText = new List<string>();
    public string WorldSpriteName = CrawlerClientConstants.WorldImage;
    public bool ForceNextState { get; set; } = false;
    public bool DoNotTransitionToThisState { get; set; }
    public bool UseSmallerButtons { get; set; } = false;

    public bool HasInput()
    {
        return Inputs.Count > 0;
    }


    public bool ShouldCheckInput()
    {
        return HasInput();
    }

    public bool CheckInput()
    {
        if (ShouldCheckInput())
        {

            foreach (CrawlerInputData input in Inputs)
            {
                input.ValidateTextAction(input.InputField.GetText());
            }
        }
        return false;
    }

    public void AddInputField(string prompt, Action<string> validationHandler)
    {
        if (!string.IsNullOrEmpty(prompt) && validationHandler != null)
        {
            Inputs.Add(new CrawlerInputData() { InputLabel = prompt, ValidateTextAction = validationHandler });
        }
    }
}