using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BackgroundScript : MonoBehaviour {
    [SerializeField] private GameObject cam;
    [SerializeField] private float parallaxFX;
    private float length, startPos, constY;

    void Start() {
        startPos = transform.position.x;
        constY = transform.position.y;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    // Parallax background implementation. 
    void Update() {
        float t = cam.transform.position.x * (1-parallaxFX);
        float dist = cam.transform.position.x * parallaxFX;
        transform.position = new Vector2(startPos + dist, constY);

        if (t > startPos + length)
            startPos += length;
        else if ( t < startPos - length)
            startPos -= length;
    }
}
