using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFacingDirectionTarget : MonoBehaviour
{
    CharacterScript player;
    public float flipYTime = 0.4f;
    public float lookOffset = 0.5f;
    private Vector3 startLookPosition;
    
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<CharacterScript>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = player.transform.position;
        if (player.IsLookingUp || player.IsLookingDown) startLookPosition = transform.position;
    }

    public void Turn()
    {
        LeanTween.rotateY(gameObject, player.IsFacingRight? 0f : 180f, flipYTime).setEaseInOutSine();
    }

    public void Look()
    {
        if (player.IsMoving) return;
        if (player.IsLookingUp) LeanTween.moveLocalY(gameObject, startLookPosition.y + lookOffset, 0.1f);
        if (player.IsLookingDown) LeanTween.moveLocalY(gameObject, startLookPosition.y - lookOffset, 0.1f);
    }

}
