using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.PlayerLoop;

namespace Pixelated
{
    [InitializeOnLoad]
    public class Initialization
    {
        static Initialization()
        {
            LayerMaskEx.CreateLayer(Pixelate.LAYER_NAME);

            if (LayerMask.NameToLayer(Pixelate.LAYER_NAME) == -1)
                Debug.LogError("Couln't create pixel layer");
        }
    }
}
