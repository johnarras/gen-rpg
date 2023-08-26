using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ShaderVariantContainer : BaseBehaviour
{
    public ShaderVariantCollection Variants;

    public void Start()
    {
        Variants.WarmUp();
    }
}
