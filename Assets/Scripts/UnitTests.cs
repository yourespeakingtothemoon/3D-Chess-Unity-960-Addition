using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitTests : MonoBehaviour {

	[SerializeField] BoardManager bManager;


	// Use this for initialization
	public void UnitTesting () {
		//run unit tests
		//results of unit tests recorded in debug log
		Debug.Log(EmptyMiddleRows());
		Debug.Log (PawnsInPosition ());
		Debug.Log (AllPieces ());
		Debug.Log (EqualOpposite ());
		Debug.Log (BishopWhiteBlack ());
		Debug.Log (KingGuarded ());

	}


	//General Testing

	//Test that no pieces are in rows 3 through 6

	private bool EmptyMiddleRows()
	{
		

		for (int y = 0; y < 8; y++) {

			for (int x = 0; x < 8; x++) {
				var existence = bManager.Chessmans [x, y];
				if(existence!=null)
				{
					if (y > 1 && y < 6) {
						return false;
					}
				}

				}
			}
		return true;

		}
					


	

	//Test that Pawns are all in second tiers
	private bool PawnsInPosition()
	{
		
			for (int x = 0; x < 8; x++) {
			if (!bManager.Chessmans [x, 1].GetType ().Name.Equals("Pawn") || !bManager.Chessmans [x, 6].GetType ().Name.Equals("Pawn")) {
				return false;
			}
			if (bManager.Chessmans [x, 0].GetType ().Name.Equals("Pawn") || bManager.Chessmans [x, 7].GetType ().Name.Equals("Pawn")) {
				return false;
			}
		
		}
		return EmptyMiddleRows ();
		}

	//Test that all pieces are present
	private bool AllPieces()
	{
		int pawnCountWhite = 0;
		int pawnCountBlack = 0;
		bool kingWhite = false;
		bool kingBlack = false;
		bool queenWhite = false;
		bool queenBlack = false;
		int knightCountWhite = 0;
		int knightCountBlack = 0;
		int bishopCountWhite = 0;
		int bishopCountBlack = 0;
		int rookCountWhite = 0;
		int rookCountBlack = 0;
		//start of GPT assisted code
		//I wrote most of the code for this method, but realized that I could save time having GPT fill in the repetitive and tedious code.
		//I prompted it with my initalized variables above and by writing out how I want each check in the switch case to look. It changed the way I do the if else by not using brackets explaining the different coding style
	//https://chat.openai.com/share/62b78ff3-6cc1-4758-8456-e26a266d2aad

		for (int y = 0; y < 8; y++) {
			for (int x = 0; x < 8; x++) {
				if (bManager.Chessmans[x, y] != null) {
					switch (bManager.Chessmans[x, y].GetType().Name) {
					case "Pawn":
						if (bManager.Chessmans[x, y].isWhite) pawnCountWhite++;
						else pawnCountBlack++;
						break;
					case "King":
						if (bManager.Chessmans[x, y].isWhite) kingWhite = true;
						else kingBlack = true;
						break;
					case "Queen":
						if (bManager.Chessmans[x, y].isWhite) queenWhite = true;
						else queenBlack = true;
						break;
					case "Knight":
						if (bManager.Chessmans[x, y].isWhite) knightCountWhite++;
						else knightCountBlack++;
						break;
					case "Bishop":
						if (bManager.Chessmans[x, y].isWhite) bishopCountWhite++;
						else bishopCountBlack++;
						break;
					case "Rook":
						if (bManager.Chessmans[x, y].isWhite) rookCountWhite++;
						else rookCountBlack++;
						break;
					}
				}
			}
		}
		//I also started writing this massive logical statement, but I had GPT finish it to save time
		return pawnCountWhite == 8 && pawnCountBlack == 8 &&
			kingWhite && kingBlack &&
			queenWhite && queenBlack &&
			knightCountWhite == 2 && knightCountBlack == 2 &&
			bishopCountWhite == 2 && bishopCountBlack == 2 &&
			rookCountWhite == 2 && rookCountBlack == 2;

		//end of GPT assisted code
	}

	//Fischer Testing

	//pieces are equal opposite (ie if on f1 black is f8)

	private bool EqualOpposite()
	{
		//only need to test tier 1
		for (int x = 0; x < 8; x++) {
			if(bManager.Chessmans[x,0].GetType() != bManager.Chessmans[x,7].GetType())
			{
				return false;
			}
		}

		return true;

		//easiest one :)

	}

	//bishops in right spaces

	private List<int> findAllBishops()
	{
		List<int> ret = new List<int>();

	//only need to test for white if other tests work
			for (int x = 0; x < 8; x++) {
			if (bManager.Chessmans [x, 0].GetType().Name.Equals("Bishop")) {
					
						ret.Add (x);
					
				}
		}

		return ret;
	}

	private bool BishopWhiteBlack()
	{
		List<int> bishops = findAllBishops ();

	

		if (bishops [0] % 2 == bishops [1] % 2) {
			return false;
		} else {
			return EqualOpposite ();
		}
		
	}
		
	//king between rooks
	private int findKing()
	{
		

		//only need to test for white if other tests work
		for (int x = 0; x < 8; x++) {
			if (bManager.Chessmans [x, 0].GetType().Name.Equals("King")) {
				
					return x;

			}
		}

		return -1;
	}

	private bool KingGuarded()
	{
		int king = findKing ();
		bool beforeRookFound = false;
		bool afterRookFound = false;
		if (king < 0) {
			return false;
		}
		for (int x = 0; x < 8; x++) {
			if (bManager.Chessmans [x, 0].GetType().Name.Equals("Rook")) {
				if (x < king) {
					if (beforeRookFound) {
						return false;
					}
					beforeRookFound = true;
				}
				if (x > king) {
					if (afterRookFound) {
						return false;
					}
					afterRookFound = true;
				}
			}
		}


		return beforeRookFound && afterRookFound && EqualOpposite ();


	}
		

}
