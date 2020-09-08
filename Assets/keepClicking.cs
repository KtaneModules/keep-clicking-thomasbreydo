using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
using KModkit;

public class keepClicking : MonoBehaviour 
{
	public KMBombInfo Bomb;
	public KMBombModule Module;
	public KMAudio Audio;
	public KMSelectable[] buttons;
	public TextMesh[] buttonTextMeshes;
	public KMSelectable submitButton;

	private static int _moduleIdCounter = 1;
	private int _moduleId = 0;
	private bool _SIGisLit;
	private int _nBatteries;
	private static string vowels = "aeiouAEIOU";
	private bool _serialNumHasVowel;

	private enum ButtonType 
	{
		aK = 0,
		pN = 1,
		gX = 2
	}

	private class Symbol 
	{
		public Symbol () {}
		public Symbol (string text_,
                 FontStyle fontStyle_,
                 ButtonType stopSymbolForButtonTypeX_)
		{
			text = text_;
			fontStyle = fontStyle_;
			stopSymbolForButtonTypeX = stopSymbolForButtonTypeX_;
		}
		public string text { get; set; }
        public FontStyle fontStyle { get; set; }
		public ButtonType stopSymbolForButtonTypeX { get; set; }
	}

	private ButtonType[] buttonTypes = new ButtonType[6];
	private ButtonType[] symbolTypes = new ButtonType[6];
	private static Symbol[] stopSymbols_aK =
	{
		new Symbol("\u2653", FontStyle.Bold, ButtonType.aK),
		new Symbol("\u2648", FontStyle.Bold, ButtonType.aK),
		new Symbol("\u03e0", FontStyle.Normal, ButtonType.aK),
		new Symbol("\uff94", FontStyle.Normal, ButtonType.aK),
		new Symbol("\u03c3", FontStyle.Normal, ButtonType.aK),
		new Symbol("\u0460", FontStyle.Normal, ButtonType.aK),
		new Symbol("\u264a", FontStyle.Bold, ButtonType.aK)
	};

	private static Symbol[] stopSymbols_pN = 
	{
		new Symbol("\u0c97", FontStyle.Bold, ButtonType.pN),
		new Symbol("\u264c", FontStyle.Bold, ButtonType.pN),
		new Symbol("\u0b69", FontStyle.Bold, ButtonType.pN),
		new Symbol("\u0629", FontStyle.Normal, ButtonType.pN),
		new Symbol("\u264b", FontStyle.Bold, ButtonType.pN),
		new Symbol("\u264f", FontStyle.Bold, ButtonType.pN),
		new Symbol("\u264e", FontStyle.Bold, ButtonType.pN)
	};

	private static Symbol[] stopSymbols_gX = 
	{
		new Symbol("\u264d", FontStyle.Bold, ButtonType.gX),
		new Symbol("\u2651", FontStyle.Bold, ButtonType.gX),
		new Symbol("\u05f1", FontStyle.Normal, ButtonType.gX),
		new Symbol("\u2649", FontStyle.Bold, ButtonType.gX),
		new Symbol("\u2652", FontStyle.Bold, ButtonType.gX),
		new Symbol("\u09ec", FontStyle.Bold, ButtonType.gX),
		new Symbol("\u2650", FontStyle.Bold, ButtonType.gX)
	};

	private static Dictionary <ButtonType, Symbol[]> stopSymbolsForButtonType =
		new Dictionary <ButtonType, Symbol[]> 
		{
			{ButtonType.aK, stopSymbols_aK},
			{ButtonType.pN, stopSymbols_pN},
			{ButtonType.gX, stopSymbols_gX}
		};

	private Dictionary <int, Symbol[]>  symbolsForButtonIndex =
		new Dictionary <int, Symbol[]>();

	// Run once, while loading screen shows
	void Start () 
	{
		_moduleId = _moduleIdCounter++;
		_nBatteries = Bomb.GetBatteryCount();
		_SIGisLit = Bomb.IsIndicatorOn("SIG");
		_serialNumHasVowel = serialNumHasVowel();
		GenerateModule();
	}

	private bool serialNumHasVowel()
	{
		foreach (char c in Bomb.GetSerialNumber())
		{
			if (vowels.Contains(c)) return true;
		}
		return false;
	}
	
	private void Awake () 
	{
		for (int i = 0; i < buttons.Length; i++) 
		{
			int j = i;
			buttons[i].OnInteract += delegate () 
			{
				handleClickButtonAtIndex(j);
				return false;
			};
		}
		submitButton.OnInteract += delegate ()
		{
			handleClickSubmit();
			return false;
		};
	}

	void GenerateModule () {
		for (int i = 0; i < buttons.Length; i++)
		{
			buttonTypes[i] = getTypeOfButtonAtIndex(i);
			assignNewSymbolNotStopForButtonAtIndex(i);
		}
		Debug.LogFormat("[Keep Clicking #{0}] Button types (left-to-right, "
		              + "top-to-bottom): {1}", _moduleId, 
					  string.Join(" ", new List<ButtonType>(buttonTypes)
					  	.ConvertAll(i => i.ToString()).ToArray()));
		Debug.LogFormat(
			"[Keep Clicking #{0}] Symbols (left-to-right, top-to-bottom):", _moduleId);
		foreach (TextMesh textMesh in buttonTextMeshes)
		{
			Debug.LogFormat(textMesh.text);
		}
	}

