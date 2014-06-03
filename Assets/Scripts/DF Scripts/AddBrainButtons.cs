﻿using UnityEngine;
using System.Collections;
using Parse;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;

public class AddBrainButtons : MonoBehaviour {

    private dfPanel _panel;
    private IEnumerable<ParseObject> brains;
    private bool DataLoaded = false;
    private bool ExpandedState = false;

    private List<Brain> rootBrains = new List<Brain>();
    private IEnumerable<Brain> allBrains = new List<Brain>();

    private dfPanel slot1, slot2, slot3, slot4;
    private Color32 slot1Color, slot2Color, slot3Color, slot4Color;
    private Color32 grey = new Color32(100, 100, 100, 255);

    private dfPanel activeSlot;

    private Brain draggedBrain;

    public dfButton BattleButton;

    public Texture2D mouseCursor;
    private DialogShow _dialog;

    Player Player
    {
        get
        {
            return ParseUser.CurrentUser as Player;
        }
    }

	// Use this for initialization
    void Start()
    {
        

        this._panel = GetComponent<dfPanel>();
        _dialog = GameObject.Find("Dialog").GetComponent<DialogShow>();
        BattleButton.Disable();
        //   _panel.transform.Find("NewBrainButton").GetComponent<dfButton>().Click += button_Click;
        
        var q = new ParseQuery<Brain>().WhereEqualTo("userId", ParseUser.CurrentUser.ObjectId).OrderBy("createdAt");
        q.FindAsync().ContinueWith(t =>
            {
                IEnumerable<Brain> result = t.Result;
           //     Dictionary<string, Brain> allBrains = new Dictionary<string, Brain>();

                foreach (Brain brain in result)
                {
                    if (brain.Population != null)
                    {
                        print("FILE: " + brain.Population.Name + ", URL: " + brain.Population.Url);
                        
                      //  StartCoroutine(WaitForRequest(brain));
                    }
                    if (brain.ParentId != null)
                    {
                        result.Where(b => b.ObjectId == brain.ParentId).First().Children.Add(brain);
                    }
                    else
                    {
                        rootBrains.Add(brain);
                    }
                }
                allBrains = result;

                DataLoaded = true;
            });

        GameObject.Find("Back Button").GetComponent<dfButton>().Click += AddBrainButtons_Click;

        slot1 = GameObject.Find("Slot 1").GetComponent<dfPanel>();
        slot1Color = slot1.BackgroundColor;
        slot2 = GameObject.Find("Slot 2").GetComponent<dfPanel>();
        slot2Color = slot2.BackgroundColor;
        slot3 = GameObject.Find("Slot 3").GetComponent<dfPanel>();
        slot3Color = slot3.BackgroundColor;
        slot4 = GameObject.Find("Slot 4").GetComponent<dfPanel>();
        slot4Color = slot4.BackgroundColor;

       
        slot1.DragDrop += slot1_DragDrop;
        slot1.DragEnter += slot1_DragEnter;
        slot1.DragLeave += slot1_DragLeave;

        slot2.DragDrop += slot1_DragDrop;
        slot2.DragEnter += slot1_DragEnter;
        slot2.DragLeave += slot1_DragLeave;

        slot3.DragDrop += slot1_DragDrop;
        slot3.DragEnter += slot1_DragEnter;
        slot3.DragLeave += slot1_DragLeave;

        slot4.DragDrop += slot1_DragDrop;
        slot4.DragEnter += slot1_DragEnter;
        slot4.DragLeave += slot1_DragLeave;

        
    }

    private string GetNameFromBrain(string objectId)
    {
        foreach (Brain b in allBrains)
        {
            if (b.ObjectId.Equals(objectId))
            {
                return b.Name;
            }
        }
        return null;
    }

    void slot1_DragLeave(dfControl control, dfDragEventArgs dragEvent)
    {
        dfPanel panel = control as dfPanel;
        panel.GetComponent<BrainPanelState>().Refresh();
        //switch (control.gameObject.name)
        //{
        //    case "Slot 1":
        //        panel.BackgroundColor = slot1Color;
        //        break;
        //    case "Slot 2":
        //        panel.BackgroundColor = slot2Color;
        //        break;
        //    case "Slot 3":
        //        panel.BackgroundColor = slot3Color;
        //        break;
        //    case "Slot 4":
        //        panel.BackgroundColor = slot4Color;
        //        break;
        //}
    }

    void slot1_DragEnter(dfControl control, dfDragEventArgs dragEvent)
    {
        dfPanel panel = control as dfPanel;
        print("Drag enter");
        if (draggedBrain != null)
        {
            panel.BackgroundColor = new Color32(255, 255, 255, 255);
            activeSlot = panel;
        }
    }

