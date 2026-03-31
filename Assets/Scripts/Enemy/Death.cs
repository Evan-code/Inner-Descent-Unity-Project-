using System.Collections;      // Lets us use basic Unity features
using System.Collections.Generic; 
using UnityEngine;             // Needed for all Unity scripts

// This script destroys the object when its health reaches 0
public class DieOnZero : MonoBehaviour
{
   private Health health;  // This will store the Health script on this object
   public Animator animator;

   void Start()
   {
       // Find the Health script attached to THIS object
       health = GetComponent<Health>();

       // When the Health script says "I died",
       // run the Die() function below
       health.OnDied += Die;
   }

   void Die()
   {
        animator.SetTrigger("Die");
        Destroy(gameObject); // Destroy object after 2 seconds
   }

}

