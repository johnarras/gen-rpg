using Assets.Scripts.Atlas.Constants;
using Assets.Scripts.Crawler.Constants;
using Assets.Scripts.Crawler.CrawlerStates;
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

public class CrawlerStateData
{
    public CrawlerStateData(ECrawlerStates state, bool forceNextState = false)
    {
        Id = state;
        ForceNextState = forceNextState;
    }

    public string InputLabel { get; set; }
    public object ExtraData { get; set; }
    public Action<string> ValidateText { get; set; }
    public string InputPlaceholderText { get; set; }
    public ECrawlerStates Id { get; private set; }
    public PartyMember Member { get; set; } 
    public List<CrawlerStateAction> Actions = new List<CrawlerStateAction>();
    public List<String> LoreText = new List<string>();
    public string WorldSpriteName = CrawlerClientConstants.WorldDefaultSpriteName;
    public bool ForceNextState { get; set; } = false;
    public string ErrorMessage { get; set; }

    public GInputField InputField;

    public bool HasInput()
    {
        return !string.IsNullOrEmpty(InputLabel) && ValidateText != null;
    }


    public bool ShouldCheckInput()
    {
        return HasInput() && InputField != null;
    }

    public bool CheckInput()
    {
        if (ShouldCheckInput())
        {
            ValidateText(InputField.text);
            return true;
        }
        return false;
    }

    public void AddInputField (string prompt, Action<string> validationHandler)
    {
        if (!string.IsNullOrEmpty(prompt) && validationHandler != null)
        {
            InputLabel = prompt;
            ValidateText = validationHandler;
        }
    }
}