using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SimulationUI : MonoBehaviour
{
    private Button open_settings;
    private VisualElement sidebar;
    private bool enablesidebar = true;
    // Start is called before the first frame update
    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        open_settings = root.Q<Button>("open-button");
        sidebar = root.Q<VisualElement>("Sidebar");

        open_settings.RegisterCallback<ClickEvent>(ToggleSidebar);
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
