using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FillGrid : MonoBehaviour {

	GameObject[] letters;
	public float letter_width=1f;
	private GridItem[,] items;
	public int Xsize;
	public int Ysize;

	private GridItem selectedItem;
	private float movementDuration = 0.3f;
	private string[] toMatchWords;
	private List<GridItem> itemsToDestroy;
	private bool[,] itemToDestroyAlreadyExist;

	public Text[] textToShowWord;
	public Text[] textToShowDefinition;
	public Image[] imgToShowWord;
	int wordMatchCount;
	int scoreToShow;
	public Text scoreText;

	void Start(){
		InitialStart ();
	}

	void InitialStart(){
		itemsToDestroy = new List<GridItem> ();
		itemToDestroyAlreadyExist =new bool[Xsize,Ysize];
		wordMatchCount = 0;
		scoreToShow = 0;
		ClearText ();
		GetLetters ();
		StartFillGrid ();
		GridItem.OnMouseOverItemEventHandler += OnMouseOverItem;
		toMatchWords = new string[]{"cat","dog","bird","bat","rat","act","air","cog","god","rid","bar","car","aid","bot","dig","bid","rag","rod","bag","raid","grid","arc","tatto","braid","dirt",
			"dart", "rot", "roar", "cord", "dad", "door", "big", "bog", "craw", "bit", "tot", "tact", "tod", "good", "acid", "art", "critic", "cod", "goo","grab","rob","dot","bad","tab","gab",
			"odd","cab","did","do","at","ad","go","too","rig","tag"};
	}

	void Update(){
		if (Input.GetKeyUp("r")) {
			SceneManager.LoadScene ("match3_test");
			InitialStart ();
		}
	}

	void ClearText(){
		for (int i = 0; i < textToShowWord.Length; i++) {
			textToShowWord [i].text = "";
			textToShowDefinition [i].text = "";
			imgToShowWord [i].enabled = false;
		}
	}

	void StartFillGrid(){
		items = new GridItem[Xsize, Ysize];
		for (int i = 0; i < Xsize; i++) {
			for (int j = 0; j < Ysize; j++) {
				items [i, j] = InstantiateLetter (i, j);
				itemToDestroyAlreadyExist [i, j] = false;
			}
		}
	}

	void GetLetters() {
		letters = Resources.LoadAll<GameObject> ("Prefabs/LetterPrefab");
		for (int i=0; i<letters.Length;i++){
			string letterName = letters [i].name;
			switch (letterName) {
			case "Letter_A":
				letters [i].GetComponent<GridItem> ().content = "a";
				break;
			case "Letter_B":
				letters [i].GetComponent<GridItem> ().content = "b";
				break;
			case "Letter_C":
				letters [i].GetComponent<GridItem> ().content = "c";
				break;
			case "Letter_D":
				letters [i].GetComponent<GridItem> ().content = "d";
				break;
			case "Letter_G":
				letters [i].GetComponent<GridItem> ().content = "g";
				break;
			case "Letter_I":
				letters [i].GetComponent<GridItem> ().content = "i";
				break;
			case "Letter_O":
				letters [i].GetComponent<GridItem> ().content = "o";
				break;
			case "Letter_R":
				letters [i].GetComponent<GridItem> ().content = "r";
				break;
			case "Letter_T":
				letters [i].GetComponent<GridItem> ().content = "t";
				break;
			}
		}
	}

	GridItem InstantiateLetter (int x, int y) {
		GameObject randomLetter = letters [Random.Range (0, letters.Length)];
		GridItem newLetter = ((GameObject)Instantiate(randomLetter, new Vector2(x*letter_width-2.25f,-y+5f),Quaternion.identity)).GetComponent<GridItem>();
		newLetter.OnItemPositionChanged (x,y);
		return newLetter;
	}

	void OnMouseOverItem(GridItem item) {
		if (selectedItem == null) {
			selectedItem = item;
			//Debug.Log("start");
		} else {
			float xDistance = Mathf.Abs (item.x - selectedItem.x);
			float yDistance = Mathf.Abs (item.y - selectedItem.y);

			if (xDistance + yDistance == 1) {
				bool sameX;
				if (xDistance == 0) {
					sameX = true;
				} else {
					sameX = false;
				}
				StartCoroutine(SwapItems (selectedItem, item, sameX));
			} 

			selectedItem = null;
			//Debug.Log("end");
		}
	}

	void SearchHorizontalMatch(GridItem item){
		string matchString = item.content;
		int left = item.x - 1;
		int right = item.x + 1;

		while ((left >= 0)&&(items [left, item.y] != null)) {
			matchString = items [left, item.y].content + matchString;
			left--;
		}

		while ((right < Xsize)&&(items [right, item.y] != null)) {
			matchString = matchString + items [right, item.y].content;
			right++;
		}

		left++;
		right--;

		int minX; int maxX;
		TryMatch (matchString, left, out minX, out maxX);

		for (int i=minX; i<maxX; i++){
			if (!itemToDestroyAlreadyExist [i, item.y]) {
				itemsToDestroy.Add (items [i, item.y]);
				itemToDestroyAlreadyExist [i, item.y] = true;
			}
		}
	}

	void SearchVerticalMatch(GridItem item){
		string matchString = item.content;
		int lower = item.y - 1;
		int upper = item.y + 1;

		while ((lower >= 0)&&(items [lower, item.y] != null)) {
			matchString = items [item.x, lower].content + matchString;
			lower--;
		}

		while ((upper < Ysize)&&(items [upper, item.y] != null)) {
			matchString =  matchString + items [item.x, upper].content;
			upper++;
		}

		lower++;
		upper--;

		int minY; int maxY;
		TryMatch (matchString, lower, out minY, out maxY);

		for (int i=minY; i<maxY; i++){
			if (!itemToDestroyAlreadyExist [item.x, i]) {
				itemsToDestroy.Add (items [item.x, i]);
				itemToDestroyAlreadyExist [item.x, i] = true;
			}
		}
	}

	void TryMatch(string toMatchString, int minIndex, out int minDestroy, out int maxDestroy) {
		bool isMatch;
		int indexMatchBegin=0;
		int matchLength=0;
		string maxMatchedWord = "no match";
		minDestroy = -1;
		maxDestroy = -1;
		for (int i=0; i<toMatchWords.Length; i++){
			isMatch = toMatchString.Contains (toMatchWords[i]);
			if ((isMatch)&&(toMatchWords[i].Length>matchLength)){
				maxMatchedWord = toMatchWords [i];
				indexMatchBegin = toMatchString.IndexOf (maxMatchedWord);
				matchLength = maxMatchedWord.Length;
				minDestroy = minIndex + indexMatchBegin;
				maxDestroy = minDestroy + matchLength;
			}
		}
		//Debug.Log(maxMatchedWord);
		if (maxMatchedWord!="no match"){
			if (wordMatchCount == 0) {
				ClearText ();
			}
			textToShowWord [wordMatchCount].text = maxMatchedWord;
			textToShowDefinition [wordMatchCount].text = "/kat/\n" +
				"noun\n" + "1. a small domesticated carnivorous mammal with soft fur, a short snout, and rectractile claws.";
			imgToShowWord [wordMatchCount].enabled = true;
			wordMatchCount++;
			scoreToShow += maxMatchedWord.Length;
		}
	}

	IEnumerator SwapItems(GridItem a, GridItem b, bool sameX) {
		ChangeRigidbody2DStatus (false);
		Vector3 aPos = a.transform.position;
		Vector3 bPos = b.transform.position;
		StartCoroutine (a.transform.Move (bPos, movementDuration));
		StartCoroutine (b.transform.Move (aPos, movementDuration));
		yield return new WaitForSeconds(movementDuration);
		ExchangeItems (a,b);
		if (sameX) {
			SearchHorizontalMatch (a);
			SearchHorizontalMatch (b);
			SearchVerticalMatch (a);
		} else {
			SearchHorizontalMatch (a);
			SearchVerticalMatch (a);
			SearchVerticalMatch (b);
		}
		foreach (GridItem iDestroy in itemsToDestroy) {
			if (iDestroy != null) {
				Destroy (iDestroy.gameObject);
				items [iDestroy.x, iDestroy.y] = null;
				for (int i=iDestroy.y; i>0; i--) {
					if (items [iDestroy.x, i-1] != null) {
						items [iDestroy.x, i - 1].OnItemPositionChanged (iDestroy.x, i);//items [iDestroy.x, i-1].y + 1);
						items [iDestroy.x, i] = items [iDestroy.x, i - 1];
						items [iDestroy.x, i - 1] = null;
					}
				}
			}
		}
		scoreText.text = "Score: "+scoreToShow.ToString();
		ChangeRigidbody2DStatus (true);
		wordMatchCount = 0;

		yield return new WaitForSeconds (0.3f);
		for (int i = 0; i < Xsize; i++) {
			for (int j = 0; j < Ysize; j++) {
				itemToDestroyAlreadyExist [i, j] = false;
				if (items[i, j] == null) {
					items [i, j] = InstantiateLetter (i, j);
				}
			}
		}
			
	}

	void ExchangeItems(GridItem a, GridItem b) {
		GridItem tempA = items[a.x,a.y];
		items [a.x, a.y] = b;
		items [b.x, b.y] = tempA;
		int bX = b.x; int bY = b.y;

		b.OnItemPositionChanged (a.x,a.y);
		a.OnItemPositionChanged (bX,bY);
	}

	void ChangeRigidbody2DStatus(bool status) {
		foreach (GridItem g in items) {
			if (g != null) {
				g.GetComponent<Rigidbody2D>().isKinematic = !status;
			}
		}
	}
}

