
using Genrpg.Shared.UI.Interfaces;
public class GImage : UnityEngine.UI.Image, IImage
{
    public float FillAmount { get { return fillAmount; } set { fillAmount = value; } }

    public UnityEngine.Color Color { get { return color; } set {  color = value; } }
}