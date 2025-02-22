using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;
using Math = ExMath;

public class BabyWhosOnFirst : MonoBehaviour {

	public KMBombInfo BombInfo;
	public KMAudio Audio;

	public KMSelectable[] Buttons;
	public MeshRenderer[] ButtonMeshes;
	public KMHighlightable[] ButtonHighlights;

	public TextMesh[] AllTexts;

	public MeshRenderer[] OffLEDs;
	public MeshRenderer[] OnLEDs;

	const int SEQUENCE_LENGTH = 24;
	string[] POS = {"top", "bottom"};

	int Stage = 0;

	int CurrentDisplay = -1;
	int[] CurrentButtons = {-1, -1};

	string[] DisplayWords = {
		"GOO", "GAA", "GO", "GAH", "GEW", "GLA", "BLA", "BAH",
		"BLUE", "BLAH", "MA", "DA", "GA", "GEU", "GLUE", "MAH",
		"WAH", "WAAA", "YA", "YAH", "DAH", "GAGA", "BLABLA", "WA",
		"NYOOM", "PEEK", "A", "BOO", "BLU", "CLUE", "UAH", "UAAA",
		"TWA", "AAHH", "LWA", "MAMA", "DADA", "PAPA", "GOOG", "GOOGLE"
	};
	int[] ReferenceLabels = {
		0, 1, 1, 1, 0, 0, 1, 0,
		1, 0, 1, 0, 1, 1, 0, 1,
		1, 0, 0, 1, 0, 1, 0, 0,
		0, 1, 1, 1, 0, 1, 0, 1,
		1, 0, 0, 1, 1, 0, 1, 0 
	};
	
	string[] ButtonWords = {
		"GOO", "GAA", "BLA", "BLUE", "GOOG", "WAAA", "GLUE", "MAMA", "WA",
		"BOO", "DA", "AAHH", "NYOOM", "BLAH"
	};
	int[][] InstructionSequences = new int[14][] {
		new int[] {2, 3, 2, 1, 6, 12, 11, 3, 5, 0, 11, 12, 1, 0, 3, 0, 2, 7, 12, 0, 3, 4, 7, 0},
		new int[] {1, 3, 0, 11, 4, 3, 3, 0, 11, 1, 0, 11, 1, 2, 11, 12, 12, 11, 2, 7, 0, 4, 12, 1},
		new int[] {5, 1, 6, 5, 2, 2, 12, 2, 1, 7, 6, 0, 7, 4, 12, 12, 7, 6, 4, 12, 11, 1, 0, 1},
		new int[] {1, 3, 3, 1, 2, 6, 0, 2, 12, 0, 12, 6, 0, 1, 3, 0, 0, 0, 8, 1, 7, 4, 3, 1},
		new int[] {2, 1, 12, 3, 1, 12, 1, 1, 3, 4, 7, 0, 2, 11, 11, 1, 12, 1, 12, 11, 3, 0, 6, 3},
		new int[] {3, 0, 2, 3, 1, 0, 0, 7, 9, 2, 1, 12, 3, 1, 12, 1, 1, 3, 4, 7, 0, 2, 11, 11, 1, 12, 1, 12, 11, 3, 0, 6, 3},
		new int[] {3, 4, 0, 1, 3, 8, 1, 4, 6, 3, 3, 4, 3, 3, 4, 10, 0, 1, 3, 0, 7, 1, 1, 1},
		new int[] {0, 12, 0, 2, 3, 1, 4, 12, 4, 10, 11, 4, 12, 7, 12, 12, 2, 5, 4, 1, 3, 7, 1, 3},
		new int[] {3, 12, 2, 12, 8, 0, 8, 2, 3, 11, 7, 4, 7, 12, 1, 12, 3, 2, 4, 3, 0, 1, 4, 4},
		new int[] {4, 12, 1, 1, 0, 2, 7, 11, 12, 7, 11, 0, 2, 6, 3, 0, 2, 9, 3, 12, 2, 12, 8, 0, 8, 2, 3, 11, 7, 4, 7, 12, 1, 12, 3, 2, 4, 3, 0, 1, 4, 4},
		new int[] {1, 12, 6, 5, 3, 8, 1, 0, 1, 5, 3, 12, 12, 12, 6, 1, 1, 2, 3, 1, 0, 4, 3, 5},
		new int[] {2, 2, 4, 2, 6, 3, 4, 6, 2, 0, 3, 4, 6, 6, 12, 3, 3, 0, 1, 8, 10, 1, 7, 4},
		new int[] {3, 11, 5, 7, 10, 6, 3, 0, 0, 3, 12, 11, 1, 11, 11, 1, 1, 6, 3, 11, 3, 0, 11, 1},
		new int[] {4, 3, 8, 1, 3, 1, 2, 6, 12, 6, 3, 0, 3, 3, 1, 3, 7, 1, 4, 1, 12, 1, 1, 12}
	};

