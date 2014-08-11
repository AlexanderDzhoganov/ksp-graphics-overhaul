using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace ksp_graphics_overhaul
{

    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class Class1 : MonoBehaviour
    {
        public void Awake()
        {
            print("ksp-graphics-overhaul: Hello world!");
        }
    }

}
