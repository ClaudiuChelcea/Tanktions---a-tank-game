using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Movement : MonoBehaviour
{
        // Variables
        public Rigidbody2D player;
        public float speedMultiplier;
   
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
                float left_right_movement = Input.GetAxis("Horizontal");
                player.velocity = new Vector2(left_right_movement * speedMultiplier * Time.deltaTime, 0f);

         
        }
}