	// For logging purposes
	string[][] InstructionLetters = new string[14][] {
		new string[] {"C", "D", "C", "B", "G", "M", "L", "D", "F", "A", "L", "M", "B", "A", "D", "A", "C", "H", "M", "A", "D", "E", "H", "A"},
		new string[] {"B", "D", "A", "L", "E", "D", "D", "A", "L", "B", "A", "L", "B", "C", "L", "M", "M", "L", "C", "H", "A", "E", "M", "B"},
		new string[] {"F", "B", "G", "F", "C", "C", "M", "C", "B", "H", "G", "A", "H", "E", "M", "M", "H", "G", "E", "M", "L", "B", "A", "B"},
		new string[] {"B", "D", "D", "B", "C", "G", "A", "C", "M", "A", "M", "G", "A", "B", "D", "A", "A", "A", "I", "B", "H", "E", "D", "B"},
		new string[] {"C", "B", "M", "D", "B", "M", "B", "B", "D", "E", "H", "A", "C", "L", "L", "B", "M", "B", "M", "L", "D", "A", "G", "D"},
		new string[] {"D", "A", "C", "D", "B", "A", "A", "H", "J", "C", "B", "M", "D", "B", "M", "B", "B", "D", "E", "H", "A", "C", "L", "L", "B", "M", "B", "M", "L", "D", "A", "G", "D"},
		new string[] {"D", "E", "A", "B", "D", "I", "B", "E", "G", "D", "D", "E", "D", "D", "E", "K", "A", "B", "D", "A", "H", "B", "B", "B"},
		new string[] {"A", "M", "A", "C", "D", "B", "E", "M", "E", "K", "L", "E", "M", "H", "M", "M", "C", "F", "E", "B", "D", "H", "B", "D"},
		new string[] {"D", "M", "C", "M", "I", "A", "I", "C", "D", "L", "H", "E", "H", "M", "B", "M", "D", "C", "E", "D", "A", "B", "E", "E"},
		new string[] {"E", "M", "B", "B", "A", "C", "H", "L", "M", "H", "L", "A", "C", "G", "D", "A", "C", "J", "D", "M", "C", "M", "I", "A", "I", "C", "D", "L", "H", "E", "H", "M", "B", "M", "D", "C", "E", "D", "A", "B", "E", "E"},
		new string[] {"B", "M", "G", "F", "D", "I", "B", "A", "B", "F", "D", "M", "M", "M", "G", "B", "B", "C", "D", "B", "A", "E", "D", "F"},
		new string[] {"C", "C", "E", "C", "G", "D", "E", "G", "C", "A", "D", "E", "G", "G", "M", "D", "D", "A", "B", "I", "K", "B", "H", "E"},
		new string[] {"D", "L", "F", "H", "K", "G", "D", "A", "A", "D", "M", "L", "B", "L", "L", "B", "B", "G", "D", "L", "D", "A", "L", "B"},
		new string[] {"E", "D", "I", "B", "D", "B", "C", "G", "M", "G", "D", "A", "D", "D", "B", "D", "H", "B", "E", "B", "M", "B", "B", "M"}
	};

