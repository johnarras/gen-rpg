using Genrpg.Shared.Crawler.Info.Services;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Info.UI
{
    public class InfoPanel : BaseBehaviour
    {
        private IInfoService _infoService;

        public GameObject InfoAnchor;
        public InfoPanelRow InfoText;

        private Stack<List<string>> _infoStack = new Stack<List<string>>();
        private List<string> _currentInfo = null;

        public void InitData()
        {
            _clientEntityService.SetActive(InfoText.gameObject, false);
        }


        public void ClearInfo()
        {
            _clientEntityService.DestroyAllChildren(InfoAnchor);
        }

        public void ShowLines(List<string> lines)
        {
            if (lines.Count < 1)
            {
                return;
            }

            ClearInfo();

            if (_currentInfo != null && _currentInfo.Count > 0)
            {
                _infoStack.Push(_currentInfo);
            }
            _currentInfo = lines;

            foreach (string line in lines)
            {
                InfoPanelRow listItem = _clientEntityService.FullInstantiate<InfoPanelRow>(InfoText);

                _clientEntityService.AddToParent(listItem, InfoAnchor);

                listItem.InitData(this, line);
            }
        }

        public void PopInfoStack()
        {
            if (_infoStack.TryPop(out List<string> currList))
            {
                _currentInfo = null;
                ShowLines(currList);
            }
            else
            {
                ClearInfo();
            }

        }

        public void ShowInfo(long entityTypeId, long entityId)
        {
            List<string> lines = _infoService.GetInfoLines(entityTypeId, entityId);
            ShowLines(lines);
        }

        public void ClearStack()
        {
            _infoStack.Clear();
        }

    }
}
