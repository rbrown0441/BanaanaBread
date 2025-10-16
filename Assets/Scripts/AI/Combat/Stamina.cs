using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stamina : MonoBehaviour
{
    [SerializeField] float max = 100f;
    [SerializeField] float regenPerSec = 10f;
    float _cur;

    void Awake() { _cur = max; }
    void Update() { _cur = Mathf.Min(max, _cur + regenPerSec * Time.unscaledDeltaTime); }

    public void Drain(float amt) { _cur = Mathf.Max(0, _cur - amt); }
    public bool IsExhausted => _cur <= 0.01f;
}
