using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DocilePassThrough : MonoBehaviour
{
    [SerializeField] Collider2D solidCollider; // your body collider
    [SerializeField] bool docile = true;       // toggle in Inspector for now

    void OnValidate() { Apply(); }
    void Start() { Apply(); }

    void Apply()
    {
        if (!solidCollider) return;
        solidCollider.isTrigger = docile; // true -> player can pass through the body
    }

    public void SetDocile(bool v) { docile = v; Apply(); }
}
