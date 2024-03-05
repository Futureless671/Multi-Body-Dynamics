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
    private Button new_body;
    private VisualElement body_view;
    private VisualElement body_editor;
    private Button body_editor_done;
    private Button body_editor_delete;
    private VisualElement sidebar;
    private bool enablesidebar = true;
    // Start is called before the first frame update
    void Start()
    {
        controller = FindObjectOfType<SimController>();
        root = GetComponent<UIDocument>().rootVisualElement;
        open_settings = root.Q<Button>("open-button");
        new_body = root.Q<Button>("New-Body");
        sidebar = root.Q<VisualElement>("Sidebar");
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

        open_settings.RegisterCallback<ClickEvent>(ToggleSidebar);
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

    private void ToggleSidebar(ClickEvent evt)
    {
        if(enablesidebar)
        {
            enablesidebar = false;
            sidebar.RemoveFromClassList("sidebar-open");
            sidebar.AddToClassList("sidebar-closed");
            open_settings.text = "<";
        }
        else
        {
            enablesidebar = true;
            sidebar.RemoveFromClassList("sidebar-closed");
            sidebar.AddToClassList("sidebar-open");
            open_settings.text = ">";
        }
    }

    public void InitializeBodyEditor()
    {
        viewwindow.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(editorbody.rt));
        namefield.value = editorbody.Name;
        redslider.value = editorbody.color.r*255;
        greenslider.value = editorbody.color.g*255;
        blueslider.value = editorbody.color.b*255;
        radiusfield.value = editorbody.radius/1000;
        massfield.value = editorbody.mass;
        pradiusfield.value = editorbody.r_i/1000;
        pvelocityfield.value = editorbody.v_i/1000;
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
            editorbody.radius = bodyradius*1000;
            editorbody.mass = mass;
            editorbody.r_i = periapsisradius*1000;
            editorbody.v_i = periapsisvelocity*1000;
        }
    }
}
