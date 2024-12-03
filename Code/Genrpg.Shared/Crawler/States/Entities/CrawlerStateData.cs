using MessagePack;
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

    [MessagePackObject]
public class CrawlerStateData
{
    public CrawlerStateData(ECrawlerStates state, bool forceNextState = false)
    {
        Id = state;
        ForceNextState = forceNextState;
    }

    [Key(0)] public List<CrawlerInputData> Inputs { get; set; } = new List<CrawlerInputData>();
    [Key(1)] public object ExtraData { get; set; }
    [Key(2)] public string InputPlaceholderText { get; set; }
    public ECrawlerStates Id { get; private set; }
    [Key(3)] public PartyMember Member { get; set; }
    public List<CrawlerStateAction> Actions = new List<CrawlerStateAction>();
    public List<String> LoreText = new List<string>();
    public string WorldSpriteName = CrawlerClientConstants.DefaultWorldBG;
    [Key(4)] public bool ForceNextState { get; set; } = false;
    [Key(5)] public bool DoNotTransitionToThisState { get; set; }
    [Key(6)] public bool UseSmallerButtons { get; set; } = false;

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
                input.ValidateTextAction(input.InputField.GetInputText());
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