	string FullSerial;
	int SerialLastDigit;
	int PortCount;
	int BatteryCount;

	int CurrentRow = 0;
	int CurrentPress = 0;
	List<int> PressedSequence = new List<int>();

	int LastStageFirstPress;

	int ThisStageLastPress = -1;
	int NextStageLastPress = -1;

	static int ModuleIdCounter = 1;
	int ModuleId;
	private bool ModuleSolved;

	bool ModuleActive = false;

	void Awake () {
		ModuleId = ModuleIdCounter++;
		GetComponent<KMBombModule>().OnActivate += Activate;

		BombInfo = GetComponent<KMBombInfo>();
		Audio = GetComponent<KMAudio>();

		foreach (KMSelectable btn in Buttons) {
			btn.OnInteract += delegate () { btnPress(btn); return false; };
		}
	}

	void Start () {
		FullSerial = BombInfo.GetSerialNumber();
		SerialLastDigit = int.Parse(FullSerial.Substring(5, 1));
		PortCount = BombInfo.GetPortCount();
		BatteryCount = BombInfo.GetBatteryCount();

		AllTexts[0].text = "";
		AllTexts[1].text = "";
		AllTexts[2].text = "";
	}

	void Activate () {
		StartCoroutine(PrepareStage(true));
	}

	void btnPress (KMSelectable pressed_btn) {
		pressed_btn.AddInteractionPunch(0.3f);

		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed_btn.transform);

		if (ModuleSolved || !ModuleActive) {
			return;
		}

		int btn_ind = 0;
		foreach (KMSelectable btn in Buttons) {
			if (btn == pressed_btn) break;
			btn_ind++;
		}

		Debug.LogFormat("[Baby Who's on First #{0}] Pressed {1} button for press #{2}.",
			ModuleId, POS[btn_ind], CurrentPress+1);

		int current_instruction = InstructionSequences[CurrentRow][CurrentPress];
		string current_letter = InstructionLetters[CurrentRow][CurrentPress];

