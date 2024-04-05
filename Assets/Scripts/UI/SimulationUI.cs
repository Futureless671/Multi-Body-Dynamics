using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class SimulationUI : MonoBehaviour
{
    private bool newbody = false;
    private VisualElement root;
    public VisualTreeAsset bodyentrybutton;
    private VisualElement viewwindow;
    private SimController controller;
    public Body template;
    public Body editorbody;
    private Slider redslider;
    private TextField namefield;
    private float redvalue = 255f;
    private Slider greenslider;
    private float greenvalue = 255f;
    private Slider blueslider;
    private float bluevalue = 255f;
    private Color color;
    private FloatField radiusfield;
    private float bodyradius;
    private FloatField massfield;
    private float mass;
    private FloatField pradiusfield;
    private float periapsisradius;
    private FloatField pvelocityfield;
    private float periapsisvelocity;
    private bool editoractive;
    private Button open_settings;
    private Button open_bodymenu;
    private Button new_body;
    private VisualElement body_view;
    private VisualElement body_editor;
    private Button body_editor_done;
    private Button body_editor_delete;
    private VisualElement rightsidebar;
    private VisualElement leftsidebar;
    private DropdownField TimeScaleUnitsField;
    private FloatField TimeScaleValueField;
    private float TimeScaleValue = 1;
    private float TimeScaleUnits = 1;
    private bool enablerightsidebar = true;
    private bool enableleftsidebar = false;
    private float BodyScalePower = 1f;
    private Slider BodyScalePowerSlider;
    private float BodyScaleMin = 0.1f;
    private Slider BodyScaleMinSlider;
    private List<VisualElement> BodyScalePlotPoints = new List<VisualElement>();
    private VisualElement bodyscaleplot;
    private int plotwidth;
    // Start is called before the first frame update
    void Start()
    {
        controller = FindObjectOfType<SimController>();
        root = GetComponent<UIDocument>().rootVisualElement;
        open_bodymenu = root.Q<Button>("right-open-button");
        open_settings = root.Q<Button>("left-open-button");
        new_body = root.Q<Button>("New-Body");
        rightsidebar = root.Q<VisualElement>("RightSidebar");
        leftsidebar = root.Q<VisualElement>("LeftSidebar");
        viewwindow = root.Q<VisualElement>("View");
        body_view = root.Q<VisualElement>("Body-View");
        body_editor = root.Q<VisualElement>("Body-Editor");
        body_editor_done = root.Q<Button>("Done-Button");
        body_editor_delete = root.Q<Button>("Delete-Button");
        namefield = root.Q<TextField>("Name");
        redslider = root.Q<Slider>("Red-Slider");
        greenslider = root.Q<Slider>("Green-Slider");
        blueslider = root.Q<Slider>("Blue-Slider");
        radiusfield = root.Q<FloatField>("Body-Radius");
        massfield = root.Q<FloatField>("Body-Mass");
        pradiusfield = root.Q<FloatField>("Periapsis-Radius");
        pvelocityfield = root.Q<FloatField>("Periapsis-Velocity");
        TimeScaleUnitsField = root.Q<DropdownField>("TimeScaleUnits");
        TimeScaleValueField = root.Q<FloatField>("TimeScale");
        BodyScalePowerSlider = root.Q<Slider>("BodyScalePowerSlider");
        BodyScaleMinSlider = root.Q<Slider>("MinSizeSlider");
        bodyscaleplot = root.Q<VisualElement>("PlotView");

        plotwidth = 256;
        
        for(int i = 0; i<=plotwidth; i++)
        {
            VisualElement pltpnt = new VisualElement();
            pltpnt.name = "PlotPoint"+i;
            pltpnt.AddToClassList("BodyScalePlotPoint");
            bodyscaleplot.Add(pltpnt);
            BodyScalePlotPoints.Add(pltpnt);
            float ht = 4;
            float wd = 4;
            pltpnt.style.position = Position.Absolute;
            pltpnt.style.bottom = -ht/2;
            pltpnt.style.left = i - wd/2;
        }

        open_bodymenu.RegisterCallback<ClickEvent>(ToggleRightSidebar);
        open_settings.RegisterCallback<ClickEvent>(ToggleLeftSidebar);
        new_body.RegisterCallback<ClickEvent>(CreateBody);
        body_editor_done.RegisterCallback<ClickEvent>(editbodydone);
        body_editor_delete.RegisterCallback<ClickEvent>(editbodydelete);

        redslider.RegisterValueChangedCallback(v =>
        {
            redvalue = v.newValue;
        });
        greenslider.RegisterValueChangedCallback(v =>
        {
            greenvalue = v.newValue;
        });
        blueslider.RegisterValueChangedCallback(v =>
        {
            bluevalue = v.newValue;
        });
        radiusfield.RegisterValueChangedCallback(v =>
        {
            bodyradius = v.newValue;
        });
        massfield.RegisterValueChangedCallback(v =>
        {
            mass = v.newValue;
            controller.resetsim();
        });
        pradiusfield.RegisterValueChangedCallback(v =>
        {
            periapsisradius = v.newValue;
            controller.resetsim();
        });
        pvelocityfield.RegisterValueChangedCallback(v =>
        {
            periapsisvelocity = v.newValue;
            controller.resetsim();
        });
        TimeScaleUnitsField.RegisterValueChangedCallback(v =>
        {
            switch(TimeScaleUnitsField.index)
            {
                case 0:
                    TimeScaleUnits = 1;
                    break;
                case 1:
                    TimeScaleUnits = 60;
                    break;
                case 2:
                    TimeScaleUnits = 3600;
                    break;
                case 3:
                    TimeScaleUnits = 86400;
                    break;
                case 4:
                    TimeScaleUnits = 31557600;
                    break;
                default:
                    TimeScaleUnits = 1;
                    break;
            }
            controller.TimeScale = TimeScaleValue*TimeScaleUnits;
        });
        TimeScaleValueField.RegisterValueChangedCallback(v =>
        {
            TimeScaleValue = v.newValue;
            controller.TimeScale = TimeScaleValue*TimeScaleUnits;
        });
        BodyScalePowerSlider.RegisterValueChangedCallback(v =>
        {
            BodyScalePower = Mathf.Pow(10,2*v.newValue-1);
            plotbodyscale();
        });
        BodyScaleMinSlider.RegisterValueChangedCallback(v =>
        {
            BodyScaleMin = v.newValue;
            plotbodyscale();
        });
        plotbodyscale();

    }

    private void plotbodyscale()
    {
        float index = 0;
        foreach(VisualElement i in BodyScalePlotPoints)
        {
            float x = index/256;
            float y = (Mathf.Pow(x,BodyScalePower)*(1-BodyScaleMin) + BodyScaleMin)*256f;
            i.style.bottom = y - 2;
            index++;
        }
        float m = controller.PrimaryBody.radius;
        foreach(Body i in controller.Bodies)
        {
            i.BodyScale = Mathf.Pow(m*i.radius,BodyScalePower)/Mathf.Pow(m,2*BodyScalePower)*(1-BodyScaleMin) + BodyScaleMin;
        }
    }

    private void editbodydone(ClickEvent evt)
    {
        editoractive = false;
        body_editor.style.display = DisplayStyle.None;
        body_view.style.display = DisplayStyle.Flex;
        if(newbody)
        {
            NewBodyEntry();
        }
        editorbody.bodyentry.button.Q<VisualElement>("body-picture").style.backgroundImage = new StyleBackground(Background.FromRenderTexture(editorbody.rt));
        editorbody.bodyentry.button.Q<Label>("body-name").text = editorbody.Name;
        editorbody = null;
        newbody = false;
    }

    private void NewBodyEntry()
    {
        BodyEntry bodyentry = new BodyEntry(editorbody,bodyentrybutton,this);
        root.Q<VisualElement>("Body-List").Add(bodyentry.button);
        editorbody.bodyentry = bodyentry;
    }

    private void editbodydelete(ClickEvent evt)
    {
        editoractive = false;
        body_editor.style.display = DisplayStyle.None;
        body_view.style.display = DisplayStyle.Flex;
        if(!newbody)
        {
            root.Q<VisualElement>("Body-List").Remove(editorbody.bodyentry.button);
        }
        controller.Bodies.Remove(editorbody);
        Destroy(editorbody.gameObject);
        editorbody = null;
        controller.resetsim();
    }

    private void CreateBody(ClickEvent evt)
    {
        editorbody = Instantiate(template);
        InitializeBodyEditor();
        newbody = true;
        controller.Bodies.Add(editorbody);
        controller.resetsim();
    }

    private void ToggleRightSidebar(ClickEvent evt)
    {
        if(enablerightsidebar)
        {
            enablerightsidebar = false;
            rightsidebar.RemoveFromClassList("right-sidebar-open");
            rightsidebar.AddToClassList("right-sidebar-closed");
            open_bodymenu.text = "<";
        }
        else
        {
            enablerightsidebar = true;
            rightsidebar.RemoveFromClassList("right-sidebar-closed");
            rightsidebar.AddToClassList("right-sidebar-open");
            open_bodymenu.text = ">";
        }
    }

    private void ToggleLeftSidebar(ClickEvent evt)
    {
        if(enableleftsidebar)
        {
            enableleftsidebar = false;
            leftsidebar.RemoveFromClassList("left-sidebar-open");
            leftsidebar.AddToClassList("left-sidebar-closed");
            open_settings.text = ">";
        }
        else
        {
            enableleftsidebar = true;
            leftsidebar.RemoveFromClassList("left-sidebar-closed");
            leftsidebar.AddToClassList("left-sidebar-open");
            open_settings.text = "<";
        }
    }

    public void InitializeBodyEditor()
    {
        viewwindow.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(editorbody.rt));
        namefield.value = editorbody.Name;
        redslider.value = editorbody.color.r*255;
        greenslider.value = editorbody.color.g*255;
        blueslider.value = editorbody.color.b*255;
        radiusfield.value = editorbody.radius;
        massfield.value = editorbody.mass;
        pradiusfield.value = editorbody.r_i;
        pvelocityfield.value = editorbody.v_i;
        editoractive = true;
        body_view.style.display = DisplayStyle.None;
        body_editor.style.display = DisplayStyle.Flex;
    }

    // Update is called once per frame
    void Update()
    {
        if(editoractive)
        {
            editorbody.Name = namefield.value;
            editorbody.name = namefield.value;
            color = new Color(redvalue/255, greenvalue/255, bluevalue/255, 1f);
            editorbody.color = color;
            editorbody.radius = bodyradius;
            editorbody.mass = mass;
            editorbody.r_i = periapsisradius;
            editorbody.v_i = periapsisvelocity;
        }
    }
}
