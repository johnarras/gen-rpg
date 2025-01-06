
using UnityEngine; // Needed
public class ShaderVariantContainer : BaseBehaviour
{
    public ShaderVariantCollection Variants;

    public void Start()
    {
        Variants?.WarmUp();
    }
}
