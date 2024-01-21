using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {

	public void StandardChess()
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene ("StandardChess");
	}
	public void NineSixty()
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene ("ChessNineSixty");

	}
}
