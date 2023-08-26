using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class for making vertical collapsable menus.
/// 
/// To Use:
/// 
/// 1. Place this on a new ScrollView root object.
/// 
/// 2. Place a Vertical Layout Group on the Content for this main scroll view. Set ChildForceExpand.Height = false
/// 
/// 3. Then inside of that main Content, place pairs of Buttons and ScrollViews for each section you want to expand.
/// 
/// 4. Place a CollapsableScrollView on the inner scroll view. Make sure that there's a button inside of that inner content.
/// 
/// 5. Put ContentSizeFitters on anything in those child groups that you can't calculate the exact size for.
/// 
/// 6. Set those child objects (except for buttons) to RaycastTarget = false....so you can hold and scroll
/// 
/// 
/// </summary>
public class CollapsingScrollViewGroup : BaseBehaviour
{

    protected List<CollapsableScrollView> _scrollViews = null;


    protected ScrollRect _mainScrollRect = null;

    protected VerticalLayoutGroup _mainLayoutGroup = null;

    public void Start()
    {

        _scrollViews = GameObjectUtils.GetComponents<CollapsableScrollView>(gameObject);


        foreach (CollapsableScrollView view in _scrollViews)
        {
            if (view != null)
            {
                view.Init(this);
            }
        }

        _mainScrollRect = GetComponent<ScrollRect>();

        

        if (_mainScrollRect != null)
        {
            RectTransform mainContent = _mainScrollRect.content;
            if (mainContent != null)
            {
                _mainLayoutGroup = mainContent.GetComponent<VerticalLayoutGroup>();
            }
        }



    }

    public void OnUpdateChild()
    {
        if (_mainLayoutGroup != null)
        {
            float overallSize = 0;

            if (_mainScrollRect != null)
            {
                overallSize = _mainScrollRect.GetComponent<RectTransform>().rect.height;
            }

            float totalChildSize = 0;

            for (int c = 0; c < _mainLayoutGroup.transform.childCount; c++)
            {
                GameObject child = _mainLayoutGroup.transform.GetChild(c).gameObject;

                CollapsableScrollView collapsingRect = child.GetComponent<CollapsableScrollView>();
                if (collapsingRect != null)
                {
                    totalChildSize += collapsingRect.GetCurrentHeight();
                }
                else
                {
                    totalChildSize += _mainLayoutGroup.transform.GetChild(c).GetComponent<RectTransform>().rect.height;
                }
            }

            RectTransform layoutRect = _mainLayoutGroup.GetComponent<RectTransform>();

            layoutRect.sizeDelta = new Vector2(layoutRect.rect.width, totalChildSize);

           _mainLayoutGroup.enabled = false;
           _mainLayoutGroup.enabled = true;

        }
    }




}