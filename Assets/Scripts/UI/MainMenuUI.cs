// These libraries allow us to use different Unity features
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Lets us load and switch scenes

// This script will control buttons in the main menu
public class MainMenuUI : MonoBehaviour
{
   // This function runs when the player presses the "Start Game" button
   public void StartGame()
   {
       // Load a scene called "GameScene"
       // The scene must exist in the Build Settings for this to work
       SceneManager.LoadScene("GameScene");
   }

   // This function runs when the player presses the "Quit Game" button
   public void QuitGame()
   {
       // This stops the game when you're running it inside the Unity editor
       // It basically exits "Play Mode"
       UnityEditor.EditorApplication.isPlaying = false;

       // NOTE:
       // If this were a built game (not inside Unity), you would usually use:
       // Application.Quit();
   }
}

