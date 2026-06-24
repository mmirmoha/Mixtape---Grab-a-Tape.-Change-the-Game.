using System;
using UnityEngine;

// Tiny helper so a panel (e.g. the Shop) can refresh its labels each time it is shown.
public class OnEnableRunner : MonoBehaviour
{
    public Action OnShown;
    void OnEnable() { OnShown?.Invoke(); }
}