		// check if this is the last instruction AND there's an active K instruction from last time
		if (CurrentPress == InstructionLetters[CurrentRow].Length - 1 && ThisStageLastPress != -1) {
			Debug.LogFormat("[Baby Who's on First #{0}] Last instruction of the stage, and there was a K last time. The {1} button was pressed then. The {2} button must be pressed.",
				ModuleId, POS[1-ThisStageLastPress], POS[ThisStageLastPress]);

			if (btn_ind != ThisStageLastPress) {
				Strike();
				return;
			}
			
		} else switch (current_instruction) {
			// parse instruction and write down pressed answer if correct

			case 0: // A: top button
				Debug.LogFormat("[Baby Who's on First #{0}] Instruction #{1} is A. The top button must be pressed.",
					ModuleId, CurrentPress+1);

				if (btn_ind != 0) {
					Strike();
					return;
				}

				break;
			
			case 1: // B: bottom button
				Debug.LogFormat("[Baby Who's on First #{0}] Instruction #{1} is B. The bottom button must be pressed.",
					ModuleId, CurrentPress+1);

				if (btn_ind != 1) {
					Strike();
					return;
				}
				
				break;

			case 2: // C: top if last serial digit is odd, bottom if not
				int odd_check = 1 - (SerialLastDigit % 2);

				Debug.LogFormat("[Baby Who's on First #{0}] Instruction #{1} is C. The last serial digit is {2}. The {3} button must be pressed.",
					ModuleId, CurrentPress+1, SerialLastDigit, POS[odd_check]);
				
				if (btn_ind != odd_check) {
					Strike();
					return;
				}
				
				break;

			case 3: // D: bottom if ports > batteries, top if not
				int port_check = Convert.ToInt32(PortCount > BatteryCount);

				Debug.LogFormat("[Baby Who's on First #{0}] Instruction #{1} is D. The port count is {2} and the battery count is {3}. The {4} button must be pressed.",
					ModuleId, CurrentPress+1, PortCount, BatteryCount, POS[port_check]);
				
				if (btn_ind != port_check) {
					Strike();
					return;
				}

				break;
			
			case 4: // E: either button
				Debug.LogFormat("[Baby Who's on First #{0}] Instruction #{1} is E. Either button can be pressed.",
					ModuleId, CurrentPress+1);
				break;
			
			case 5: // F: first in prev stage; if this is stage 1, top
				if (Stage > 0) {
					Debug.LogFormat("[Baby Who's on First #{0}] Instruction #{1} is F. Last stage, the first press was the {2} button. The {2} button must be pressed.",
					ModuleId, CurrentPress+1, POS[LastStageFirstPress]);

					if (btn_ind != LastStageFirstPress) {
						Strike();
						return;
					}
					
				} else {
					Debug.LogFormat("[Baby Who's on First #{0}] Instruction #{1} is F. There is no previous stage. The top button must be pressed.",
					ModuleId, CurrentPress+1);

					if (btn_ind != 0) {
						Strike();
						return;
					}
				}

				break;

			case 6: // G: least pressed so far this stage
				int[] presses = {0, 0};

				for (int i = 0; i < CurrentPress; i++) {
					presses[PressedSequence[i]]++;
				}

				if (presses[0] == presses[1]) {
					Debug.LogFormat("[Baby Who's on First #{0}] Instruction #{1} is G. Both buttons have been pressed {2} time{3}. Either button can be pressed.",
						ModuleId, CurrentPress+1, presses[0], (presses[0] != 1) ? "s" : "");

				} else {
					int min_ind = (presses[0] > presses[1]) ? 1 : 0;
					Debug.LogFormat("[Baby Who's on First #{0}] Instruction #{1} is G. The top button has been pressed {2} time{3}. The bottom button has been pressed {4} time{5}. The {6} button must be pressed.",
						ModuleId, CurrentPress+1, presses[0], (presses[0] != 1) ? "s" : "", presses[1], (presses[1] != 1) ? "s" : "", POS[min_ind]);
				
					if (presses[btn_ind] > presses[1-btn_ind]) {
						Strike();
						return;
					}
				}

				break;
			
			case 7: // H: bottom if instruction is in first half; otherwise, top
				int relevant_row = CurrentRow;
				int half_check = Convert.ToInt32(CurrentPress < SEQUENCE_LENGTH/2); // default check (no J interaction)

				if (CurrentRow == 5) { // HARDCODED J checks
					half_check = (CurrentPress <= 20) ? 1 : 0;
					relevant_row--;
				}
				if (CurrentRow == 9) {
					half_check = (CurrentPress <= 11 || (CurrentPress >= 18 && CurrentPress <= 29)) ? 1 : 0;
					relevant_row--;
				}

				Debug.LogFormat("[Baby Who's on First #{0}] Instruction #{1} is H. The current instruction is in the {2} half of row {3}. The {4} button must be pressed.",
					ModuleId, CurrentPress+1, (half_check == 1) ? "first" : "second", relevant_row+1, POS[half_check]);
				
				if (btn_ind != half_check) {
					Strike();
					return;
				}
				
				break;
			
			case 8: // I: even timer -> top, odd timer -> bottom
				int current_time = (int)BombInfo.GetTime();

				int minutes = current_time/60;
				int seconds = current_time%60;

				int timer_check = seconds % 2;

				Debug.LogFormat("[Baby Who's on First #{0}] Instruction #{1} is I. The current bomb time is {2}:{3}{4}. The {5} button must be pressed.",
					ModuleId, CurrentPress+1, minutes, (seconds < 10) ? "0" : "", seconds, POS[timer_check]);

				if (btn_ind != timer_check) {
					Strike();
					return;
				}

				break;
			
			case 9: // J: bottom button. Other logic is handled elsewhere
				Debug.LogFormat("[Baby Who's on First #{0}] Instruction #{1} is J. The bottom button must be pressed. Restarting sequence in the row above (row {2}, label {3}).",
					ModuleId, CurrentPress+1, CurrentRow, ButtonWords[CurrentRow-1]);

				if (btn_ind != 1) {
					Strike();
					return;
				}

				break;
			
			case 10: // K: either button. Remember opposite press for next stage
				Debug.LogFormat("[Baby Who's on First #{0}] Instruction #{1} is K. Either button can be pressed. Next stage, the last button pressed must be the opposite of this one.",
					ModuleId, CurrentPress+1);
				
				NextStageLastPress = 1 - btn_ind;
				break;
			
			case 11: // L: top button if display matches top button; bottom if not
				int disp_match_check = 1;

				string T_BTN = ButtonWords[CurrentButtons[0]];
				string DISP = DisplayWords[CurrentDisplay];

				for (int i = 0; i < T_BTN.Length; i++) {
					if (DISP.IndexOf(T_BTN[i]) != -1) {
						disp_match_check = 0;
						break;
					}
				}

				Debug.LogFormat("[Baby Who's on First #{0}] Instruction #{1} is L. Display is {2}, top button is {3}. The {4} button must be pressed.",
					ModuleId, CurrentPress+1, DISP, T_BTN, POS[disp_match_check]);

				if (btn_ind != disp_match_check) {
					Strike();
					return;
				}

				break;
			
			case 12: // M: bottom button if serial matches bottom button; top if not
				int ser_match_check = 0;

				string B_BTN = ButtonWords[CurrentButtons[1]];

				for (int i = 0; i < B_BTN.Length; i++) {
					if (FullSerial.IndexOf(B_BTN[i]) != -1) {
						ser_match_check = 1;
						break;
					}
				}

				Debug.LogFormat("[Baby Who's on First #{0}] Instruction #{1} is M. Serial number is {2}, bottom button is {3}. The {4} button must be pressed.",
					ModuleId, CurrentPress+1, FullSerial, B_BTN, POS[ser_match_check]);

				if (btn_ind != ser_match_check) {
					Strike();
					return;
				}

				break;
		}

