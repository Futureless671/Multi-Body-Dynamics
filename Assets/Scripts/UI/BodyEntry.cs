using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BodyEntry
{
    public VisualElement button;
    public Body body;
    public SimulationUI simui;
    public BodyEntry(Body body, VisualTreeAsset template, SimulationUI simui)
    {
        button = template.Instantiate();
        this.body = body;
        this.simui = simui;

        button.Q<Button>().RegisterCallback<ClickEvent>(editbody);
    }
    private void editbody(ClickEvent evt)
    {
        simui.editorbody = body;
        simui.InitializeBodyEditor();
    }
}
