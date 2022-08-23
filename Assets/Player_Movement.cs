using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Movement : MonoBehaviour
{
        // Variables
        public Rigidbody2D player;
        public float speedMultiplier;
        Animator animator;
        
        // Start is called before the first frame update
        void Start()
        {
                animator = GetComponent<Animator>();
                animator.Play("Idle");   
        }

        // Update is called once per frame
        void Update()
        {
                float left_right_movement = Input.GetAxis("Horizontal");
                player.velocity = new Vector2(left_right_movement * speedMultiplier * Time.deltaTime, 0f);

                if (left_right_movement != 0)
                {
                        animator.speed = 1;
                        animator.Play("Move_right");
                }
                else
                {
                        animator.speed = 0;
                        animator.Play("Idle");
                }
        }
}