    void slot1_DragDrop(dfControl control, dfDragEventArgs dragEvent)
    {
        dfPanel panel = control as dfPanel;

        BrainPanelState brainPanel = panel.GetComponent<BrainPanelState>();

        if (draggedBrain != null)
        {
            if (draggedBrain.ChampionGene == null)
            {
                _dialog.ShowDialog("This brain has not been trained yet and cannot be used in battle mode.");
                brainPanel.Refresh();
                return;
            }
           
            brainPanel.AddBrain(draggedBrain);
            BattleButton.Enable();
        }       
    }

    void OnDragEnd()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        
    }

    void button_DragStart(dfControl control, dfDragEventArgs dragEvent)
    {
        BrainButton bb = control as BrainButton;
        draggedBrain = bb.Brain;
        dragEvent.State = dfDragDropState.Dragging;
        dragEvent.Data = this;
        Cursor.SetCursor(mouseCursor, Vector2.zero, CursorMode.Auto);
        print("Drag start");
    }


    IEnumerator WaitForRequest(Brain brain)
    {
        WWW www = new WWW(brain.ChampionGene.Url.AbsoluteUri);
        print("Downloading");
        yield return www;
        print("Done downloading");

        string folderPath = Application.persistentDataPath + string.Format("/{0}", ParseUser.CurrentUser.Username);
        DirectoryInfo dirInf = new DirectoryInfo(folderPath);
        if (!dirInf.Exists)
        {
            Debug.Log("Creating subdirectory");
            dirInf.Create();
        }
        string popFilePath = Application.persistentDataPath + string.Format("/{0}/{1}.champ.xml", ParseUser.CurrentUser.Username, brain.ObjectId);
        File.WriteAllText(popFilePath, www.text);
    }

    void AddBrainButtons_Click(dfControl control, dfMouseEventArgs mouseEvent)
    {
        if (ExpandedState)
        {
            AddBrains(rootBrains);
            ExpandedState = false;
        }
    }

    private float HypotenuseLength(float pc, float c, float B)
    {
        float A = pc - c;
        return Mathf.Sqrt(A * A + B * B);
    }

    private float Angle(float a, float b)
    {
        float angle = Mathf.Atan(b / a) * Mathf.Rad2Deg;
        return angle;
    }

    private float AddButton(Brain b, int level, float ratio, float idx, float prev_center)
    {
        var spacing = 90;
        var count = _panel.Controls.Count;
        var button = _panel.AddControl<BrainButton>();
        button.BackgroundSprite = "listitem-selected";
        button.HoverSprite = "listitem-hover";
        button.PressedSprite = "listitem-selected";
        button.Text = level + ". " + b.Name;
        button.Width = _panel.Width * ratio - 10;
        button.Height = 48;
        button.RelativePosition = new Vector3(_panel.Width * idx + 5, level * spacing);
        button.IsNewBrain = b.IsNewBrain;
        button.Brain = b;
        button.ZOrder = 10;
        button.Click += button_Click;
     //   button.MouseHover += button_MouseHover;
      //  button.MouseLeave += button_MouseLeave;
        
        button.DragStart += button_DragStart;

        float center = _panel.Width * idx + button.Width / 2;

        if (level > 0)
        {
            var line = _panel.AddControl<dfSprite>();
            line.SpriteName = "vslider-track-normal";
            line.Width = 12;
            //line.Height = spacing - button.Height;
            float B = spacing - button.Height;
            line.Height = HypotenuseLength(prev_center, center, B);
            Vector3 rot = Vector3.forward;
            float angle = Angle(B, center - prev_center);
            print("Angle: " + angle);
            line.transform.Rotate(rot, angle);
            float padding = 0;
            if (angle < 0)
            {
                padding = -5;
            }
            if (angle > 0)
            {
                padding = 5;
            }
            line.RelativePosition = new Vector3(prev_center, level * spacing - B + padding);
            line.ZOrder = 0;

            var expand = button.AddControl<BrainButton>();
            expand.BackgroundSprite = "dropdown-btn-normal";
            expand.HoverSprite = "dropdown-btn-hover";
            expand.DisabledSprite = "button-disabled";
            expand.PressedSprite = "dropdown-btn-normal";
            //expand.Text = "+";
            expand.Width = 25;
            expand.Height = 25;
            expand.RelativePosition = new Vector3(5, 10);
            expand.Brain = b;
            expand.Click += expand_click;
        }

        var plus = button.AddControl<BrainButton>();
        plus.BackgroundSprite = "button-normal";
        plus.HoverSprite = "button-hover";
        plus.DisabledSprite = "button-disabled";
        plus.PressedSprite = "button-normal";
        plus.Text = "New";
        plus.Width = 40;
        plus.Height = 25;
        plus.RelativePosition = new Vector3(button.Width - plus.Width - 5, 10);
        plus.Brain = b;
        plus.Click += new_button_Click;

        

        return center;
    }

    

    
    private void expand_click(dfControl control, dfMouseEventArgs mouseEvent)
    {
        BrainButton bb = control as BrainButton;
        List<Brain> brains = new List<Brain>() { bb.Brain };
        mouseEvent.Use();
        ExpandedState = true;
        AddBrains(brains);
    }

    void button_MouseLeave(dfControl control, dfMouseEventArgs mouseEvent)
    {
        BrainButton but = control as BrainButton;
        but.Width = but.OriginalWidth;
        
        but.ZOrder = 10;
    }

    void button_MouseHover(dfControl control, dfMouseEventArgs mouseEvent)
    {
        BrainButton but = control as BrainButton;
        but.OriginalWidth = but.Width;
        but.Width = 400;
        but.ZOrder = 50;
    }

    void button_Click(dfControl control, dfMouseEventArgs mouseEvent)
    {
        //print("Button clicked");
        BrainButton bb = control as BrainButton;
        Settings.Brain = bb.Brain;
    //    Settings.IsNewBrain = bb.IsNewBrain;
        Application.LoadLevel("Training Overview");
        
    }

    void new_button_Click(dfControl control, dfMouseEventArgs mouseEvent)
    {
        BrainButton bb = control as BrainButton;
        
        Settings.Brain = bb.Brain.Branch();
      
        Application.LoadLevel("Training Overview");
        
        //    Application.LoadLevel("Training Overview");
            mouseEvent.Use();
    }
	
	// Update is called once per frame
	void Update () {
        if (DataLoaded)
        {
            foreach (Brain brain in allBrains)
            {
                if (brain.ChampionGene != null)
                {
                    print("FILE: " + brain.Population.Name + ", URL: " + brain.Population.Url);

                    StartCoroutine(WaitForRequest(brain));
                }
            }
            if (rootBrains.Count == 0)
            {
                Brain b = new Brain();
                b.Name = "New brain";
                b.UserId = ParseUser.CurrentUser.ObjectId;
                b.IsNewBrain = true;
                rootBrains.Add(b);
            }
            AddBrains(rootBrains);
            slot1.GetComponent<BrainPanelState>().Initialize();
            //Player player = ParseUser.CurrentUser as Player;
            //if (player.Slot1 != null)
            //{
            //    slot1.GetComponent<BrainPanelState>().AddBrain(allBrains.Where(t => t.ObjectId.Equals(player.Slot1)).First());
            //}
            //if (player.Slot2 != null)
            //{
            //    slot2.GetComponent<BrainPanelState>().AddBrain(allBrains.Where(t => t.ObjectId.Equals(player.Slot2)).First());
            //}
            //if (player.Slot3 != null)
            //{
            //    slot3.GetComponent<BrainPanelState>().AddBrain(allBrains.Where(t => t.ObjectId.Equals(player.Slot3)).First());
            //}
            //if (player.Slot4 != null)
            //{
            //    slot4.GetComponent<BrainPanelState>().AddBrain(allBrains.Where(t => t.ObjectId.Equals(player.Slot4)).First());
            //}            
        }
	}

    void AddBrains(List<Brain> brains)
    {
        var children = new List<GameObject>();

        foreach (Transform child in transform)
        {
            children.Add(child.gameObject);
        }
        children.ForEach(child => Destroy(child));

        GameObject.Find("MainPanel").GetComponent<dfPanel>().Enable();

     //   GameObject.Find("LoadingLabel").GetComponent<dfLabel>().IsVisible = false;
        DataLoaded = false;
        int level = 0;
        int idx = 0;
        float ratio = 1.0f / brains.Count;

        foreach (Brain b in brains)
        {
            recurseAdd(b, level, ratio, idx, idx * ratio, 0);
            idx++;
        }
    }

    void recurseAdd(Brain b, int level, float ratio, int idx, float startRatio, float prev_center)
    {
        float center = AddButton(b, level, ratio, startRatio, prev_center);
        print(level + ". " + b.Name + ", Startratio: " + startRatio + ", center: " + center);
        
        if (b.Children.Count > 0)
        {
            int count = b.Children.Count;
            float r = ratio / count;
            int i = 0;
            foreach (Brain child in b.Children)
            {
                recurseAdd(child, level + 1, r, i, startRatio + i * r, center);
                i++;
            }
        }
        
        
    }
}