	ButtonType getTypeOfButtonAtIndex (int index) {
		if (isInTopRow(index) && _nBatteries > 2)
		{
			return ButtonType.pN;
		}
		if (isInBottomRow(index) && _SIGisLit)
		{
			return ButtonType.aK;
		}
		if (_serialNumHasVowel)
		{
			return ButtonType.pN;
		}
		return ButtonType.gX;
	}

	bool isInTopRow (int index)
	{
		return (index <= 2);
	}

	bool isInBottomRow (int index)
	{
		return (index >= 3);
	}

	Symbol randomSymbolNotStopSymbolForIndex (int index)
	{
		return randomSymbolNotStopSymbolForType(getTypeOfButtonAtIndex(index));
	}

	// symbol that isn't a valid stop symbol for this button
	Symbol randomSymbolNotStopSymbolForType (ButtonType type)
	{
		switch (type)
		{
			case ButtonType.aK:
				return randomSymbolNotStopSymbolForType_aK();
			case ButtonType.pN:
				return randomSymbolNotStopSymbolForType_pN();
			case ButtonType.gX:
				return randomSymbolNotStopSymbolForType_gX();
		}
		throw new System.ArgumentException("Invalid button type", "type");
	}
	
	Symbol randomSymbolNotStopSymbolForType_aK ()
	{
		int symbolIndex = Random.Range(0, 14);
		if (symbolIndex < 7) {
			return stopSymbols_pN[symbolIndex];
		}
		return stopSymbols_gX[symbolIndex - 7];
	}
	
	Symbol randomSymbolNotStopSymbolForType_pN ()
	{
		int symbolIndex = Random.Range(0, 14);
		if (symbolIndex < 7) {
			return stopSymbols_aK[symbolIndex];
		}
		return stopSymbols_gX[symbolIndex - 7];
	}
	
	Symbol randomSymbolNotStopSymbolForType_gX ()
	{
		int symbolIndex = Random.Range(0, 14);
		if (symbolIndex < 7) {
			return stopSymbols_aK[symbolIndex];
		}
		return stopSymbols_pN[symbolIndex - 7];
	}

	Symbol randomStopSymbolForType (ButtonType type)
	{
		return stopSymbolsForButtonType[type][Random.Range(0, 7)];
	}

	private void handleClickSubmit ()
	{
		Audio.PlayGameSoundAtTransform(
			KMSoundOverride.SoundEffect.ButtonPress, submitButton.transform);
		submitButton.AddInteractionPunch();
		handlePassOrStrike();
	}

	void handleClickButtonAtIndex (int index)
	{
		Audio.PlayGameSoundAtTransform(
			KMSoundOverride.SoundEffect.ButtonPress, buttons[index].transform);
		buttons[index].AddInteractionPunch();
		setNewSymbolForButtonAtIndex(index);
	}

	void setNewSymbolForButtonAtIndex (int index)
	{
		if (Random.Range(0, 2) == 0)  // appx. 50% that the button will be done
		{
			assignNewSymbolNotStopForButtonAtIndex(index);
		}
		else
		{
			assignNewSymbolStopForButtonAtIndex(index);
		}
	}

	void assignNewRandomSymbolToButtonAtIndex (int index)
	{
		if (Random.Range(0, 3) == 0)
		{
			assignNewSymbolStopForButtonAtIndex(index);
		}
		else
		{
			assignNewSymbolNotStopForButtonAtIndex(index);
		}
	}

	void assignNewSymbolNotStopForButtonAtIndex (int index)
	{
		Symbol newSymbol;
		string oldSymbolText = buttonTextMeshes[index].text;
		while (true)
		{
			newSymbol = randomSymbolNotStopSymbolForIndex(index);
			if (newSymbol.text != oldSymbolText)
			{
				break;
			}
		}
		assignSymbolToTextMeshAtIndex(index, newSymbol);
	}
	
	void assignNewSymbolStopForButtonAtIndex (int index)
	{
		Symbol newSymbol;
		string oldSymbolText = buttonTextMeshes[index].text;
		while (true)
		{
			newSymbol = randomStopSymbolForType(buttonTypes[index]);
			if (newSymbol.text != oldSymbolText)
			{
				break;
			}
		}
		assignSymbolToTextMeshAtIndex(index, newSymbol);
	}

	void handlePassOrStrike ()
	{
		for (int i = 0; i < buttons.Length; i++)
		{
			if (buttonAtIndexIsNotDone(i))
			{
				Module.HandleStrike();
				return;	
			}
		}
		Module.HandlePass();
	}

	bool buttonAtIndexIsNotDone (int index)
	{
		return symbolTypes[index] != buttonTypes[index];
	}

	void assignSymbolToTextMeshAtIndex (int index, Symbol symbol)
	{
		_setTextMeshToSymbol(ref buttonTextMeshes[index], symbol);
		symbolTypes[index] = symbol.stopSymbolForButtonTypeX;
	}

	private void _setTextMeshToSymbol (ref TextMesh mesh, Symbol symbol)
	{
		mesh.text = symbol.text;
		mesh.fontStyle = symbol.fontStyle;
	}
}
