using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HarmonyLib;

namespace SolastaKoreanMod
{
    internal static class Patch
    {
        [HarmonyPatch(typeof(NotificationItem), "Bind")]
        internal static class NotificationItem_Bind_Patch
        {
            public static void Postfix(GuiLabel ___titleLabel, GuiLabel ___descriptionLabel)
            {
                ___titleLabel.TMP_Text.autoSizeTextContainer = true;
                ___descriptionLabel.TMP_Text.autoSizeTextContainer = true;
            }
        }

        [HarmonyPatch(typeof(DialogChoiceItem), "Bind")]
        internal static class DialogChoiceItem_Bind_Patch
        {
            public static void Postfix(GuiLabelHighlighter ___labelHighlighter)
            {
                ___labelHighlighter.TargetLabel.TMP_Text.overflowMode = TMPro.TextOverflowModes.Overflow;
            }
        }
    }
}
