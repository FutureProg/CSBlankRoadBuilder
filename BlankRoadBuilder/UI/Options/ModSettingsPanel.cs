using AlgernonCommons.UI;

using BlankRoadBuilder.Domain;

using ColossalFramework;
using ColossalFramework.UI;

using ICities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine;

namespace BlankRoadBuilder.UI.Options;

public class ModSettingsPanel : UIPanel
{
    public ModSettingsPanel()
    {
        try
        {
            var tabStrip = AutoTabstrip.AddTabstrip(this, 0f, 0f, OptionsPanelManager<ModSettingsPanel>.PanelWidth, OptionsPanelManager<ModSettingsPanel>.PanelHeight, out _, tabHeight: 32f);

            new GeneralOptions(tabStrip, 0);
            new LaneSizeOptions(tabStrip, 1);
            new MarkingsOptions(tabStrip, 2);
            new FillerOptions(tabStrip, 3);

            tabStrip.selectedIndex = -1;
            tabStrip.selectedIndex = 0;
        }
        catch (Exception ex) { Debug.LogError(ex); }
    }
}
