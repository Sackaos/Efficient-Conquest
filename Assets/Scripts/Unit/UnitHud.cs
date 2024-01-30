using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.UIElements;

public class UnitHud : MonoBehaviour
{
    [Header("UI")]

    public GameObject ActionGrid;
    public GameObject ButtonPrefab;
    Unit UnitToDisplay;
    Button[] Buttons = new Button[15];

    [Header("Misc")]
    public UnitStats temproaryUnitToSpawn;
    private Transform a;
    //[field: SerializeField, Header("Hiya")]



    public static UnitHud UnitHudSingleton { get; private set; }


    private void Awake()
    {
        if(!UnitHudSingleton) {
            UnitHudSingleton = this;
        } else {
            Debug.LogError("BAUBAU! UnitHudSingleton not SINGLETON");
            Destroy(this);
        }

        Button CurButton;
        for(int i = 0; i < 15; i++) {
            //instantiate ui at parent.width/5*j+XSpacing*j,parent.height/3*i+YSpacing*i

            CurButton = Instantiate(ButtonPrefab, ActionGrid.transform, false).GetComponent<Button>();
            Buttons[i] = CurButton;

            int index = i;
            CurButton.onClick.AddListener(() =>
            {
                DoFunc(index);
            });
            DisableButton(i);
        }
        a = transform;


    }

    public void DisplayUnit(Unit unit)
    {
        UnitToDisplay = unit;
        ClearGrid();
        if(unit != null)
            FillGrid();


    }
    private void DoFunc(int actionID)
    {
        Unit.Actions action = Unit.Actions.Empty;
        for(int i = 0; i < UnitToDisplay.stats.Actions.Length; i++)//EADITING THIS LINE
        {
            if(UnitToDisplay.stats.Actions[i].id == actionID) {

                action = UnitToDisplay.stats.Actions[i].action;
                break;
            }
        }
        switch(action) {
            case Unit.Actions.Empty:
                Debug.LogError("OhNyoooo not supposed to be:(  unitHud Switch(Action)");
                break;
            case Unit.Actions.Move:
                break;
            case Unit.Actions.Attack:
                break;
            case Unit.Actions.Build:
                break;
            case Unit.Actions.SpawnFunkyStuff:
                GameCoordinator.GameCoordinatorSingleton.SpawnUnit(temproaryUnitToSpawn, a);
                break;
            default:
                break;
        }
    }
    private void FillGrid()
    {
        foreach(var action in UnitToDisplay.stats.Actions) {
            EnableButton(action.id, action.action + "");
        }
    }
    private void ClearGrid()
    {
        for(int i = 0; i < Buttons.Length; i++) {
            DisableButton(i);
        }

    }

    private void EnableButton(int i, String text)
    {
        Buttons[i].GetComponent<Button>().enabled = true;
        Buttons[i].GetComponent<Image>().enabled = true;
        Buttons[i].GetComponentInChildren<TMP_Text>().text = text;
    }

    private void DisableButton(int i)
    {
        //Buttons[i].enabled = false;
        Buttons[i].GetComponent<Button>().enabled = false;
        Buttons[i].GetComponent<Image>().enabled = false;
        Buttons[i].GetComponentInChildren<TMP_Text>().text = "";
    }

}
