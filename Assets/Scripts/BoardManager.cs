using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; set; }
    private bool[,] allowedMoves { get; set; }

    private const float TILE_SIZE = 1.0f;
    private const float TILE_OFFSET = 0.5f;

    private int selectionX = -1;
    private int selectionY = -1;

    public List<GameObject> chessmanPrefabs;
    private List<GameObject> activeChessman;
	//change made by christian dahl
	List<int>takenSpaces = new List<int>();

	[SerializeField] private bool isFischerChess = false;

	[SerializeField] private UnitTests uTest = new UnitTests ();
	//end change

    private Quaternion whiteOrientation = Quaternion.Euler(0, 270, 0);
    private Quaternion blackOrientation = Quaternion.Euler(0, 90, 0);

    public Chessman[,] Chessmans { get; set; }
    private Chessman selectedChessman;

    public bool isWhiteTurn = true;

    private Material previousMat;
    public Material selectedMat;

    public int[] EnPassantMove { set; get; }

    // Use this for initialization
    void Start()
    {
        Instance = this;
		//change by christian dahl
		if (isFischerChess) {
			SpawnFischerChessmans ();
		} else {
			SpawnAllChessmans ();
		}
		//run unit test
		uTest.UnitTesting ();
		//end change
        EnPassantMove = new int[2] { -1, -1 };
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSelection();

        if (Input.GetMouseButtonDown(0))
        {
            if (selectionX >= 0 && selectionY >= 0)
            {
                if (selectedChessman == null)
                {
                    // Select the chessman
                    SelectChessman(selectionX, selectionY);
                }
                else
                {
                    // Move the chessman
                    MoveChessman(selectionX, selectionY);
                }
            }
        }

        if (Input.GetKey("escape"))
            Application.Quit();
    }

    private void SelectChessman(int x, int y)
    {
        if (Chessmans[x, y] == null) return;

        if (Chessmans[x, y].isWhite != isWhiteTurn) return;

        bool hasAtLeastOneMove = false;

        allowedMoves = Chessmans[x, y].PossibleMoves();
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                if (allowedMoves[i, j])
                {
                    hasAtLeastOneMove = true;
                    i = 8;
                    break;
                }
            }
        }

        if (!hasAtLeastOneMove)
            return;

        selectedChessman = Chessmans[x, y];
        previousMat = selectedChessman.GetComponent<MeshRenderer>().material;
        selectedMat.mainTexture = previousMat.mainTexture;
        selectedChessman.GetComponent<MeshRenderer>().material = selectedMat;

        BoardHighlights.Instance.HighLightAllowedMoves(allowedMoves);
    }

    private void MoveChessman(int x, int y)
    {
        if (allowedMoves[x, y])
        {
            Chessman c = Chessmans[x, y];

            if (c != null && c.isWhite != isWhiteTurn)
            {
                // Capture a piece

                if (c.GetType() == typeof(King))
                {
                    // End the game
                    EndGame();
                    return;
                }

                activeChessman.Remove(c.gameObject);
                Destroy(c.gameObject);
            }
            if (x == EnPassantMove[0] && y == EnPassantMove[1])
            {
                if (isWhiteTurn)
                    c = Chessmans[x, y - 1];
                else
                    c = Chessmans[x, y + 1];

                activeChessman.Remove(c.gameObject);
                Destroy(c.gameObject);
            }
            EnPassantMove[0] = -1;
            EnPassantMove[1] = -1;
            if (selectedChessman.GetType() == typeof(Pawn))
            {
                if(y == 7) // White Promotion
                {
                    activeChessman.Remove(selectedChessman.gameObject);
                    Destroy(selectedChessman.gameObject);
                    SpawnChessman(1, x, y, true);
                    selectedChessman = Chessmans[x, y];
                }
                else if (y == 0) // Black Promotion
                {
                    activeChessman.Remove(selectedChessman.gameObject);
                    Destroy(selectedChessman.gameObject);
                    SpawnChessman(7, x, y, false);
                    selectedChessman = Chessmans[x, y];
                }
                EnPassantMove[0] = x;
                if (selectedChessman.CurrentY == 1 && y == 3)
                    EnPassantMove[1] = y - 1;
                else if (selectedChessman.CurrentY == 6 && y == 4)
                    EnPassantMove[1] = y + 1;
            }

            Chessmans[selectedChessman.CurrentX, selectedChessman.CurrentY] = null;
            selectedChessman.transform.position = GetTileCenter(x, y);
            selectedChessman.SetPosition(x, y);
            Chessmans[x, y] = selectedChessman;
            isWhiteTurn = !isWhiteTurn;
        }

        selectedChessman.GetComponent<MeshRenderer>().material = previousMat;

        BoardHighlights.Instance.HideHighlights();
        selectedChessman = null;
    }

    private void UpdateSelection()
    {
        if (!Camera.main) return;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 50.0f, LayerMask.GetMask("ChessPlane")))
        {
            selectionX = (int)hit.point.x;
            selectionY = (int)hit.point.z;
        }
        else
        {
            selectionX = -1;
            selectionY = -1;
        }
    }

    private void SpawnChessman(int index, int x, int y, bool isWhite)
    {
        Vector3 position = GetTileCenter(x, y);
        GameObject go;

        if (isWhite)
        {
            go = Instantiate(chessmanPrefabs[index], position, whiteOrientation) as GameObject;
        }
        else
        {
            go = Instantiate(chessmanPrefabs[index], position, blackOrientation) as GameObject;
        }

        go.transform.SetParent(transform);
        Chessmans[x, y] = go.GetComponent<Chessman>();
        Chessmans[x, y].SetPosition(x, y);
        activeChessman.Add(go);
    }

    private Vector3 GetTileCenter(int x, int y)
    {
        Vector3 origin = Vector3.zero;
        origin.x += (TILE_SIZE * x) + TILE_OFFSET;
        origin.z += (TILE_SIZE * y) + TILE_OFFSET;

        return origin;
    }

	//change by christian dahl

	private void SpawnFischerChessmans()
	{
		activeChessman = new List<GameObject>();
		Chessmans = new Chessman[8, 8];


		FischersBishops ();
		FischersGuard ();
		FischerKing ();


		PawnSpawn ();




		//saving incorrect code for posterity
		/*activeChessman = new List<GameObject>();
		Chessmans = new Chessman[8, 8];

		//initalize key varaibles
		//king x value is stored for determining king coordinate
		int kingX = 0;
		//int for saving a random value for both sides.
		int save = 0;
		//array of ints for storing taken x values
		List<int> taken = new List<int>();

		//king placement determination
		//king cannot be placed in a or h, since rooks need to be around the king
		kingX = Random.Range(1,6)
			taken.Add(kingX);
			//king 
			//white
			SpawnChessman(0, kingX, 0, true);
			//black
			SpawnChessman(6,kingX,7,false);
		//rooks
		save= Random.Range(0,kingX-1);
		taken.Add(save);
		//white
		SpawnChessman(2,save,0,true);
		//black
		SpawnChessman(8,save,0,false);
		//rook 2
		save=Random.Range(kingX+1,7);
		taken.Add(save);
		SpawnChessman(2,save,0,true);
		//black
		SpawnChessman(8,save,7,false);

		//bishops
		//even is black odd is white
		bool takenSpace = true;
		while(takenSpace)
		{
			save=Random.Range(0,7);
			takenSpace = taken.Contains(save);
		}
		//bishop1 placement
		//white
		SpawnChessman(3,save,0,true);
		//black
		SpawnChessman(9,
		//next we will find what color the bishop is on and then use that information to determine the next bishop spot
		if(save %*/

	}

	//bishops!
	private void FischersBishops()
	{
		//bishops first and foremost
		int x = UnityEngine.Random.Range(0,7);
		int y = UnityEngine.Random.Range(0,7);
		takenSpaces.Add (x);


		if (x % 2 == 0) {
			//its on black square
			//spawn on white
			while (y % 2 == 0) {
				y = UnityEngine.Random.Range (0, 7);
			}
		} else {
			//on white
			//spawn on black
			while (y % 2 != 0) {
				y = UnityEngine.Random.Range (0, 7);
			}
		}
		takenSpaces.Add (y);

		SpawnBishops (x, y);
	}

	private void SpawnBishops(int one, int two)
	{
		SpawnChessman (3, one, 0, true);
		SpawnChessman(9,one,7,false);

		SpawnChessman (3, two, 0, true);
		SpawnChessman(9,two,7,false);
	}

	//Everything Else That Doesnt Have Requirements (TM)
	private void FischersGuard()
	{
		int queen = findASpot ();
		int knight1 = findASpot ();
		int knight2 = findASpot ();

		SpawnQueensGuard (queen, knight1, knight2);
	}
	private void SpawnQueensGuard(int q,int k1,int k2)
	{
		SpawnChessman(1, q, 0, true);
		SpawnChessman(7, q, 7, false);

		// Knights
		SpawnChessman(4, k1, 0, true);
		SpawnChessman(4, k2, 0, true);
		// Knights
		SpawnChessman(10, k1, 7, false);
		SpawnChessman(10, k2, 7, false);
	}

	private int findASpot()
	{
		int x = UnityEngine.Random.Range (0, 7);
		while (takenSpaces.Contains (x)) {
			x = UnityEngine.Random.Range (0, 7);
		}
		takenSpaces.Add (x);
		return x;
	}

	//King and His Rooks
	private void FischerKing()
	{
		SpawnKingRook (NextOpen (), NextOpen (), NextOpen ());
	}

	private void SpawnKingRook(int rk1, int kng, int rk2)
	{
		// King
		SpawnChessman(0, kng, 0, true);
		// Rooks
		SpawnChessman(2, rk1, 0, true);
		SpawnChessman(2, rk2, 0, true);	

		// King
		SpawnChessman(6, kng, 7, false);
		// Rooks
		SpawnChessman(8, rk1, 7, false);
		SpawnChessman(8, rk2, 7, false);
	}
	private int NextOpen()
	{
		int next = 0;
		while (takenSpaces.Contains (next)) {
			next++;
		}
		takenSpaces.Add (next);

		return next;
	}


	private void PawnSpawn()
	{
		// Pawns
		for (int i = 0; i < 8; i++)
		{
			SpawnChessman(5, i, 1, true);
		}
		// Pawns
		for (int i = 0; i < 8; i++)
		{
			SpawnChessman(11, i, 6, false);
		}
	}

	//change end







    private void SpawnAllChessmans()
    {
        activeChessman = new List<GameObject>();
        Chessmans = new Chessman[8, 8];

        /////// White ///////

        // King
        SpawnChessman(0, 3, 0, true);

        // Queen
        SpawnChessman(1, 4, 0, true);

        // Rooks
        SpawnChessman(2, 0, 0, true);
        SpawnChessman(2, 7, 0, true);

        // Bishops
        SpawnChessman(3, 2, 0, true);
        SpawnChessman(3, 5, 0, true);

        // Knights
        SpawnChessman(4, 1, 0, true);
        SpawnChessman(4, 6, 0, true);

        // Pawns
        for (int i = 0; i < 8; i++)
        {
            SpawnChessman(5, i, 1, true);
        }


        /////// Black ///////

        // King
        SpawnChessman(6, 4, 7, false);

        // Queen
        SpawnChessman(7, 3, 7, false);

        // Rooks
        SpawnChessman(8, 0, 7, false);
        SpawnChessman(8, 7, 7, false);

        // Bishops
        SpawnChessman(9, 2, 7, false);
        SpawnChessman(9, 5, 7, false);

        // Knights
        SpawnChessman(10, 1, 7, false);
        SpawnChessman(10, 6, 7, false);

        // Pawns
        for (int i = 0; i < 8; i++)
        {
            SpawnChessman(11, i, 6, false);
        }
    }
		

    private void EndGame()
    {
        if (isWhiteTurn)
            Debug.Log("White wins");
        else
            Debug.Log("Black wins");

        foreach (GameObject go in activeChessman)
        {
            Destroy(go);
        }

        isWhiteTurn = true;
        BoardHighlights.Instance.HideHighlights();
        SpawnAllChessmans();
    }
}