		Debug.LogFormat("[Baby Who's on First #{0}] Correct!", ModuleId);
		PressedSequence.Insert(CurrentPress, btn_ind);
		CurrentPress++;

		if (CurrentPress >= InstructionSequences[CurrentRow].Length) {
			OffLEDs[Stage].enabled = false;
			OnLEDs[Stage].enabled = true;

			// Cycle the memory for rule K
			ThisStageLastPress = NextStageLastPress;
			NextStageLastPress = -1; 

			Stage++;
			Debug.LogFormat("[Baby Who's on First #{0}] Stage {1} solved!", ModuleId, Stage);
			LastStageFirstPress = PressedSequence[0];

			if (Stage == 3) {
				Solve();
			} else {
				StartCoroutine(PrepareStage(false));
			}
		}
	}

	IEnumerator PrepareStage (bool skip_animation) {
		Debug.LogFormat("[Baby Who's on First #{0}] Initiating stage {1}.", ModuleId, Stage+1);

		if (!skip_animation) {
			ModuleActive = false;

			AllTexts[0].text = "";
			yield return new WaitForSeconds (1.1f);

			AllTexts[1].text = "";
			ButtonMeshes[0].enabled = false;
			ButtonHighlights[0].transform.localScale = new Vector3(0, 0, 0);
			yield return new WaitForSeconds (0.3f);

			AllTexts[2].text = "";
			ButtonMeshes[1].enabled = false;
			ButtonHighlights[1].transform.localScale = new Vector3(0, 0, 0);
			yield return new WaitForSeconds (0.6f);
		}

		

		// Reset press counter
		CurrentPress = 0;

		// Update display
		CurrentDisplay = Rnd.Range(0, DisplayWords.Length);
		AllTexts[0].text = DisplayWords[CurrentDisplay];

		if (!skip_animation) {
			yield return new WaitForSeconds (0.4f);
		}

		// Update top button
		CurrentButtons[0] = Rnd.Range(0, ButtonWords.Length);
		AllTexts[1].text = ButtonWords[CurrentButtons[0]];

		if (!skip_animation) {
			ButtonMeshes[0].enabled = true;
			ButtonHighlights[0].transform.localScale = new Vector3(1.05f, 1.15f, 0.5f);
			yield return new WaitForSeconds (0.4f);
		}

		// Update bottom button (must be different from top)
		CurrentButtons[1] = Rnd.Range(0, ButtonWords.Length-1);
		if (CurrentButtons[1] >= CurrentButtons[0]) CurrentButtons[1]++;
		AllTexts[2].text = ButtonWords[CurrentButtons[1]];

		if (!skip_animation) {
			ButtonMeshes[1].enabled = true;
			ButtonHighlights[1].transform.localScale = new Vector3(1.05f, 1.15f, 0.5f);
		}

		ModuleActive = true;

		int ReferencePos = ReferenceLabels[CurrentDisplay]; // Is the reference for this display top or bottom?
		CurrentRow = CurrentButtons[ReferencePos]; // Index of the reference label in the labels list
		int InstructionPtr = 0; // Current instruction index being parsed

		// Log all info
		Debug.LogFormat("[Baby Who's on First #{0}] Display word is {1}. Buttons are {2} and {3}.",
			ModuleId, DisplayWords[CurrentDisplay], ButtonWords[CurrentButtons[0]], ButtonWords[CurrentButtons[1]]);
		
		Debug.LogFormat("[Baby Who's on First #{0}] Reference button is the {1} button, label {2}.",
			ModuleId, POS[ReferencePos], ButtonWords[CurrentRow]);
		
		Debug.LogFormat("[Baby Who's on First #{0}] Full sequence of instructions (with instruction J accounted for): {1}",
			ModuleId, string.Join(", ", InstructionLetters[CurrentRow]));

		// Clear press sequence buffer
		PressedSequence.Clear();

		yield return null;
	}

	void OnDestroy () {
		
	}

	void Update () {

	}

	void Solve () {
		GetComponent<KMBombModule>().HandlePass();
	}

	void Strike () {
		Debug.LogFormat("[Baby Who's on First #{0}] Incorrect! Striking and resetting stage {1}.",
			ModuleId, Stage+1);
		
		NextStageLastPress = -1; // if a K set up an upcoming memory press this stage, reset it alongside the strike

		GetComponent<KMBombModule>().HandleStrike();
		StartCoroutine(PrepareStage(false));
	}

#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"Use !{0} 0/1/2 to press buttons. 0 is the top button, 1 is the bottom button, 2 will automatically press top/button according to the current timer parity. Chain commands by separating them with spaces.";
#pragma warning restore 414

	IEnumerator ProcessTwitchCommand (string Command) {
		Command = Command.Trim().ToUpper();
		yield return null;

		string[] Presses = Command.Split(' ');

		for (int i = 0; i < Presses.Length; i++) {
			if (!"012".Contains(Presses[i]) || Presses[i].Length != 1) {
				yield return "sendtochaterror Invalid button press!";
				yield break;
			}
		}

		for (int i = 0; i < Presses.Length; i++) {
			switch (Presses[i]) {
				case "0":
					btnPress(Buttons[0]);
					break;
				case "1":
					btnPress(Buttons[1]);
					break;
				case "2":
					btnPress(Buttons[((int)BombInfo.GetTime()) % 2]);
					break;
			}
			yield return new WaitForSeconds(.1f);
		}
	}

	IEnumerator TwitchHandleForcedSolve () {
		Solve();
		yield return null;
	}
}
