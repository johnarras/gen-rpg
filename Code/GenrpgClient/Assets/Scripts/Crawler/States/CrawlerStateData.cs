using Assets.Scripts.Atlas.Constants;
using Assets.Scripts.Crawler.Constants;
using Assets.Scripts.Crawler.CrawlerStates;
using Assets.Scripts.UI.Core;
using Assets.Scripts.UI.Crawler.States;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Unity.Profiling.LowLevel;
using UnityEngine;


public class CrawlerInputData
{
    public string InputLabel;
    public Action<string> ValidateTextAction;
    public LabeledInputField InputField;
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
                input.ValidateTextAction(input.InputField.Input.Text);
            }
        }
        return false;
    }

    public void AddInputField (string prompt, Action<string> validationHandler)
    {
        if (!string.IsNullOrEmpty(prompt) && validationHandler != null)
        {
            Inputs.Add(new CrawlerInputData() { InputLabel = prompt, ValidateTextAction = validationHandler });
        }
    }
}